# 🐾 ProntPet API

API RESTful para gerenciamento de prontuários veterinários, desenvolvida em **ASP.NET Core 8**. Utiliza **persistência poliglota**: **Oracle** (via Entity Framework Core) para os dados relacionais e **MongoDB** (NoSQL, via Repository Pattern) para a linha do tempo clínica dos pets. A API é protegida por **JWT**, oferece **paginação, ordenação, filtros e HATEOAS** nas consultas, e conta com observabilidade (Health Checks, logging estruturado, tracing e métricas) e uma suíte de testes automatizados.

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
  - [⚠️ Checklist antes de rodar](#️-checklist-antes-de-rodar)
  - [2. Suba o MongoDB com Docker](#2-suba-o-mongodb-com-docker-obrigatório)
  - [3. Configure as credenciais](#3-configure-as-credenciais-no-appsettingsjson-obrigatório)
- [Autenticação (JWT)](#-autenticação-jwt)
- [Paginação, Ordenação, Filtros e HATEOAS](#-paginação-ordenação-filtros-e-hateoas)
- [Documentação das Rotas](#-documentação-das-rotas)
  - [Autenticação](#autenticação)
  - [Tutores](#tutores)
  - [Pets](#pets)
  - [Vacinações](#vacinações)
  - [Prontuários Médicos](#prontuários-médicos)
  - [Clínicas](#clínicas)
  - [Consultas](#consultas)
  - [Timeline do Pet (MongoDB)](#timeline-do-pet-mongodb)
- [Monitoramento e Observabilidade](#-monitoramento-e-observabilidade)
- [Testes Automatizados](#-testes-automatizados)
- [Estrutura dos Bancos de Dados](#-estrutura-dos-bancos-de-dados)
- [Solução de Problemas](#-solução-de-problemas)

---

## 📖 Descrição do Projeto

O **ProntPet** é um sistema de gerenciamento veterinário que centraliza informações sobre tutores (donos de pets) e seus animais, permitindo o registro completo do histórico de saúde de cada pet — incluindo vacinas aplicadas, prontuários médicos, consultas realizadas em clínicas e uma **linha do tempo clínica** (exames, procedimentos, medicações e anotações) armazenada em MongoDB.

### Funcionalidades principais

- Cadastro e gestão de **tutores** com dados únicos (CPF, e-mail, telefone) e **senha armazenada como hash**
- **Login com JWT** (e-mail e senha do tutor) e proteção de todas as rotas de negócio
- Cadastro e gestão de **pets** vinculados aos seus tutores
- Registro do **histórico de vacinação** de cada pet
- Criação de **prontuários médicos** com tipo sanguíneo, alergias, doenças crônicas e microchip
- Registro de **consultas veterinárias** vinculadas a prontuários e clínicas
- Cadastro de **clínicas veterinárias** com CNPJ único
- **Timeline clínica do pet em MongoDB** (documentos com tags e atributos livres)
- **Paginação, ordenação e filtros** em todas as listagens, com **links HATEOAS**
- Documentação interativa via **Swagger UI** (com botão *Authorize* para o token)
- **Health Checks** de liveness e readiness (Oracle e MongoDB)
- **Logging estruturado** (Serilog) com correlação de requisições
- **Tracing distribuído e métricas customizadas** via OpenTelemetry
- **Testes automatizados** (unitários e de integração)

### Arquitetura em camadas

```
Controllers ──► Services ──► AppDbContext (EF Core) ──► Oracle   (tutores, pets, vacinas, prontuários, consultas, clínicas)
                   │
                   └───────► IPetTimelineRepository ──► MongoDB   (timeline clínica do pet)
```

- **Controllers** — recebem a requisição HTTP, aplicam `[Authorize]`, traduzem o resultado do Service em resposta (200/201/204/400/401/404) e montam os **links HATEOAS**, sem regra de negócio.
- **Services** — concentram a regra de negócio (validações, filtros, ordenação, paginação, existência de registros). São o alvo dos testes unitários.
- **Repositories** — *Repository Pattern* para o MongoDB: `IRepository<T>` (contrato genérico), `MongoRepository<T>` (implementação concreta base) e `PetTimelineRepository` (consultas específicas com filtros e paginação).
- **Models** — entidades do EF Core (tabelas Oracle `DB_*`) e documento `PetTimelineEntry` (MongoDB).
- **Dtos** — objetos de entrada/saída (`*Request`, `*Response` com `Links`, `*QueryParameters` com paginação/filtros).
- **Common** — tipos reutilizáveis: `QueryParameters`, `PagedResult<T>`, `PagedResponse<T>`, `LinkDto`, `PaginationLinkBuilder`, `TutorPasswordHasher`.
- **Infrastructure** — DbContext Oracle, MongoDB, Health Checks, OpenTelemetry e autenticação JWT.
- **Middleware** — `CorrelationIdMiddleware` e `GlobalExceptionHandler`.
- **Diagnostics** — constantes de nomeação de tracing/métricas.

---

## 🛠 Tecnologias

**API**
- [.NET 8](https://dotnet.microsoft.com/) / [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core 8](https://learn.microsoft.com/en-us/ef/core/) + [Oracle.EntityFrameworkCore 8.21.121](https://www.nuget.org/packages/Oracle.EntityFrameworkCore)
- [MongoDB.Driver](https://www.mongodb.com/docs/drivers/csharp/current/) — persistência NoSQL
- [Microsoft.AspNetCore.Authentication.JwtBearer 8](https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer) — autenticação JWT
- ASP.NET Core Identity `PasswordHasher` — hash de senha (PBKDF2)
- [Swashbuckle (Swagger) 6.6.2](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

**Infraestrutura**
- [Docker](https://www.docker.com/) + Docker Compose — MongoDB local (`mongo:7`)

**Observabilidade**
- Health Checks (`HealthChecks.EntityFrameworkCore`, `HealthChecks.UI.Client`)
- [Serilog.AspNetCore](https://github.com/serilog/serilog-aspnetcore) (Console + File)
- [OpenTelemetry](https://opentelemetry.io/docs/languages/net/) (tracing e métricas)

**Testes**
- [xUnit](https://xunit.net/), [Moq](https://github.com/devlooped/moq)
- [EF Core InMemory](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/) e [Microsoft.AspNetCore.Mvc.Testing](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)

---

## 🚀 Instalação e Execução

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (o **runtime 8** é obrigatório — ter apenas o .NET 9/10 não basta)
- [**Docker Desktop**](https://www.docker.com/products/docker-desktop/) (ou Docker Engine + Compose) — para o MongoDB
- Acesso a uma instância **Oracle Database** (compatível com 19c) e suas credenciais
- [Oracle Client](https://www.oracle.com/database/technologies/instant-client.html) configurado (se necessário)

### ⚠️ Checklist antes de rodar

> [!IMPORTANT]
> **Quem for rodar o projeto precisa, obrigatoriamente, de DUAS coisas além do código:**
>
> 1. 🐳 **Subir o container do MongoDB** com Docker (`docker compose up -d`) — [ver passo 2](#2-suba-o-mongodb-com-docker-obrigatório)
> 2. 🔑 **Criar o `appsettings.json` com as credenciais** (Oracle, MongoDB e chave JWT) — [ver passo 3](#3-configure-as-credenciais-no-appsettingsjson-obrigatório)
>
> O `appsettings.json` **não vem no repositório** (está no `.gitignore` por segurança). Sem ele, a API não sobe. Sem o container do Mongo, a API sobe, mas as rotas `/api/PetTimeline` falham e o `/health/ready` fica `Unhealthy`.

### 1. Clone o repositório

```bash
git clone https://github.com/Challenge-2TDSPX-2026/dotNetProntPet.git
cd dotNetProntPet
```

### 2. Suba o MongoDB com Docker (obrigatório)

O repositório já traz o `docker-compose.yml` na raiz (ao lado de `ProntPetSolution.slnx`). Com o Docker em execução, rode **na raiz do repositório**:

> [!IMPORTANT]
> ### 🐳 Comandos do container MongoDB
>
> ```bash
> # SUBIR o MongoDB em segundo plano (baixa a imagem na primeira vez)
> docker compose up -d
>
> # CONFERIR se está de pé (coluna STATUS deve mostrar "Up")
> docker ps --filter name=prontpet-mongo
>
> # Ver os logs
> docker compose logs -f mongo
>
> # PARAR o container (os dados são mantidos)
> docker compose stop
>
> # Subir de novo depois de parar
> docker compose start
>
> # REMOVER o container (os dados continuam no volume)
> docker compose down
>
> # REMOVER container E APAGAR TODOS OS DADOS do Mongo (volume)
> docker compose down -v
> ```

O container criado é o `prontpet-mongo` (imagem `mongo:7`), escutando em `localhost:27017`, com os dados guardados no volume `prontpet-mongo-data`. O arquivo `docker-compose.yml`:

```yaml
services:
  mongo:
    image: mongo:7
    container_name: prontpet-mongo
    restart: unless-stopped
    ports:
      - "27017:27017"
    volumes:
      - prontpet-mongo-data:/data/db

volumes:
  prontpet-mongo-data:
```

**Para conferir o que foi gravado no Mongo:**

```bash
docker exec -it prontpet-mongo mongosh --eval "db.getSiblingDB('prontpet').pet_timeline.find().pretty()"
```

> [!WARNING]
> **Erro `port is already allocated` / `Bind for 0.0.0.0:27017 failed`?** Outro processo (outro container Mongo ou um MongoDB instalado no Windows) já usa a porta 27017. Duas saídas:
> - **Liberar a porta:** `docker ps` (veja quem usa 27017) e `docker stop <container>`; ou pare o serviço do Windows (`Stop-Service MongoDB`, PowerShell como administrador).
> - **Usar outra porta:** no `docker-compose.yml` troque `"27017:27017"` por `"27018:27017"` **e** use `mongodb://localhost:27018` na `MongoDb:ConnectionString` do `appsettings.json`.
>
> Depois: `docker compose down` e `docker compose up -d`.

### 3. Configure as credenciais no `appsettings.json` (obrigatório)

> [!IMPORTANT]
> ### 🔑 Credenciais — o `appsettings.json` NÃO está no repositório
>
> Crie o arquivo **`ProntPet.API/appsettings.json`** a partir do modelo `appsettings.json.example` e **preencha as credenciais**:
>
> ```bash
> # Linux / macOS / Git Bash
> cp ProntPet.API/appsettings.json.example ProntPet.API/appsettings.json
> ```
> ```powershell
> # Windows (PowerShell)
> Copy-Item ProntPet.API\appsettings.json.example ProntPet.API\appsettings.json
> ```
>
> Depois edite o `ProntPet.API/appsettings.json`:
>
> ```json
> {
>   "Logging": {
>     "LogLevel": {
>       "Default": "Information",
>       "Microsoft.AspNetCore": "Warning"
>     }
>   },
>   "AllowedHosts": "*",
>   "ConnectionStrings": {
>     "OracleConnection": "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/orcl;"
>   },
>   "MongoDb": {
>     "ConnectionString": "mongodb://localhost:27017",
>     "DatabaseName": "prontpet"
>   },
>   "Jwt": {
>     "Key": "TROQUE-POR-UMA-CHAVE-SECRETA-COM-NO-MINIMO-32-CARACTERES",
>     "Issuer": "ProntPet.API",
>     "Audience": "ProntPet.Clients",
>     "ExpirationMinutes": 120
>   }
> }
> ```

| Seção | Campo | O que colocar |
|---|---|---|
| `ConnectionStrings` | `OracleConnection` | **Seu usuário e senha do Oracle** (`User Id`, `Password`) e o `Data Source` do seu banco. A senha diferencia maiúsculas de minúsculas. |
| `MongoDb` | `ConnectionString` | `mongodb://localhost:27017` (ou a porta que você mapeou no `docker-compose.yml`) |
| `MongoDb` | `DatabaseName` | `prontpet` |
| `Jwt` | `Key` | **Chave secreta própria**, com **no mínimo 32 caracteres** (veja abaixo como gerar) |
| `Jwt` | `Issuer` / `Audience` | Podem ficar como no exemplo |
| `Jwt` | `ExpirationMinutes` | Validade do token em minutos (1 a 1440; padrão 120) |

**Gerando uma chave JWT:**

```bash
openssl rand -base64 48                                   # Linux / macOS / Git Bash
```
```powershell
[Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))   # PowerShell 7+
```

> [!WARNING]
> - **Nunca faça commit** do `appsettings.json` nem compartilhe sua senha do Oracle ou a chave JWT (o arquivo já está no `.gitignore`).
> - Se `Jwt` estiver ausente ou com chave curta, a API **falha ao iniciar** com uma mensagem clara no log (`Configuração 'Jwt:Key' ausente ou curta demais...`).
> - Alternativa mais segura em desenvolvimento: **user-secrets**, que guardam as credenciais fora da pasta do projeto:
>   ```bash
>   dotnet user-secrets init --project ProntPet.API
>   dotnet user-secrets set "ConnectionStrings:OracleConnection" "User Id=SEU_USUARIO;Password=SUA_SENHA;Data Source=oracle.fiap.com.br:1521/orcl;" --project ProntPet.API
>   dotnet user-secrets set "Jwt:Key" "SUA-CHAVE-COM-32-OU-MAIS-CARACTERES" --project ProntPet.API
>   ```
>   Em produção, use variáveis de ambiente (ex.: `Jwt__Key`, `ConnectionStrings__OracleConnection`).

### 4. Restaure as dependências

```bash
dotnet restore
```

### 5. Execute as migrations (Oracle)

Cria as tabelas no Oracle:

```bash
# apenas na primeira vez, se ainda não tiver a ferramenta do EF Core:
dotnet tool install --global dotnet-ef

dotnet ef database update --project ProntPet.API
```

*(Visual Studio: `Tools` → `NuGet Package Manager` → `Package Manager Console` → `Update-Database`, com o projeto `ProntPet.API` como padrão.)*

> O MongoDB **não precisa de migrations**: o banco `prontpet` e a coleção `pet_timeline` são criados automaticamente no primeiro registro.

### 6. Execute a aplicação

```bash
dotnet run --project ProntPet.API
```

**Visual Studio:** `F5` (perfil `http` ou `https`).

A API estará disponível em:
- HTTP: `http://localhost:5257`
- HTTPS: `https://localhost:7224`

### 7. Acesse o Swagger

Em modo desenvolvimento:

```
http://localhost:5257/swagger
```

### 8. Verifique se está tudo pronto

Abra `http://localhost:5257/health/ready`. O status geral deve ser `Healthy`, com **`oracle-database`** e **`mongodb`** saudáveis. Se algum aparecer `Unhealthy`, veja a [Solução de Problemas](#-solução-de-problemas).

---

## 🔐 Autenticação (JWT)

Todas as rotas exigem um **token JWT**, exceto as públicas:

| Rota pública | Para quê |
|---|---|
| `POST /api/Tutor` | Cadastro do tutor (ponto de partida) |
| `POST /api/Auth/login` | Login e emissão do token |
| `GET /health/live`, `GET /health/ready` | Monitoramento |

O login é feito com o **e-mail e a senha do Tutor**. A senha é armazenada **somente como hash** (ASP.NET Core Identity `PasswordHasher`, PBKDF2 com sal) e nunca é retornada pela API.

### Fluxo (Swagger)

1. `POST /api/Tutor` — cadastre um tutor (e-mail, CPF e telefone **novos**).
2. `POST /api/Auth/login` — informe `email` e `password`; copie o valor de `token` da resposta.
3. Clique em **Authorize** (🔓 no topo do Swagger), cole **apenas o token** (o prefixo `Bearer` é automático) e confirme.
4. Chame qualquer rota protegida. Sem token (ou com token inválido/expirado) a resposta é **`401 Unauthorized`**.

### Fluxo (linha de comando)

```bash
# 1) Cadastrar o tutor (rota pública)
curl -X POST http://localhost:5257/api/Tutor \
  -H "Content-Type: application/json" \
  -d '{"name":"João Silva","cpf":"123.456.789-00","phone":"(11) 99999-9999","email":"joao@email.com","password":"senha123","address":"Rua Exemplo, 123"}'

# 2) Login -> guarda o token
TOKEN=$(curl -s -X POST http://localhost:5257/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"joao@email.com","password":"senha123"}' | jq -r .token)

# 3) Usar o token
curl "http://localhost:5257/api/Tutor?pageSize=5" -H "Authorization: Bearer $TOKEN"
```

```powershell
# PowerShell
$login = Invoke-RestMethod -Method Post -Uri http://localhost:5257/api/Auth/login `
  -ContentType "application/json" -Body '{"email":"joao@email.com","password":"senha123"}'

Invoke-RestMethod -Uri "http://localhost:5257/api/Tutor?pageSize=5" `
  -Headers @{ Authorization = "Bearer $($login.token)" }
```

**Resposta do login (`200 OK`):**

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2026-11-01T01:30:00Z",
  "tutorId": 1,
  "name": "João Silva",
  "email": "joao@email.com"
}
```

O token (HS256) carrega as claims `sub` (id do tutor), `email`, `name` e `jti`, e expira em `Jwt:ExpirationMinutes` (padrão 120 min).

> ⚠️ **Tutores cadastrados antes da introdução do hash** têm senha em texto puro no banco e **não conseguem fazer login** — cadastre novos tutores.
> ℹ️ O login não diferencia "e-mail inexistente" de "senha incorreta" (ambos `401`), para não revelar quais e-mails existem.

---

## 📑 Paginação, Ordenação, Filtros e HATEOAS

Todas as **listagens** aceitam os mesmos parâmetros de query, mais filtros específicos de cada recurso (listados na [documentação das rotas](#-documentação-das-rotas)).

| Parâmetro | Padrão | Descrição |
|---|---|---|
| `pageNumber` | `1` | Página desejada (valores menores que 1 viram 1) |
| `pageSize` | `10` | Itens por página (**máximo 50**; acima disso é ajustado para 50) |
| `sortBy` | depende do recurso | Campo de ordenação (campo desconhecido usa o padrão do recurso) |
| `ascending` | `true` | `true` crescente, `false` decrescente |

> 💡 Informe `sortBy` e `ascending` **juntos**. Recursos com ordenação padrão decrescente (vacinações, prontuários, consultas e timeline) assumem `ascending=false` quando você não o informa.

A ordenação sempre desempata pelo `Id`, então a paginação é **estável** (nenhum item se repete ou some entre páginas).

### Formato da resposta (`PagedResponse`)

Exemplo: `GET /api/Pet/tutor/1?species=cachorro&pageSize=10`

```json
{
  "items": [
    {
      "id": 1,
      "idTutor": 1,
      "name": "Rex",
      "species": "Cachorro",
      "breed": "Labrador",
      "birthDate": "2020-05-10T00:00:00",
      "weight": 28.5,
      "sex": "Macho",
      "links": [
        { "href": "http://localhost:5257/api/Pet/1", "rel": "self", "method": "GET" },
        { "href": "http://localhost:5257/api/Pet/1", "rel": "update_pet", "method": "PUT" },
        { "href": "http://localhost:5257/api/Pet/1", "rel": "delete_pet", "method": "DELETE" },
        { "href": "http://localhost:5257/api/Tutor/1", "rel": "tutor", "method": "GET" },
        { "href": "http://localhost:5257/api/Vaccination/pet/1", "rel": "vaccinations", "method": "GET" },
        { "href": "http://localhost:5257/api/MedicalRecord/pet/1", "rel": "medical_records", "method": "GET" },
        { "href": "http://localhost:5257/api/PetTimeline/pet/1", "rel": "timeline", "method": "GET" }
      ]
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "totalRecords": 1,
  "links": [
    {
      "href": "http://localhost:5257/api/Pet/tutor/1?species=cachorro&pageNumber=1&pageSize=10&ascending=True",
      "rel": "self",
      "method": "GET"
    }
  ]
}
```

### Links HATEOAS

- **Links da página** (`links` na raiz): `self`, `first_page`, `prev_page`, `next_page` e `last_page` (só aparecem quando existem). Eles **preservam os filtros, a ordenação e o tamanho da página**, então basta seguir o `href`.
- **Links de cada item** (`links` dentro do item): ações do recurso (`self`, `update_*`, `delete_*`) e **navegação entre recursos** — por exemplo, um pet aponta para seu tutor, vacinações, prontuários e timeline (Oracle → MongoDB); uma consulta aponta para o prontuário e a clínica.
- `GET` por id e `POST` também devolvem o recurso com seus `links`.

---

## 📡 Documentação das Rotas

> Legenda de acesso: 🔒 exige token JWT · 🔓 rota pública.
> Os corpos JSON usam `camelCase`. Datas em listas aceitam o formato `yyyy-MM-dd`.

### Autenticação

Base URL: `/api/Auth`

| Método | Rota | Acesso | Descrição | Corpo | Respostas |
|--------|------|:---:|-----------|-------|-----------|
| `POST` | `/api/Auth/login` | 🔓 | Autentica o tutor e devolve o token | `LoginRequest` | `200 OK` / `400 Bad Request` / `401 Unauthorized` |
| `GET` | `/api/Auth/me` | 🔒 | Dados do tutor autenticado (lidos do token) | — | `200 OK` / `401 Unauthorized` |

**LoginRequest:**
```json
{ "email": "joao@email.com", "password": "senha123" }
```

---

### Tutores

Base URL: `/api/Tutor`

| Método | Rota | Acesso | Descrição | Corpo | Respostas |
|--------|------|:---:|-----------|-------|-----------|
| `GET` | `/api/Tutor` | 🔒 | Lista tutores (paginado, sem expor senha) | — | `200 OK` |
| `GET` | `/api/Tutor/{id}` | 🔒 | Busca um tutor pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Tutor` | 🔓 | Cadastra um novo tutor | `TutorRequest` | `201 Created` / `400 Bad Request` |
| `PUT` | `/api/Tutor/{id}` | 🔒 | Atualiza um tutor (a senha informada é re-hasheada) | `TutorRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Tutor/{id}` | 🔒 | Remove um tutor | — | `204 No Content` / `404 Not Found` |

**Filtros e ordenação de `GET /api/Tutor`:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `name` | texto | Nome contém o texto (ignora maiúsculas) |
| `email` | texto | E-mail contém o texto (ignora maiúsculas) |
| `cpf` | texto | CPF contém o texto |
| `sortBy` | `name` (padrão), `email`, `cpf` | Campo de ordenação |

Exemplo: `/api/Tutor?name=maria&sortBy=email&ascending=false&pageSize=5`

**TutorRequest:**
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

| Método | Rota | Acesso | Descrição | Corpo | Respostas |
|--------|------|:---:|-----------|-------|-----------|
| `GET` | `/api/Pet/tutor/{idTutor}` | 🔒 | Lista os pets de um tutor (paginado) | — | `200 OK` |
| `GET` | `/api/Pet/{id}` | 🔒 | Busca um pet pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Pet` | 🔒 | Cadastra um novo pet | `PetRequest` | `201 Created` / `404 Not Found` / `400 Bad Request` |
| `PUT` | `/api/Pet/{id}` | 🔒 | Atualiza um pet | `PetUpdateRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Pet/{id}` | 🔒 | Remove um pet | — | `204 No Content` / `404 Not Found` |

**Filtros e ordenação de `GET /api/Pet/tutor/{idTutor}`:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `name` | texto | Nome contém o texto |
| `breed` | texto | Raça contém o texto |
| `species` | texto | Espécie **igual** ao texto (ignora maiúsculas) |
| `sex` | texto | Sexo **igual** ao texto (ignora maiúsculas) |
| `sortBy` | `name` (padrão), `species`, `breed`, `birthDate`, `weight` | Campo de ordenação |

Exemplo: `/api/Pet/tutor/1?species=gato&sortBy=weight&ascending=false`

**PetRequest** (criação):
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

**PetUpdateRequest** (atualização — `name`, `species` e `sex` obrigatórios):
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

| Método | Rota | Acesso | Descrição | Corpo | Respostas |
|--------|------|:---:|-----------|-------|-----------|
| `GET` | `/api/Vaccination/pet/{idPet}` | 🔒 | Lista as vacinações de um pet (paginado) | — | `200 OK` / `400 Bad Request` |
| `GET` | `/api/Vaccination/{id}` | 🔒 | Busca uma vacinação pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Vaccination` | 🔒 | Registra uma nova vacinação | `VaccinationRequest` | `201 Created` / `404 Not Found` / `400 Bad Request` |
| `PUT` | `/api/Vaccination/{id}` | 🔒 | Atualiza uma vacinação | `VaccinationRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Vaccination/{id}` | 🔒 | Remove uma vacinação | — | `204 No Content` / `404 Not Found` |

**Filtros e ordenação de `GET /api/Vaccination/pet/{idPet}`:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `vaccineName` | texto | Nome da vacina contém o texto |
| `lot` | texto | Lote contém o texto |
| `appliedFrom` / `appliedTo` | data `yyyy-MM-dd` | Período de aplicação (inclusive; `appliedFrom` > `appliedTo` retorna `400`) |
| `expired` | `true` / `false` | `true` = vencidas; `false` = dentro da validade |
| `sortBy` | `applicationDate` (padrão, decrescente), `expirationDate`, `vaccineName`, `lot` | Campo de ordenação |

Exemplo: `/api/Vaccination/pet/1?expired=false&appliedFrom=2026-01-01&pageSize=5`

**VaccinationRequest:**
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

| Método | Rota | Acesso | Descrição | Corpo | Respostas |
|--------|------|:---:|-----------|-------|-----------|
| `GET` | `/api/MedicalRecord/pet/{idPet}` | 🔒 | Busca o prontuário de um pet (resposta paginada com 0 ou 1 item) | — | `200 OK` |
| `GET` | `/api/MedicalRecord/{id}` | 🔒 | Busca um prontuário pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/MedicalRecord` | 🔒 | Cadastra um novo prontuário | `MedicalRecordRequest` | `201 Created` / `404 Not Found` |
| `PUT` | `/api/MedicalRecord/{id}` | 🔒 | Atualiza um prontuário | `MedicalRecordRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/MedicalRecord/{id}` | 🔒 | Remove um prontuário | — | `204 No Content` / `404 Not Found` |

**Filtros e ordenação de `GET /api/MedicalRecord/pet/{idPet}`:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `bloodType` | texto | Tipo sanguíneo **igual** ao texto (ignora maiúsculas) |
| `isCastrated` | `true` / `false` | Pets castrados ou não |
| `search` | texto | Contém o texto em alergias ou doenças crônicas |
| `sortBy` | `lastUpdate` (padrão, decrescente), `bloodType` | Campo de ordenação |

**MedicalRecordRequest:**
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

> ⚠️ Cada pet pode ter apenas um prontuário (por isso a listagem por pet retorna no máximo um item). O código de microchip deve ser único. O tipo sanguíneo é obrigatório e limitado a 5 caracteres.

---

### Clínicas

Base URL: `/api/Clinic`

| Método | Rota | Acesso | Descrição | Corpo | Respostas |
|--------|------|:---:|-----------|-------|-----------|
| `GET` | `/api/Clinic` | 🔒 | Lista as clínicas (paginado) | — | `200 OK` |
| `GET` | `/api/Clinic/{id}` | 🔒 | Busca uma clínica pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Clinic` | 🔒 | Cadastra uma nova clínica | `ClinicRequest` | `201 Created` |
| `PUT` | `/api/Clinic/{id}` | 🔒 | Atualiza uma clínica | `ClinicRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Clinic/{id}` | 🔒 | Remove uma clínica | — | `204 No Content` / `404 Not Found` |

**Filtros e ordenação de `GET /api/Clinic`:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `name` | texto | Nome contém o texto |
| `cnpj` | texto | CNPJ contém o texto |
| `address` | texto | Endereço contém o texto |
| `sortBy` | `name` (padrão), `cnpj`, `address` | Campo de ordenação |

**ClinicRequest:**
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

| Método | Rota | Acesso | Descrição | Corpo | Respostas |
|--------|------|:---:|-----------|-------|-----------|
| `GET` | `/api/Consultation/medical-record/{idRecord}` | 🔒 | Lista as consultas de um prontuário (paginado) | — | `200 OK` / `400 Bad Request` |
| `GET` | `/api/Consultation/{id}` | 🔒 | Busca uma consulta pelo ID | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/Consultation` | 🔒 | Registra uma nova consulta | `ConsultationRequest` | `201 Created` / `404 Not Found` |
| `PUT` | `/api/Consultation/{id}` | 🔒 | Atualiza uma consulta | `ConsultationRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/Consultation/{id}` | 🔒 | Remove uma consulta | — | `204 No Content` / `404 Not Found` |

**Filtros e ordenação de `GET /api/Consultation/medical-record/{idRecord}`:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `idClinic` | inteiro | Consultas de uma clínica |
| `from` / `to` | data `yyyy-MM-dd` | Período (inclusive; `from` > `to` retorna `400`) |
| `search` | texto | Contém o texto em sintomas, diagnóstico ou observações |
| `sortBy` | `consultationDate` (padrão, decrescente), `idClinic`, `diagnosis` | Campo de ordenação |

**ConsultationRequest:**
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

> ⚠️ O prontuário (`idMedicalRecord`) e a clínica (`idClinic`) devem existir. `symptoms`, `diagnosis` e `observations` são opcionais.

---

### Timeline do Pet (MongoDB)

Base URL: `/api/PetTimeline` — eventos e anotações clínicas de cada pet, armazenados na coleção `pet_timeline` do **MongoDB**. O `idPet` referencia o pet no Oracle (e é validado na criação).

| Método | Rota | Acesso | Descrição | Corpo | Respostas |
|--------|------|:---:|-----------|-------|-----------|
| `GET` | `/api/PetTimeline/pet/{idPet}` | 🔒 | Lista a timeline de um pet (paginado) | — | `200 OK` / `400 Bad Request` |
| `GET` | `/api/PetTimeline/{id}` | 🔒 | Busca um registro pelo ID (ObjectId de 24 caracteres) | — | `200 OK` / `404 Not Found` |
| `POST` | `/api/PetTimeline` | 🔒 | Cria um registro na timeline | `PetTimelineRequest` | `201 Created` / `404 Not Found` / `400 Bad Request` |
| `PUT` | `/api/PetTimeline/{id}` | 🔒 | Atualiza um registro | `PetTimelineUpdateRequest` | `204 No Content` / `404 Not Found` |
| `DELETE` | `/api/PetTimeline/{id}` | 🔒 | Remove um registro | — | `204 No Content` / `404 Not Found` |

**Filtros e ordenação de `GET /api/PetTimeline/pet/{idPet}`:**

| Parâmetro | Tipo | Descrição |
|---|---|---|
| `type` | enum | `Observation`, `Consultation`, `Exam`, `Procedure`, `Vaccination` ou `Medication` |
| `tag` | texto | Registros que contenham a tag (igualdade exata) |
| `search` | texto | Contém o texto no título ou na descrição (ignora maiúsculas) |
| `from` / `to` | data ou data/hora (UTC) | Período de `occurredAt`. Se `to` trouxer só a data, o **dia inteiro** é incluído. `from` > `to` retorna `400` |
| `sortBy` | `occurredAt` (padrão, decrescente), `title`, `type`, `createdAt` | Campo de ordenação |

Exemplo: `/api/PetTimeline/pet/1?type=Exam&tag=rotina&from=2026-10-01&to=2026-10-31&pageSize=5`

**PetTimelineRequest** (criação):
```json
{
  "idPet": 1,
  "type": "Exam",
  "title": "Hemograma completo",
  "description": "Resultados dentro da normalidade",
  "tags": ["rotina", "sangue"],
  "attributes": { "laboratorio": "VetLab", "peso": "12,4kg" },
  "occurredAt": "2026-10-20T14:30:00Z"
}
```

**PetTimelineUpdateRequest** (atualização — mesmos campos, sem `idPet`):
```json
{
  "type": "Exam",
  "title": "Hemograma completo (revisado)",
  "description": "Resultados dentro da normalidade",
  "tags": ["rotina"],
  "attributes": { "laboratorio": "VetLab" },
  "occurredAt": "2026-10-20T14:30:00Z"
}
```

> ⚠️ `title` é obrigatório. `type` é aceito como texto (`"Exam"`). `occurredAt` é opcional na criação (padrão: agora, em UTC). `attributes` aceita campos livres de texto.

---

## 🩺 Monitoramento e Observabilidade

### Health Checks

A API expõe dois endpoints de Health Check (públicos, sem token), pensados para cenários diferentes de monitoramento (ex.: liveness/readiness probes em Kubernetes, ou um monitor de uptime simples):

| Endpoint | Verifica | Uso típico |
|---|---|---|
| `GET /health/live` | Apenas se o processo da API está de pé (não depende de nada externo) | Liveness probe — reiniciar o container se falhar |
| `GET /health/ready` | Conectividade real com o **Oracle** (`oracle-database`) e com o **MongoDB** (`mongodb`, via comando `ping`) | Readiness probe — só rotear tráfego se estiver saudável |

Exemplo de resposta de `GET /health/ready` (formato `HealthChecks.UI.Client`):

```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0423456",
  "entries": {
    "oracle-database": {
      "status": "Healthy",
      "duration": "00:00:00.0310000",
      "tags": ["ready"]
    },
    "mongodb": {
      "status": "Healthy",
      "description": "MongoDB respondeu ao ping.",
      "duration": "00:00:00.0050000",
      "tags": ["ready"]
    }
  }
}
```

Se o Oracle **ou** o MongoDB estiver indisponível, o `status` geral muda para `"Unhealthy"`, a entrada com problema mostra o motivo, e o endpoint responde com HTTP `503 Service Unavailable`.

### Logging Estruturado

O logging é feito com **Serilog**, configurado em `Program.cs`:

- **Console** — acompanhamento em tempo real durante o desenvolvimento.
- **Arquivo** — `logs/prontpet-YYYYMMDD.log`, com rotação diária e retenção dos últimos 14 arquivos (pasta ignorada no `.gitignore`).
- **Níveis**: `Information` (operações de negócio bem-sucedidas), `Warning` (regras violadas — registro não encontrado, validação falhou, falha de login) e `Error` (exceções não tratadas, capturadas pelo `GlobalExceptionHandler`).
- **Correlação de requisições** — o `CorrelationIdMiddleware` gera (ou reaproveita) um `X-Correlation-Id`, devolve o valor no header de resposta e o injeta em toda linha de log da requisição:

```bash
grep "3fa85f64-5717-4562-b3fc-2c963f66afa6" logs/prontpet-20260910.log
```

- Toda requisição HTTP concluída gera uma linha de log automática (`UseSerilogRequestLogging`), com método, path, status e duração.
- 💡 Ao investigar um erro, procure no arquivo de log a linha `[ERR]`: ela traz o *stack trace* completo (o console também imprime métricas do OpenTelemetry, que podem esconder o erro).

### Tracing e Métricas (OpenTelemetry)

Configurado em `Infrastructure/DependencyInjection.cs`:

- **Tracing** — cada operação de negócio nos `Services` (`CreatePet`, `Login`, `GetPetTimelineByPet`...) gera um *span* nomeado, com tags relevantes (`pet.id`, `tutor.id`, `page.number`, `page.size`...) e status de erro quando a regra falha. Somado a isso, `AddAspNetCoreInstrumentation()` gera spans automáticos para cada requisição HTTP.
- **Métricas de negócio** — contadores customizados por entidade (`pets_created_total`, `tutors_created_total`, `vaccinations_created_total`...), alguns com tags de categoria (`species`, `vaccine_name`).
- **Tempo de resposta e taxa de erro** — cobertos pela instrumentação padrão do ASP.NET Core (`http.server.request.duration`).
- **Exportação** — por padrão, para o **Console** (`AddConsoleExporter()`). Em produção, troque por `AddOtlpExporter()` apontando para um coletor (Jaeger, Grafana Tempo, Application Insights...).

---

## 🧪 Testes Automatizados

Os testes seguem o padrão **AAA (Arrange, Act, Assert)** e a nomenclatura `MetodoTestado_Cenario_ResultadoEsperado`.

| Projeto | O que testa | Como isola dependências |
|---|---|---|
| `ProntPet.UnitTests` | Regras de negócio dos `Services` (incluindo paginação/filtros, autenticação/hash/JWT e timeline), contratos de paginação e configuração do Mongo | `ILogger`/`IMeterFactory`/`IPetTimelineRepository` mockados com **Moq**; `AppDbContext` com **EF Core InMemory** (um banco novo por teste) |
| `ProntPet.IntegrationTests` | Fluxo HTTP completo dos `Controllers` (rota → autenticação → binding → Service → persistência) e o **repositório MongoDB real** | `WebApplicationFactory<Program>` com Oracle trocado por EF InMemory, MongoDB trocado por um **fake em memória** e JWT de teste (todo client já sai autenticado; há um client anônimo para testar `401`) |

> ✅ Os testes unitários e os de integração HTTP **não precisam de Oracle nem de MongoDB** — rodam isolados e de forma determinística.
>
> 🐳 Os testes do **repositório MongoDB real** (`PetTimelineRepositoryMongoTests`, marcados com `[MongoFact]`) usam o container do Docker. Eles criam um banco temporário (`prontpet_test_<guid>`), apagado ao final — o banco `prontpet` nunca é tocado. Se o Mongo **não estiver no ar**, esses testes aparecem como **skipped** (não falham). Para executá-los: `docker compose up -d`. Se o seu Mongo usa outra porta, defina antes: `$env:PRONTPET_TEST_MONGO = "mongodb://localhost:27018"` (PowerShell) ou `export PRONTPET_TEST_MONGO=mongodb://localhost:27018` (bash).

### Executando pelo Visual Studio

1. Abra a solução `ProntPetSolution.slnx`.
2. Abra o **Test Explorer**: menu `Test` → `Test Explorer` (ou `Ctrl+E, T`).
3. O Visual Studio descobre os testes após o build.
4. Para rodar tudo: **Run All Tests** (▶). Para uma suíte ou classe: botão direito → `Run`. Para depurar: botão direito → `Debug`.

> Dica: agrupe por projeto no Test Explorer para ver `ProntPet.UnitTests` e `ProntPet.IntegrationTests` separadamente.

### Executando pelo VS Code ou terminal

**Pré-requisito:** [.NET SDK](https://dotnet.microsoft.com/download) (e, opcionalmente, a extensão [C# Dev Kit](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csdevkit) para o Test Explorer).

```bash
# (Opcional) subir o Mongo para rodar também os testes [MongoFact]
docker compose up -d

# Rodar TODOS os testes
dotnet test

# Apenas os unitários / apenas os de integração
dotnet test ProntPet.UnitTests
dotnet test ProntPet.IntegrationTests

# Filtrar por nome (classe ou teste)
dotnet test --filter "FullyQualifiedName~PetTimeline"

# Saída detalhada (mostra quais testes foram executados e quais foram skipped)
dotnet test --logger "console;verbosity=detailed"
```

> A solução usa o formato `.slnx`, que exige um .NET SDK recente. Com o SDK 8, rode os projetos individualmente (`dotnet test ProntPet.UnitTests`).

---

## 🗂 Estrutura dos Bancos de Dados

**Oracle (relacional — EF Core)**

```
DB_TUTOR (tutores; senha armazenada como hash)
  └── DB_PET (pets, FK → DB_TUTOR)
        ├── DB_VACCINATION (vacinações, FK → DB_PET)
        └── DB_MEDICAL_RECORD (prontuários, FK → DB_PET, 1:1)
              └── DB_CONSULTATION (consultas, FK → DB_MEDICAL_RECORD + DB_CLINIC)

DB_CLINIC (clínicas)
  └── DB_CONSULTATION (FK → DB_CLINIC)
```

**MongoDB (documentos)** — banco `prontpet`, coleção `pet_timeline`:

```json
{
  "_id": "6730a1f2c3d4e5f6a7b8c9d0",
  "IdPet": 1,
  "Type": "Exam",
  "Title": "Hemograma completo",
  "Description": "Resultados dentro da normalidade",
  "Tags": ["rotina", "sangue"],
  "Attributes": { "laboratorio": "VetLab", "peso": "12,4kg" },
  "OccurredAt": "2026-10-20T14:30:00Z",
  "CreatedAt": "2026-10-31T21:05:12Z"
}
```

`IdPet` referencia `DB_PET.ID` no Oracle (a existência do pet é validada pelo Service ao criar o registro).

---

## 🛟 Solução de Problemas

| Sintoma | Causa provável | Solução |
|---|---|---|
| `Bind for 0.0.0.0:27017 failed: port is already allocated` ao subir o Docker | Outro Mongo (container ou serviço do Windows) usa a porta 27017 | Pare o outro Mongo **ou** mapeie `"27018:27017"` no `docker-compose.yml` e use `mongodb://localhost:27018` no `appsettings.json` |
| `/health/ready` com `mongodb` **Unhealthy** (`SocketException 10061` / timeout) | Container do Mongo parado, ou porta errada no `appsettings.json` | `docker ps --filter name=prontpet-mongo`; `docker compose up -d`; confira a porta |
| `/health/ready` com `oracle-database` **Unhealthy** ou erro `ORA-01017` | Usuário/senha do Oracle ausentes ou incorretos | Preencha `ConnectionStrings:OracleConnection` no `appsettings.json` e **reinicie a API** |
| A API não sobe: `Configuração 'Jwt:Key' ausente ou curta demais` | Falta a seção `Jwt` no `appsettings.json` (ou chave com menos de 32 caracteres) | Adicione a seção `Jwt` (veja o [passo 3](#3-configure-as-credenciais-no-appsettingsjson-obrigatório)) |
| `Configuração do MongoDB ausente...` | Falta a seção `MongoDb` no `appsettings.json` | Adicione `MongoDb:ConnectionString` e `MongoDb:DatabaseName` |
| `You must install or update .NET to run this application` (`Microsoft.AspNetCore.App 8.0.0`) | Só há runtime 9/10 instalado | Instale o **.NET 8 SDK** (`winget install Microsoft.DotNet.SDK.8`) |
| `401 Unauthorized` em uma rota protegida | Token ausente, inválido ou expirado | Faça login de novo em `POST /api/Auth/login` e use *Authorize* no Swagger |
| `401` no login com credenciais corretas | Tutor cadastrado antes do hash de senha (senha em texto puro) | Cadastre um novo tutor |
| Rotas `/api/PetTimeline` demoram ~5 s e retornam `500` | Mongo fora do ar (timeout de seleção de servidor) | Suba o container do Mongo |
| Testes `[MongoFact]` aparecem como *skipped* | Mongo não está acessível para os testes | `docker compose up -d` (ou defina `PRONTPET_TEST_MONGO`) |
