using System.Text.Json.Serialization;

namespace GaesdeApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourseLevel
{
    Beginner,
    Intermediate,
    Advanced
}