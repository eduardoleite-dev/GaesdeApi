using System.Text.Json.Serialization;

namespace GaesdeApi.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnrollmentStatus
{
    PendingPayment,
    Active,
    Dropped,
    Completed
}