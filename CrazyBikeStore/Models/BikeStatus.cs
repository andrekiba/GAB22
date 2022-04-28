using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CrazyBikeStore.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BikeStatus
    {
        [EnumMember(Value = "available")]
        Available = 1,
        [EnumMember(Value = "pending")]
        Pending = 2,
        [EnumMember(Value = "sold")]
        Sold = 3
    }
}