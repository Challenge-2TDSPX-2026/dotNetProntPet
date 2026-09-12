# 🐾 ProntPet API

API RESTful para gerenciamento de prontuários veterinários, desenvolvida em **ASP.NET Core 8** com banco de dados **Oracle**. Permite o cadastro e controle completo de tutores, pets, vacinas, prontuários médicos, consultas e clínicas veterinárias — com camadas de observabilidade (Health Checks, logging estruturado, tracing e métricas) e uma suíte de testes automatizados cobrindo Services e Controllers.

---

## Equipe

| Nome | RM |
|------|----|
| Anthony De Souza Henriques | RM566188 |
| Guilherme Santos Fonseca | RM564232 |
| Gustavo Araújo Da Silva | RM566526 |

---

## 📋 Sumário

- [Descrição do Projeto](#-descrição-do-projeto)
- [Tecnologias](#-tecnologias)
- [Instalação e Execução](#-instalação-e-execução)
- [Documentação das Rotas](#-documentação-das-rotas)
  - [Tutores](#tutores)
  - [Pets](#pets)
  - [Vacinações](#vacinações)
  - [Prontuários Médicos](#prontuários-médicos)
  - [Clínicas](#clínicas)
  - [Consultas](#consultas)
- [Monitoramento e Observabilidade](#-monitoramento-e-observabilidade)
  - [Health Checks](#health-checks)
  - [Logging Estruturado](#logging-estruturado)
  - [Tracing e Métricas (OpenTelemetry)](#tracing-e-métricas-opentelemetry)
- [Testes Automatizados](#-testes-automatizados)
  - [Executando pelo Visual Studio](#executando-pelo-visual-studio)
  - [Executando pelo VS Code](#executando-pelo-vs-code)

---

## 📖 Descrição do Projeto

O **ProntPet** é um sistema de gerenciamento veterinário que centraliza informações sobre tutores (donos de pets) e seus animais, permitindo o registro completo do histórico de saúde de cada pet — incluindo vacinas aplicadas, prontuários médicos e consultas realizadas em clínicas.

O sistema foi construído seguindo o padrão de arquitetura MVC com separação em camadas de Models, DTOs e Controllers, utilizando Entity Framework Core para mapeamento objeto-relacional e Oracle como banco de dados.

### Funcionalidades principais

- Cadastro e gestão de **tutores** com dados únicos (CPF, e-mail, telefone)
- Cadastro e gestão de **pets** vinculados aos seus tutores
- Registro do **histórico de vacinação** de cada pet
- Criação de **prontuários médicos** com tipo sanguíneo, alergias, doenças crônicas e microchip
- Registro de **consultas veterinárias** vinculadas a prontuários e clínicas
- Cadastro de **clínicas veterinárias** com CNPJ único
- Documentação interativa via **Swagger UI**
- **Health Checks** de liveness e readiness (incluindo conectividade com o Oracle)
- **Logging estruturado** (Serilog) com correlação de requisições e saída em console e arquivo
- **Tracing distribuído e métricas customizadas** via OpenTelemetry
- **Testes automatizados** (unitários e de integração), organizados em projetos próprios

### Arquitetura em camadas

A aplicação segue uma separação em camadas dentro do próprio projeto:

- **Controllers** — recebem a requisição HTTP e traduzem o resultado do Service em uma resposta (200/201/204/400/404), sem regra de negócio.
- **Services** — concentram toda a regra de negócio (validações, checagens de existência, orquestração do `AppDbContext`), e são o alvo dos testes unitários.
- **Models** — entidades do EF Core, mapeadas para as tabelas Oracle (`DB_*`).
- **Dtos** — objetos de entrada/saída da API, com conversão explícita para os Models (`ToEntity()`).
- **Infrastructure** — configuração centralizada de DbContext, Health Checks e OpenTelemetry (`AddInfrastructure`).
- **Middleware** — `GlobalExceptionHandler` (tratamento global de exceções).
- **Diagnostics** — constantes de nomeação para as fontes de tracing/métricas customizadas.

---

## 🛠 Tecnologias

**API**
- [.NET 8](https://dotnet.microsoft.com/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core 8](https://learn.microsoft.com/en-us/ef/core/)
- [Oracle.EntityFrameworkCore 8.21.121](https://www.nuget.org/packages/Oracle.EntityFrameworkCore)
- [Swashbuckle (Swagger) 6.6.2](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

**Observabilidade**
- [Microsoft.Extensions.Diagnostics.HealthChecks](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/health-checks) + `HealthChecks.EntityFrameworkCore` — Health Checks
- [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore) (`Serilog.Sinks.Console`, `Serilog.Sinks.File`) — logging estruturado
- [OpenTelemetry](https://opentelemetry.io/docs/languages/net/) (`OpenTelemetry.Extensions.Hosting`, `Instrumentation.AspNetCore`, `Instrumentation.Http`, `Exporter.Console`) — tracing e métricas

**Testes**
- [xUnit](https://xunit.net/) — framework de testes
- [Moq](https://github.com/devlooped/moq) — mocking de `ILogger`/`IMeterFactory`
- [Microsoft.EntityFrameworkCore.InMemory](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/) — persistência isolada nos testes
- [Microsoft.AspNetCore.Mvc.Testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests) (`WebApplicationFactory`) — testes de integração

---

## 🚀 Instalação e Execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado
- Instância do **Oracle Database** acessível (compatível com a versão 19c)
- [Oracle Client](https://www.oracle.com/database/technologies/instant-client.html) configurado (se necessário)

### 1. Clone o repositório

```bash
git clone https://github.com/Challenge-2TDSPX-2026/dotNetProntPet.git
cd dotNetProntPet
```

### 2. Configure a connection string

Crie o arquivo `appsettings.json` na raiz do projeto (ele está no `.gitignore` por segurança). Use o arquivo `appsettings.json.example` como base:

```bash
cp appsettings.json.example appsettings.json
```

Edite o `appsettings.json` preenchendo as credenciais do seu banco Oracle:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "OracleConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=SEU_DATA_SOURCE;"
  }
}
```

### 3. Restaure as dependências

**CLI do .NET:**
```bash
dotnet restore
```
 
**Visual Studio (PMC):**
```powershell
# Não é necessário no Visual Studio, mas pode rodar se preferir
Update-Package -Reinstall
```

> Para abrir o PMC no Visual Studio: `Tools` → `NuGet Package Manager` → `Package Manager Console`

### 4. Execute as migrations

Aplique as migrations para criar as tabelas no banco de dados:

**CLI do .NET:**
```bash
dotnet ef database update
```
 
**Visual Studio (Package Manager Console):**
```powershell
Update-Database
```

### 5. Execute a aplicação

**CLI do .NET:**
```bash
dotnet run
```
 
**Visual Studio:**
- Pressione `F5` ou clique no botão de play (▶) na barra de ferramentas
- Selecione o perfil desejado (`https` ou `http`)

A API estará disponível em:
- HTTP: `http://localhost:5257`
- HTTPS: `https://localhost:7224`

### 6. Acesse o Swagger

Com a aplicação rodando em modo desenvolvimento, a documentação interativa estará disponível em:

```
http://localhost:5257/swagger
```
 
ou
 
```
https://localhost:7224/swagger
```

---

## 📡 Documentação das Rotas

### Tutores

Base URL: `/api/Tutor`

| Método | Rota | Descrição | Corpo da requisição | Respostas |
|--------|------|-----------|---------------------|-----------|
| `GET` | `/api/Tutor` | Lista todos os tutores (sem expor senha) | — | `200 OK` |
| `GET` | `/api/Tutor/{id}` | Busca um tutor pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Tutor` | Cadastra um novo tutor | `TutorRequest` | `201 Created` / `400 Bad Request` |
| `PUT` | `/api/Tutor/{id}` | Atualiza os dados de um tutor | `TutorRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Tutor/{id}` | Remove um tutor | — | `204 No Content` / `404 Not Found` |

**TutorRequest** (corpo JSON):
```json
{
  "name": "João Silva",
  "cpf": "123.456.789-00",
  "phone": "(11) 99999-9999",
  "email": "joao@email.com",
  "password": "senha123",
  "address": "Rua Exemplo, 123 - São Paulo/SP"
}
```

> ⚠️ CPF, e-mail e telefone devem ser únicos no sistema.

---

### Pets

Base URL: `/api/Pet`

| Método | Rota | Descrição | Corpo da requisição | Respostas |
|--------|------|-----------|---------------------|-----------|
| `GET` | `/api/Pet/tutor/{idTutor}` | Lista os pets de um tutor | — | `200 OK` |
| `GET` | `/api/Pet/{id}` | Busca um pet pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Pet` | Cadastra um novo pet | `PetRequest` | `201 Created` / `404 Not Found` / `400 Bad Request` |
| `PUT` | `/api/Pet/{id}` | Atualiza os dados de um pet | `PetUpdateRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Pet/{id}` | Remove um pet | — | `204 No Content` / `404 Not Found` |

**PetRequest** (corpo JSON para criação):
```json
{
  "idTutor": 1,
  "name": "Rex",
  "species": "Cachorro",
  "breed": "Labrador",
  "birthDate": "2020-05-10",
  "weight": 28.5,
  "sex": "Macho"
}
```

**PetUpdateRequest** (corpo JSON para atualização — `name`, `species` e `sex` são obrigatórios):
```json
{
  "name": "Rex",
  "species": "Cachorro",
  "breed": "Labrador",
  "birthDate": "2020-05-10",
  "weight": 30.0,
  "sex": "Macho"
}
```

> ⚠️ O peso não pode ser negativo. O tutor informado em `idTutor` deve existir.

---

### Vacinações

Base URL: `/api/Vaccination`

| Método | Rota | Descrição | Corpo da requisição | Respostas |
|--------|------|-----------|---------------------|-----------|
| `GET` | `/api/Vaccination/pet/{idPet}` | Lista as vacinações de um pet | — | `200 OK` |
| `GET` | `/api/Vaccination/{id}` | Busca uma vacinação pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Vaccination` | Registra uma nova vacinação | `VaccinationRequest` | `201 Created` / `404 Not Found` / `400 Bad Request` |
| `PUT` | `/api/Vaccination/{id}` | Atualiza os dados de uma vacinação | `VaccinationRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Vaccination/{id}` | Remove uma vacinação | — | `204 No Content` / `404 Not Found` |

**VaccinationRequest** (corpo JSON):
```json
{
  "idPet": 1,
  "vaccineName": "V10",
  "applicationDate": "2024-01-15",
  "expirationDate": "2025-01-15",
  "lot": "L20240115"
}
```

> ⚠️ A data de expiração deve ser posterior à data de aplicação. O pet informado em `idPet` deve existir.

---

### Prontuários Médicos

Base URL: `/api/MedicalRecord`

| Método | Rota | Descrição | Corpo da requisição | Respostas |
|--------|------|-----------|---------------------|-----------|
| `GET` | `/api/MedicalRecord/pet/{idPet}` | Busca o prontuário de um pet | — | `200 OK` |
| `GET` | `/api/MedicalRecord/{id}` | Busca um prontuário pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/MedicalRecord` | Cadastra um novo prontuário | `MedicalRecordRequest` | `201 Created` / `404 Not Found` |
| `PUT` | `/api/MedicalRecord/{id}` | Atualiza os dados de um prontuário | `MedicalRecordRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/MedicalRecord/{id}` | Remove um prontuário | — | `204 No Content` / `404 Not Found` |

**MedicalRecordRequest** (corpo JSON):
```json
{
  "idPet": 1,
  "bloodType": "A+",
  "allergies": "Dipirona",
  "chronicDiseases": "Displasia coxofemoral",
  "isCastrated": true,
  "microchipCode": "985112000123456"
}
```

> ⚠️ Cada pet pode ter apenas um prontuário. O código de microchip deve ser único. O tipo sanguíneo (`bloodType`) é obrigatório e limitado a 5 caracteres.

---

### Clínicas

Base URL: `/api/Clinic`

| Método | Rota | Descrição | Corpo da requisição | Respostas |
|--------|------|-----------|---------------------|-----------|
| `GET` | `/api/Clinic` | Lista todas as clínicas | — | `200 OK` |
| `GET` | `/api/Clinic/{id}` | Busca uma clínica pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Clinic` | Cadastra uma nova clínica | `ClinicRequest` | `201 Created` |
| `PUT` | `/api/Clinic/{id}` | Atualiza os dados de uma clínica | `ClinicRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Clinic/{id}` | Remove uma clínica | — | `204 No Content` / `404 Not Found` |

**ClinicRequest** (corpo JSON):
```json
{
  "name": "Clínica VetSaúde",
  "cnpj": "12.345.678/0001-99",
  "address": "Av. Paulista, 1000 - São Paulo/SP"
}
```

> ⚠️ O CNPJ deve ser único no sistema.

---

### Consultas

Base URL: `/api/Consultation`

| Método | Rota | Descrição | Corpo da requisição | Respostas |
|--------|------|-----------|---------------------|-----------|
| `GET` | `/api/Consultation/medical-record/{idRecord}` | Lista as consultas de um prontuário | — | `200 OK` |
| `GET` | `/api/Consultation/{id}` | Busca uma consulta pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Consultation` | Registra uma nova consulta | `ConsultationRequest` | `201 Created` / `404 Not Found` |
| `PUT` | `/api/Consultation/{id}` | Atualiza os dados de uma consulta | `ConsultationRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Consultation/{id}` | Remove uma consulta | — | `204 No Content` / `404 Not Found` |

**ConsultationRequest** (corpo JSON):
```json
{
  "idMedicalRecord": 1,
  "idClinic": 1,
  "consultationDate": "2024-06-10",
  "symptoms": "Vômito e apatia",
  "diagnosis": "Gastroenterite",
  "observations": "Retornar em 7 dias para reavaliação"
}
```

> ⚠️ O prontuário (`idMedicalRecord`) e a clínica (`idClinic`) informados devem existir. Os campos `symptoms`, `diagnosis` e `observations` são opcionais.

---

## 🩺 Monitoramento e Observabilidade

### Health Checks

A API expõe dois endpoints de Health Check, pensados para cenários diferentes de monitoramento (ex: liveness/readiness probes em Kubernetes, ou um monitor de uptime simples):

| Endpoint | Verifica | Uso típico |
|---|---|---|
| `GET /health/live` | Apenas se o processo da API está de pé (não depende de nada externo) | Liveness probe — reiniciar o container se falhar |
| `GET /health/ready` | Conectividade real com o banco Oracle (`AddDbContextCheck<AppDbContext>`) | Readiness probe — só rotear tráfego se estiver saudável |

Exemplo de resposta de `GET /health/ready` (formato `HealthChecks.UI.Client`):

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "oracle-database": {
      "status": "Healthy",
      "duration": "00:00:00.0110000",
      "tags": ["ready"]
    }
  }
}
```

Se o Oracle estiver indisponível, o campo `status` muda para `"Unhealthy"` e o endpoint responde com HTTP `503 Service Unavailable`.

### Logging Estruturado

O logging é feito com **Serilog**, configurado em `Program.cs`:

- **Saída em Console** — acompanhamento em tempo real durante o desenvolvimento.
- **Saída em Arquivo** — `logs/prontpet-YYYYMMDD.log`, com rotação diária e retenção dos últimos 14 arquivos (pasta ignorada no `.gitignore`).
- **Níveis usados**: `Information` (operações de negócio bem-sucedidas — criação, atualização, remoção), `Warning` (regras de negócio violadas — registro não encontrado, validação falhou) e `Error` (exceções não tratadas, capturadas pelo `GlobalExceptionHandler`).
- **Correlação de requisições** — o `CorrelationIdMiddleware` gera (ou reaproveita, se o cliente enviar) um `X-Correlation-Id`, devolve o mesmo valor no header de resposta, e injeta esse Id em toda linha de log emitida durante o processamento daquela requisição. Isso permite filtrar, no arquivo de log, tudo que aconteceu numa requisição específica:

```bash
grep "3fa85f64-5717-4562-b3fc-2c963f66afa6" logs/prontpet-20260910.log
```

- Toda requisição HTTP concluída também gera uma linha de log automática (via `UseSerilogRequestLogging`), com método, path, status code e duração.

### Tracing e Métricas (OpenTelemetry)

Configurado em `Infrastructure/DependencyInjection.cs`:

- **Tracing** — cada operação de negócio nos `Services` (`CreatePet`, `UpdateTutor`, etc.) gera um *span* nomeado, com tags relevantes (`pet.id`, `tutor.id`, ...) e status de erro quando a regra de negócio falha. Somado a isso, `AddAspNetCoreInstrumentation()` gera spans automáticos para cada requisição HTTP.
- **Métricas de negócio** — contadores customizados por entidade (`pets_created_total`, `tutors_created_total`, `vaccinations_created_total`, etc.), alguns com tags de categoria (ex: `species`, `vaccine_name`).
- **Tempo de resposta e taxa de erro** — cobertos automaticamente pela instrumentação padrão do ASP.NET Core (`http.server.request.duration`), sem necessidade de métricas manuais.
- **Exportação** — por padrão, tudo é exportado para o **Console** (`AddConsoleExporter()`), prático para desenvolvimento local. Em produção, troque por `AddOtlpExporter()` apontando para um coletor (Jaeger, Grafana Tempo, Application Insights, etc.) — é só trocar essa linha, o resto da instrumentação não muda.

---

## 🧪 Testes Automatizados

O projeto tem duas suítes de teste, seguindo o padrão **AAA (Arrange, Act, Assert)** e a nomenclatura `MetodoTestado_Cenario_ResultadoEsperado`:

| Projeto | O que testa | Como isola dependências |
|---|---|---|
| `ProntPet.UnitTests` | Regras de negócio dos `Services`, isoladamente | `ILogger`/`IMeterFactory` mockados com **Moq**; `AppDbContext` real, mas com provider **EF Core InMemory** (um banco novo por teste) |
| `ProntPet.IntegrationTests` | Fluxo HTTP completo dos `Controllers` (rota → binding → Service → EF Core) | `WebApplicationFactory<Program>`, com o `AppDbContext` também trocado para **EF Core InMemory** (compartilhado entre as classes via `CollectionFixture`, pra não depender de um Oracle real) |

> ✅ Nenhuma das duas suítes precisa de uma instância Oracle real disponível — todos os testes rodam de forma isolada e determinística.

### Executando pelo Visual Studio

1. Abra a solução `ProntPet.sln`.
2. Se o **Test Explorer** não estiver visível: menu `Test` → `Test Explorer` (ou `Ctrl+E, T`).
3. O Visual Studio descobre os testes automaticamente após o build (pode levar alguns segundos na primeira vez).
4. Para rodar tudo: clique em **Run All Tests** (▶ no topo do Test Explorer).
5. Para rodar só uma suíte ou uma classe específica: clique com o botão direito sobre o projeto/classe desejado (ex: `PetServiceTests`) → `Run`.
6. Para depurar um teste específico: botão direito sobre o teste → `Debug`.

> Dica: agrupe por projeto (ícone de agrupamento no topo do Test Explorer) para visualizar `ProntPet.UnitTests` e `ProntPet.IntegrationTests` separadamente.

### Executando pelo VS Code

**Pré-requisito:** extensão [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) (recomendada, traz um Test Explorer integrado) — ou apenas o [.NET SDK](https://dotnet.microsoft.com/download) para rodar tudo via terminal.

**Via terminal integrado (`` Ctrl+` ``):**

```bash
# Rodar TODOS os testes da solução (unitários + integração)
dotnet test

# Rodar apenas os testes unitários
dotnet test ProntPet.UnitTests

# Rodar apenas os testes de integração
dotnet test ProntPet.IntegrationTests

# Rodar um teste (ou grupo de testes) específico, filtrando pelo nome
dotnet test --filter "FullyQualifiedName~PetServiceTests"

# Rodar com saída detalhada (útil para ver qual teste falhou e por quê)
dotnet test --logger "console;verbosity=detailed"
```

**Via Test Explorer do C# Dev Kit:**

1. Instale a extensão C# Dev Kit e abra a pasta do projeto no VS Code.
2. Clique no ícone de frasco (⚗️/Testing) na barra lateral esquerda.
3. Os testes de `ProntPet.UnitTests` e `ProntPet.IntegrationTests` aparecem organizados em árvore, por projeto/classe.
4. Use os botões ▶ para rodar todos, ou passe o mouse sobre um teste/classe específica para rodar ou depurar individualmente.

---

## 🗂 Estrutura do Banco de Dados

```
DB_TUTOR (tutores)
  └── DB_PET (pets, FK → DB_TUTOR)
        ├── DB_VACCINATION (vacinações, FK → DB_PET)
        └── DB_MEDICAL_RECORD (prontuários, FK → DB_PET, 1:1)
              └── DB_CONSULTATION (consultas, FK → DB_MEDICAL_RECORD + DB_CLINIC)

DB_CLINIC (clínicas)
  └── DB_CONSULTATION (FK → DB_CLINIC)
```
