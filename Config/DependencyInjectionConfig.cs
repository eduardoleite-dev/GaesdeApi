using GaesdeApi.Services;
using GaesdeApi.Services.Interfaces;

namespace GaesdeApi.Config;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddDependencyInjectionConfig(this IServiceCollection services)
    {
        services.AddScoped<IMongoTestService, MongoTestService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<IEnrollmentAccessService, EnrollmentAccessService>();
        services.AddScoped<IContentCompletionService, ContentCompletionService>();
        services.AddScoped<IQuizAttemptService, QuizAttemptService>();
        services.AddScoped<IContentService, ContentService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<IQuestionOptionService, QuestionOptionService>();
        services.AddScoped<IQuizService, QuizService>();
        services.AddScoped<IUserAnswerService, UserAnswerService>();
        services.AddScoped<ICommentService, CommentService>();
        services.AddScoped<IAssignmentSubmissionService, AssignmentSubmissionService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        return services;
    }
}