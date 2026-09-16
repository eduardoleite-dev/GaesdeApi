# GaesdeApi

API REST para uma plataforma educacional. O sistema gerencia usuários, cursos, módulos, conteúdos, quizzes, questões, matrículas, submissões de atividades e comentários.

## Stack

- .NET 10
- ASP.NET Core Web API
- MongoDB Atlas
- JWT Bearer Authentication
- Cloudinary para imagens e PDFs
- Swagger/OpenAPI
- xUnit, Moq e Coverlet

## Pré-requisitos

- .NET SDK 10
- MongoDB ou MongoDB Atlas
- Conta Cloudinary para upload de arquivos
- PowerShell no Windows para usar `build.ps1`

## Configuração

Variáveis esperadas:

```text
MongoDbSettings__ConnectionString
JwtSettings__SecretKey
CloudinarySettings__CloudName
CloudinarySettings__ApiKey
CloudinarySettings__ApiSecret
```

Exemplo local usando PowerShell:

```powershell
$env:MongoDbSettings__ConnectionString = "mongodb://localhost:27017/GaesdeApi"
$env:JwtSettings__SecretKey = "chave-local-com-no-minimo-32-caracteres"
$env:CloudinarySettings__CloudName = "seu-cloud-name"
$env:CloudinarySettings__ApiKey = "sua-api-key"
$env:CloudinarySettings__ApiSecret = "seu-api-secret"
```

As variáveis com `__` correspondem à estrutura hierárquica do `appsettings.json`.

## Executar localmente

Na raiz do projeto:

```powershell
dotnet restore
dotnet run
```

URLs de desenvolvimento:

- HTTP: `http://localhost:5116`
- HTTPS: `https://localhost:7123`
- Swagger: `http://localhost:5116/`

A aplicação também configura a porta `8080` no `Program.cs` quando executada em ambientes de deploy.

## Build e testes

O pipeline padrão está em [build.ps1](build.ps1):

```powershell
.\build.ps1
```

O script executa:

1. `clean` da API e dos testes
2. `restore` da API e dos testes
3. `build` da API e dos testes
4. `test` da suíte xUnit

Execução direta dos testes:

```powershell
dotnet test .\Test\GaesdeApi.Tests.csproj
```

Cobertura:

```powershell
dotnet test .\Test\GaesdeApi.Tests.csproj --collect:"XPlat Code Coverage"
```

## Autenticação

A API usa JWT Bearer. Faça login em:

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "username": "usuarioMaster",
  "password": "62270208"
}
```

O token contém:

- `nameidentifier`: ID do usuário
- `nivel_acesso`: nível numérico
- `role`: nome do nível de acesso

Níveis disponíveis:

| Valor | Nível |
| ---: | --- |
| 0 | Administrador |
| 2 | Professor |
| 3 | Aluno |
| 4 | Vendedor |

Use o token nas requisições protegidas:

```http
Authorization: Bearer SEU_TOKEN
```

> Em produção, altere a credencial padrão do administrador e não mantenha segredos no arquivo de configuração.

## Principais recursos

### Usuários

```text
GET    /api/users
GET    /api/users/me
GET    /api/users/{id}
POST   /api/users
PUT    /api/users/{id}
DELETE /api/users/{id}
```

### Categorias

```text
GET    /api/categories
GET    /api/categories/{id}
POST   /api/categories
PUT    /api/categories/{id}
DELETE /api/categories/{id}
```

### Cursos

```text
GET    /api/courses
GET    /api/courses/mine
GET    /api/courses/catalog
GET    /api/courses/{id}
POST   /api/courses
PUT    /api/courses/{id}
PATCH  /api/courses/{id}/submit-review
PATCH  /api/courses/{id}/publish
PATCH  /api/courses/{id}/archive
DELETE /api/courses/{id}
```

Regras principais:

- Professor cria e edita os próprios cursos.
- O instrutor é obtido do token.
- O curso começa como `Draft`.
- O professor envia o curso para `Review`.
- O administrador publica ou arquiva.
- Alunos e vendedores consultam o catálogo publicado.

### Estrutura educacional

```text
GET/POST/PUT/DELETE /api/modules
GET/POST/PUT/DELETE /api/contents
GET/POST/PUT/DELETE /api/quizzes
GET/POST/PUT/DELETE /api/questions
GET/POST/PUT/DELETE /api/questionoptions
```

Relacionamento:

```text
Course
  -> Modules
      -> Contents
          -> Quiz
              -> Questions
                  -> QuestionOptions
