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