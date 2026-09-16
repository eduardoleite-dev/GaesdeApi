namespace GaesdeApi;

public static class Messages
{
    public static class Auth
    {
        public const string InvalidCredentials = "Credenciais inválidas";
    }

    public static class Users
    {
        public const string CreateConflict = "E-mail já cadastrado ou nível de acesso inválido.";
        public const string UpdateNotFound = "Usuário não encontrado, e-mail já cadastrado ou nível de acesso inválido.";
    }

    public static class Categories
    {
        public const string CreateConflict = "Nome já cadastrado ou inválido.";
        public const string UpdateNotFound = "Categoria não encontrada ou nome já cadastrado.";
    }

    public static class Courses
    {
        public const string CreateConflict = "Curso inválido, slug já cadastrado, categoria inválida ou usuário não é professor.";
        public const string UpdateNotFound = "Curso não encontrado, slug já cadastrado ou dados inválidos.";
    }

    public static class Enrollments
    {
        public const string CreateConflict = "Usuário ou curso inválido, ou a matrícula já existe.";
        public const string InvalidProgressOrNotFound = "Progresso inválido ou matrícula não encontrada.";
    }

    public static class Contents
    {
        public const string CreateConflict = "Conteúdo inválido ou ordem já utilizada neste módulo.";
        public const string UpdateNotFound = "Conteúdo não encontrado ou dados inválidos.";
    }

    public static class Questions
    {
        public const string CreateConflict = "Questão inválida, quiz inexistente ou ordem já utilizada.";
        public const string UpdateNotFound = "Questão não encontrada ou dados inválidos.";
    }

    public static class Modules
    {
        public const string CreateConflict = "Módulo inválido, curso inexistente ou ordem já utilizada.";
        public const string UpdateNotFound = "Módulo não encontrado ou dados inválidos.";
    }

    public static class Hello
    {
        public const string Public = "Hello World";
        public const string Private = "Hello World Autenticado!";
        public const string Administrator = "Hello Administrador";
        public const string Professor = "Hello Professor";
        public const string Student = "Hello Aluno";
        public const string Seller = "Hello Vendedor";
    }
}