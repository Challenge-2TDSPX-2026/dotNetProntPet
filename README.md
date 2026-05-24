# 🐾 ProntPet API

API RESTful para gerenciamento de prontuários veterinários, desenvolvida em **ASP.NET Core 8** com banco de dados **Oracle**. Permite o cadastro e controle completo de tutores, pets, vacinas, prontuários médicos, consultas e clínicas veterinárias.

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

---

## 🛠 Tecnologias

- [.NET 8](https://dotnet.microsoft.com/)
- [ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core 8](https://learn.microsoft.com/en-us/ef/core/)
- [Oracle.EntityFrameworkCore 8.21.121](https://www.nuget.org/packages/Oracle.EntityFrameworkCore)
- [Swashbuckle (Swagger) 6.6.2](https://github.com/domaindrivendev/Swashbuckle.AspNetCore)

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