```

Conteúdos suportados:

- `Video`
- `Text`
- `Pdf`
- `Quiz`
- `Assignment`

### Matrículas e avaliações

```text
GET    /api/enrollments
GET    /api/enrollments/me
GET    /api/enrollments/{id}
POST   /api/enrollments
PATCH  /api/enrollments/{id}/progress
PATCH  /api/enrollments/{id}/status/{status}
DELETE /api/enrollments/{id}
```

```text
GET/POST/PUT/DELETE /api/useranswers
GET/POST/PUT/DELETE /api/questionoptions
```

O usuário autenticado é usado automaticamente nas matrículas, respostas e submissões. Não envie `userId` pela URL para consultar os próprios dados.

### Submissões de atividades

```text
GET    /api/assignmentsubmissions
GET    /api/assignmentsubmissions/{id}
POST   /api/assignmentsubmissions
PATCH  /api/assignmentsubmissions/{id}/grade
DELETE /api/assignmentsubmissions/{id}
```

A atividade precisa ser um conteúdo do tipo `Assignment`. Apenas o professor responsável pelo curso ou administrador pode corrigir.

### Comentários

```text
GET    /api/comments
GET    /api/comments/{id}
POST   /api/comments
PUT    /api/comments/{id}
DELETE /api/comments/{id}
POST   /api/comments/{id}/reactions
DELETE /api/comments/{id}/reactions
POST   /api/comments/{id}/archive
DELETE /api/comments/{id}/archive
```

Comentários podem ser do tipo `Course` ou `Chat`, possuem destinatários, respostas, anexos, reações e arquivamento individual.

## Upload de mídia

O Cloudinary é acessado por:

```http
POST /api/media/upload
Content-Type: multipart/form-data
```

Campo obrigatório:

```text
file
```

Formatos aceitos:

- JPG
- PNG
- WEBP
- GIF
- PDF

Limite: `20 MB`.

O retorno contém `url` e `publicId`. Use a URL nos campos de capa, vídeo, PDF ou submissão de atividade.

Exclusão de mídia, apenas para administradores:

```http
DELETE /api/media/{publicId}?isPdf=false
```

## Paginação e filtros

As listagens retornam:

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalItems": 0,
  "totalPages": 0
}
```

Parâmetros comuns:

```text
?page=1&pageSize=20
```

O tamanho máximo de página é `100`.

Filtros relevantes:

- Users: `search`, `accessLevel`
- Categories: `search`
- Courses: `status`, `level`, `search`
- Modules: `courseId`, `search`
- Contents: `moduleId`, `type`, `freePreview`
- Questions: `quizId`, `type`
- QuestionOptions: `questionId`, `isCorrect`
- Enrollments: `status`, `courseId`
- UserAnswers: `attemptId`, `questionId`, `isCorrect`
- AssignmentSubmissions: `contentId`, `enrollmentId`, `graded`
- Comments: `courseId`, `type`

## Organização do projeto

```text
Config/        Configuração de MongoDB, JWT, Swagger e DI
Controllers/   Endpoints HTTP
DTOs/          Contratos de entrada e saída
Models/        Entidades e enums
Services/      Regras de negócio
Utils/         Mensagens, autorização e paginação
Test/          Testes xUnit
```

A estrutura do banco está documentada em [database.dbml](database.dbml).

## Convenções de desenvolvimento

- Nunca retorne `PasswordHash` em DTOs.
- Use o token para identificar o usuário autenticado.
- Valide relacionamentos antes de persistir dados.
- Mantenha datas em UTC.
- Use exclusão lógica quando o model possuir `DeletedAt`.
- Centralize mensagens em `Utils/Messages.cs`.
- Registre novos services em `Config/DependencyInjectionConfig.cs`.
- Adicione testes para novos endpoints e regras de negócio.
- Execute `build.ps1` antes de publicar alterações.
