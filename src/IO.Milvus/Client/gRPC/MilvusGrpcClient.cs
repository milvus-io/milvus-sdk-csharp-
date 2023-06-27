﻿using Grpc.Core;
using Grpc.Net.Client;
using IO.Milvus.Diagnostics;
using IO.Milvus.Grpc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IO.Milvus.Client.gRPC;

/// <summary>
/// Milvus gRPC client
/// </summary>
public sealed partial class MilvusGrpcClient : IMilvusClient
{
    /// <summary>
    /// The constructor for the <see cref="MilvusGrpcClient"/>
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="port"></param>
    /// <param name="name"></param>
    /// <param name="password"></param>
    /// <param name="grpcChannel"></param>
    /// <param name="callOptions"></param>
    /// <param name="log"></param>
    public MilvusGrpcClient(
        string endpoint,
        int port = 19530,
        string name = "root",
        string password = "milvus",
        GrpcChannel grpcChannel = null,
        CallOptions? callOptions = default,
        ILogger log = null)
    {
        Verify.NotNull(endpoint, "Milvus client cannot be null or empty");

        var address = SanitizeEndpoint(endpoint,port);

        this._log = log ?? NullLogger<MilvusGrpcClient>.Instance;
        if (grpcChannel is not null)
        {
            this._grpcChannel = grpcChannel;
            this._ownsGrpcChannel = false;
        }
        else
        {
            this._grpcChannel = GrpcChannel.ForAddress(address);
            this._ownsGrpcChannel = true;
        }

        _callOptions = callOptions ?? new CallOptions(
            new Metadata()
            {
                { "authorization", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{name}:{password}")) }
            });

        _grpcClient = new MilvusService.MilvusServiceClient(_grpcChannel);
    }

    ///<inheritdoc/>
    public string Address => _grpcChannel.Target;

    ///<inheritdoc/>
    public async Task<MilvusHealthState> HealthAsync(CancellationToken cancellationToken = default)
    {
        _log.LogDebug("Check if connection is health");

        var response = await _grpcClient.CheckHealthAsync(new CheckHealthRequest(), _callOptions.WithCancellationToken(cancellationToken));
        if (!response.IsHealthy)
        {
            foreach (var reason in response.Reasons)
            {
                _log.LogWarning(reason);
            }
        }

        return new MilvusHealthState(response.IsHealthy, response.Status.Reason,response.Status.ErrorCode);
    }

    ///<inheritdoc/>
    public override string ToString()
    {
        return $"{{{nameof(MilvusGrpcClient)}:{Address}}}";
    }

    #region Private ===============================================================================
    private readonly ILogger _log;
    private readonly GrpcChannel _grpcChannel;
    private readonly CallOptions _callOptions;
    private readonly MilvusService.MilvusServiceClient _grpcClient;
    private readonly bool _ownsGrpcChannel;

    private static Uri SanitizeEndpoint(string endpoint, int? port)
    {
        Verify.IsValidUrl(nameof(endpoint), endpoint, false, true, false);

        UriBuilder builder = new(endpoint);
        if (port.HasValue) { builder.Port = port.Value; }

        return builder.Uri;
    }

    /// <summary>
    /// Close milvus connection.
    /// </summary>
    public void Dispose()
    {
        if (_ownsGrpcChannel)
        {
            _grpcChannel.Dispose();
        }
    }
    #endregion
}