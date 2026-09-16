# Como Construir e Publicar uma Web API .NET 10 com MongoDB e Azure

Este tutorial documenta a arquitetura, o desenvolvimento e a publicação de uma Web API em **.NET 10**, integrada ao **MongoDB Atlas**, protegida por autenticação **JWT** e hospedada no **Azure App Service**.

## Visão geral da arquitetura

A aplicação organiza suas responsabilidades em camadas:

```text
PortfolioApi/
├── Config/          # Configurações e métodos de extensão
├── Controllers/     # Endpoints HTTP
├── DTOs/            # Objetos de entrada e saída da API
├── Models/          # Entidades persistidas no MongoDB
└── Services/        # Regras de negócio e acesso aos dados
```

O arquivo `Program.cs` permanece responsável por orquestrar as configurações e montar o pipeline HTTP.

## 1. Criar o projeto e instalar as dependências

Verifique se o SDK do .NET 10 está instalado:

```powershell
dotnet --version
```

Crie o projeto Web API e entre na pasta criada:

dotnet new webapi -n PortfolioApi
Set-Location PortfolioApi
```

Adicione os pacotes utilizados pela aplicação:

```powershell
dotnet add package BCrypt.Net-Next --version 4.2.0
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.11
dotnet add package MongoDB.Driver --version 3.11.0
dotnet add package Swashbuckle.AspNetCore --version 10.2.3
```

Crie as pastas usadas para organizar o código:

```powershell
New-Item -ItemType Directory -Force -Path Config, Controllers, DTOs, Models, Services
New-Item -ItemType Directory -Force -Path Services/Interfaces
```

Restaure as dependências e confirme que o projeto compila:

```powershell
dotnet restore
dotnet build
```

Para executar localmente:

```powershell
dotnet run
```

Com a aplicação em execução, acesse a URL exibida no terminal e abra `/swagger`.

## 2. Estruturação e configuração da aplicação

Para manter o código organizado, as responsabilidades de inicialização são separadas em classes estáticas com métodos de extensão, dentro da pasta `Config`.

| Arquivo | Responsabilidade |
| --- | --- |
| `SwaggerConfig.cs` | Configura o Swagger/OpenAPI com autenticação via token Bearer (JWT). |
| `MongoConfig.cs` | Lê a conexão do MongoDB e registra `IMongoClient` e `IMongoDatabase` como singletons. |
| `JwtConfig.cs` | Configura a autenticação e a validação de tokens JWT. |
| `DependencyInjectionConfig.cs` | Registra os serviços de negócio com ciclo de vida `Scoped`. |

## 3. Configuração do MongoDB Atlas

Utilizamos o driver oficial do MongoDB para C#. Para funcionar tanto localmente quanto no Azure, a string de conexão pode ser lida usando diferentes chaves de configuração:

```csharp
var connectionString = configuration["ConnectionStrings__MongoDB"]
                                             ?? configuration["ConnectionStrings:MongoDB"]
                                             ?? configuration["MongoDbSettings:ConnectionString"]
                                             ?? throw new InvalidOperationException(
                                                     "String de conexão não encontrada.");
```

No Azure App Service, as configurações aninhadas do .NET usam dois sublinhados (`__`).

## 4. Autenticação híbrida com JWT

O sistema de login atende a dois cenários:

1. **Conta mestre:** usuário administrativo fixo para inicialização ou acesso de emergência.
2. **Usuários do MongoDB:** usuários cadastrados no banco, identificados por e-mail ou nome.

As senhas dos usuários dinâmicos são validadas com hashes seguros usando a biblioteca `BCrypt.Net-Next`.

O endpoint de login retorna um `LoginResponseDto` com:

- o token JWT, válido por duas horas;
- a data e hora de expiração (`expiresAt`);
- o nome e o ID do usuário autenticado.

## 5. CRUD de usuários

O gerenciamento de usuários é dividido entre `Models`, `DTOs`, `Services` e `Controllers`.

### Entidade

A entidade `User` é mapeada para a collection `Users` do MongoDB e oferece suporte a `ObjectId`.

### Paginação e filtros

O tipo genérico `PagedResult<T>` permite:

- controlar a página atual com `page`;
- definir a quantidade de itens com `pageSize`;
- pesquisar por nome ou e-mail usando `searchTerm`;
- realizar buscas sem diferenciar maiúsculas e minúsculas.

### Proteção das rotas

O `UsersController` é protegido pelo atributo `[Authorize]`. As requisições devem enviar o token no cabeçalho HTTP:

```http
Authorization: Bearer <seu-token-jwt>
```

## 6. Deploy no Azure App Service

O deploy é automatizado pela Azure CLI e pelo script `deploy.ps1`.

### 5.1 Configurar variáveis no Azure

Defina as configurações do App Service com `az webapp config appsettings set`:

```powershell
az webapp config appsettings set `
    --name Portifolio `
    --resource-group gaesde_group `
    --settings "JwtSettings__SecretKey=$env:JWT_SECRET_KEY"

az webapp config appsettings set `
    --name Portifolio `
    --resource-group gaesde_group `
    --settings "ConnectionStrings__MongoDB=$env:MONGODB_CONNECTION_STRING"
```

Os valores devem vir de um mecanismo seguro de secrets. Não substitua as variáveis por credenciais reais dentro do código ou deste tutorial.

### 5.2 Publicar a aplicação

O script de deploy executa as seguintes etapas:

1. Compila e publica o projeto em modo `Release` com `dotnet publish`.
2. Compacta os arquivos publicados em um arquivo `.zip`.
3. Envia o pacote para o endpoint Kudu do Azure App Service.
4. Aguarda o warm-up e a inicialização da aplicação .NET 10 na porta `8080`.

## Resultado

Depois da publicação, a aplicação fica disponível no Azure App Service com:

- **Swagger:** documentação interativa acessível na URL principal do App Service;
- **rotas públicas:** teste de conexão e login em `/api/Auth/login`;
- **rotas privadas:** endpoints protegidos por JWT e operações CRUD no MongoDB Atlas.