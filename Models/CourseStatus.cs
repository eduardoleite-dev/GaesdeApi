using System.Text.Json.Serialization;

namespace GaesdeApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourseStatus
{
    Draft,
    Review,
    Published,
    Archived
}