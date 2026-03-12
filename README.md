# Configuração do Ambiente

## Pré-requisitos

- .NET 10
- Docker


```bash
dotnet tool install --global dotnet-ef
```

---

## Banco de Dados (Docker)

1. Suba o PostgreSQL com Docker Compose:

```bash
docker-compose up -d
```

2. Configure a connection string no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5434;Database=TestScoreDB;Username=postgres;Password=postgres123"
  }
}
```

3. Aplique as migrations:

```bash
dotnet ef database update
```

### pgAdmin

O pgAdmin estará disponível em `http://localhost:5050`.


Para conectar ao banco dentro do pgAdmin, use `postgres` como host (nome do serviço no Docker) e porta `5432`. O nome do Database deve ser TestScoreDB.

---

## Executando a aplicação

```bash
cd Teste.ScoreAPI
dotnet restore
dotnet run
```

Swagger disponível em: `http://localhost:<porta>/swagger`
