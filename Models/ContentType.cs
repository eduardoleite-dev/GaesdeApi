using System.Text.Json.Serialization;

namespace GaesdeApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentType
{
    Video,
    Text,
    Pdf,
    Quiz,
    Assignment
}