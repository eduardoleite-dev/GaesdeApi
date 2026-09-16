using System.Text.Json.Serialization;

namespace GaesdeApi.Models;

public enum AccessLevel
{
    Administrador = 0,
    Professor = 2,
    Aluno = 3,
    Vendedor = 4
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourseStatus
{
    Draft,
    Review,
    Published,
    Archived
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum CourseLevel
{
    Beginner,
    Intermediate,
    Advanced
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EnrollmentStatus
{
    PendingPayment,
    Active,
    Dropped,
    Completed
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ContentType
{
    Video,
    Text,
    Pdf,
    Quiz,
    Assignment
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum QuestionType
{
    MultipleChoice,
    TrueFalse,
    Essay,
    Matching
}