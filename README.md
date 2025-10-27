KRTBankAccount API

API para gerenciamento de contas do banco KRT, implementada em .NET 8 seguindo princípios DDD, SOLID e Clean Code, com cache Redis para otimização de consultas.

-------------------------------------------------------------------------------

Tecnologias

- .NET 8
- ASP.NET Core Web API (MVC)
- Dapper para acesso a SQL Server
- SQL Server (Docker)
- Redis para cache distribuído
- xUnit + Moq + FluentAssertions + Bogus para testes unitários
- Docker (opcional para SQL Server e Redis)

-------------------------------------------------------------------------------

Estrutura do Projeto

KRTBankAccount/
│

├── KRTBankAccount.Domain/         # Entidades, Value Objects, Interfaces

├── KRTBankAccount.Application/    # Serviços, DTOs

├── KRTBankAccount.Infrastructure/ # Repositórios, Cache Redis, Dapper

├── KRTBankAccount.API/            # Controllers, Program.cs

└── KRTBankAccount.Tests/          # Testes unitários

-------------------------------------------------------------------------------

Funcionalidades

CRUD de contas bancárias com os seguintes dados:

- ID (GUID)
- HolderName (string)
- CPF (string)
- IsActive (bool)

Endpoints

Método  | Endpoint                 | Descrição
--------|--------------------------|------------------------------
POST    | /api/accounts            | Cria uma nova conta
GET     | /api/accounts/{id}       | Retorna conta pelo ID (com cache Redis)
PUT     | /api/accounts/{id}       | Atualiza conta existente e remove cache
DELETE  | /api/accounts/{id}       | Desativa conta existente

-------------------------------------------------------------------------------

Banco de Dados

- SQL Server 2022 (Docker)
- Scripts de criação de tabela:

CREATE TABLE Accounts (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    HolderName NVARCHAR(150) NOT NULL,
    CPF NVARCHAR(11) NOT NULL,
    IsActive BIT NOT NULL
);

-------------------------------------------------------------------------------

Cache Redis

- Armazena consultas de contas já realizadas no mesmo dia para reduzir custo de consultas no SQL Server.
- Chave utilizada: account:{id}
- TTL configurado: 24h

-------------------------------------------------------------------------------

Testes Unitários

- xUnit + Moq + Bogus
- Cobertura:

1. POST: cria conta com validação de CPF
2. GET: busca conta do cache ou do banco, armazena no cache se necessário
3. PUT: atualiza conta, trata status case-insensitive, remove cache
4. DELETE: desativa conta existente, não altera se conta não existe

Exemplo de execução:

dotnet test

-------------------------------------------------------------------------------

Docker

SQL Server

docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=MyStrongP@ssw0rd" -p 1433:1433 --name krt-sql -d mcr.microsoft.com/mssql/server:2022-latest

Redis

docker run -p 6379:6379 --name krt-redis -d redis

-------------------------------------------------------------------------------

Como Rodar

1. Configurar appsettings.json com string de conexão SQL Server e Redis.
2. Restaurar pacotes:

dotnet restore

3. Rodar aplicação:

dotnet run --project KRTBankAccount.API

4. Acesse Swagger UI:

https://localhost:7292/swagger/index.html

-------------------------------------------------------------------------------

Observações

- Segue padrões DDD: separação clara entre Domínio, Aplicação e Infraestrutura.
- Segue princípios SOLID e Clean Code.
- Cache Redis otimiza consultas e reduz custo no SQL Server.
- Testes unitários garantem comportamento esperado em todas operações.

-------------------------------------------------------------------------------

Autor

Ravele Rodrigues Almeida
Email: ravelerodrigues@gmail.com
