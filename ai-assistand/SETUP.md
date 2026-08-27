# Step-by-step setup

## Option A — Run everything with Docker (recommended)

This starts PostgreSQL, the API, and the React web UI together.

### Prerequisites

1. [Docker Desktop](https://www.docker.com/products/docker-desktop/)
2. DeepSeek API key
3. Fish Audio API key (optional until you want voice)

### 1. Open the project

```powershell
cd C:\Users\Bhbored\Desktop\assistant
```

### 2. Configure keys

```powershell
Copy-Item .env.example .env
```

Edit `.env`:

```text
DeepSeek__ApiKey=your-deepseek-key
FishAudio__ApiKey=your-fish-audio-key
FishAudio__VoiceId=your-voice-id
```

### 3. Start the full stack

Start Docker Desktop, then:

```powershell
docker compose up --build -d
```

First build can take several minutes.

Check status:

```powershell
docker compose ps
docker compose logs -f api
```

### 4. Open the app

| Service | URL |
|---|---|
| Web app | http://localhost:8080 |
| Swagger | http://localhost:8080/swagger |
| Health | http://localhost:8080/health |
| Hangfire | http://localhost:8080/hangfire |
| API (direct) | http://localhost:5080 |
| Postgres | localhost:5432 |

Nginx on the web container proxies `/api`, `/hubs`, `/swagger`, `/hangfire`, and `/health` to the API.

### 5. First workflow

1. Open http://localhost:8080
2. Create a goal
3. Add tasks
4. Chat with the assistant
5. Create a reminder

### 6. Stop

```powershell
docker compose down
```

Remove database volume too:

```powershell
docker compose down -v
```

### Common Docker problems

**Docker daemon / pipe error**  
Start Docker Desktop and wait until it is fully running.

**API unhealthy / cannot connect to database**  
Wait for Postgres healthcheck, then:

```powershell
docker compose restart api
docker compose logs api
```

**Assistant says API key is missing**  
Confirm `.env` has `DeepSeek__ApiKey`, then:

```powershell
docker compose up -d --force-recreate api
```

**Rebuild after code changes**

```powershell
docker compose up --build -d
```

---

## Option B — Local development

Use this when you want hot reload on the API or web app.

### Prerequisites

1. .NET 10 SDK
2. Node.js 20+
3. Docker Desktop (Postgres only)

### 1. Start Postgres only

```powershell
docker compose up postgres -d
```

### 2. Configure keys

Same `.env` as above, or set PowerShell env vars / `appsettings.json`.

For local API use:

```text
ConnectionStrings__Postgres=Host=localhost;Port=5433;Database=personal_assistant;Username=postgres;Password=123456
```

### 3. Run API

```powershell
dotnet restore
dotnet build
dotnet run --project src/PersonalAssistant.Api
```

API: http://localhost:5080

### 4. Run web

```powershell
cd web\personal-assistant-web
Copy-Item .env.example .env
npm install
npm run dev
```

Web: http://localhost:5173

### 5. Tests

```powershell
dotnet test
```

---

## Project layout

```text
assistant/
├── src/
│   ├── PersonalAssistant.Api/
│   ├── PersonalAssistant.Application/
│   ├── PersonalAssistant.Domain/
│   └── PersonalAssistant.Infrastructure/
├── web/personal-assistant-web/
├── tests/PersonalAssistant.UnitTests/
├── docker-compose.yml
├── .env.example
├── README.md
└── SETUP.md
```
