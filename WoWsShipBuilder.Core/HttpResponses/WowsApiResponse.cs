using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace WoWsShipBuilder.Core.HttpResponses
{
    public class WowsApiResponse<T> where T : IWowsApiResponseData
    {
        public string Status { get; set; } = "error";

        public WowsApiResponseMeta Meta { get; set; } = new();

        public WowsApiResponseError? Error { get; set; }

        public Dictionary<long, T?> Data { get; set; } = new();
    }

    [SuppressMessage("Ordering", "SA1201", Justification = "Ordering can be ignored here.")]
    public interface IWowsApiResponseData
    {
    }

    public class WowsApiResponseMeta
    {
        public int Count { get; set; }

        [JsonProperty("page_total")]
        public int? TotalPages { get; set; }

        public int? Total { get; set; }

        public int? Limit { get; set; }

        public int? Page { get; set; }
    }

    public class WowsApiResponseError
    {
        public string? Field { get; set; }

        public string? Message { get; set; }

        public string? Code { get; set; }

        public string? Value { get; set; }
    }

    /// <summary>
    /// Exception thrown when something wrong happens with WG API response.
    /// </summary>
    [Serializable]
    public class WowsApiException : Exception
    {
        public WowsApiException()
        {
        }

        public WowsApiException(string message)
            : base(message)
        {
        }

        public WowsApiException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public WowsApiException(string message, string id)
            : this(message)
        {
            this.Id = id;
        }

        public WowsApiException(string message, string? field, string? answer, string? code, string? value)
            : this(message)
        {
            this.Field = field;
            this.Answer = answer;
            this.Code = code;
            this.Value = value;
        }

        /// <summary>
        /// Gets the IDs of the items returning incorrect response.
        /// </summary>
        public string? Id { get; }

        /// <summary>
        /// Gets the section of the WG API response returning the incorrect response.
        /// </summary>
        public string? Field { get; }

        /// <summary>
        /// Gets the error message returned by WG API.
        /// </summary>
        public string? Answer { get; }

        /// <summary>
        /// Gets the error code returned by WG API.
        /// </summary>
        public string? Code { get; }

        /// <summary>
        /// Gets the value returning the incorrect response.
        /// </summary>
        public string? Value { get; }
    }

    #region Ships and Camos Images

    [SuppressMessage("Ordering", "SA1201", Justification = "Ordering can be ignored here.")]
    public enum ImageSize
    {
        Small,
        Large,
        Medium,
        Contour,
    }

    public enum ImageType
    {
        Ship,
        Camo,
    }

    public class ImageData : IWowsApiResponseData
    {
        [JsonProperty("images")]
        public Dictionary<ImageSize, string>? ShipImages { get; set; }

        [JsonProperty("image")]
        public string? CamoImage { get; set; }
    }

    #endregion
}
