﻿using System.Text.Json.Serialization;

namespace IO.Milvus.ApiSchema;

/// <summary>
/// Load a collection for search
/// </summary>
internal sealed class LoadCollectionRequest
{
    /// <summary>
    /// Collection Name
    /// </summary>
    /// <remarks>
    /// The collection name you want to load
    /// </remarks>
    [JsonPropertyName("collection_name")]
    public string CollectionName { get; set; }

    /// <summary>
    /// The replica number to load, default by 1
    /// </summary>
    [JsonPropertyName("replica_number")]
    public int ReplicaNumber { get; set; } = 1;

    /// <summary>
    /// Database name
    /// </summary>
    /// <remarks>
    /// available in <c>Milvus 2.2.9</c>
    /// </remarks>
    [JsonPropertyName("db_name")]
    public string DbName { get; set; }
}
