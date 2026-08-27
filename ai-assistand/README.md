# Personal AI Goal & Accountability Assistant

Local-first personal assistant for goals, tasks, progress, reminders, chat, and voice.

## Stack

- ASP.NET Core Web API (`.NET 10`)
- PostgreSQL + EF Core
- Hangfire for durable reminders
- DeepSeek for tool-enabled chat
- Fish Audio for STT/TTS
- React + TypeScript + TanStack Query + SignalR

## Quick start (everything in Docker)

1. Copy `.env.example` to `.env` and set your API keys
2. Start Docker Desktop
3. Run:

```powershell
docker compose up --build -d
```

Open:

- App: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- Health: http://localhost:8080/health
- Hangfire: http://localhost:8080/hangfire
- API direct: http://localhost:5080

Stop:

```powershell
docker compose down
```

Full walkthrough: [SETUP.md](./SETUP.md)

## Local development (without containers for app code)

```powershell
docker compose up postgres -d
dotnet run --project src/PersonalAssistant.Api
cd web/personal-assistant-web
npm install
npm run dev
```

## MVP loop

1. Create a goal on the dashboard
2. Add tasks on the goal page
3. Ask the assistant “What should I work on next?”
4. Mark progress or complete a task via chat or UI
5. Schedule a reminder and keep the web app open for SignalR delivery

## Configuration

Set these in `.env` (used by Compose):

- `DeepSeek__ApiKey`
- `DeepSeek__BaseUrl`
- `DeepSeek__Model`
- `FishAudio__ApiKey`
- `FishAudio__VoiceId`

Compose overrides the Postgres connection to the `postgres` service automatically.

Never commit real API keys.
