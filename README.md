
# 📊 StatsHub

StatsHub — тестовый проект.  

Он демонстрирует архитектуру **CQRS + MediatR**, валидацию через **FluentValidation**, оповещения через **SignalR**, кэширование на **Redis**, SPA‑клиент на **React (CRA)** и хранение данных в **PostgreSQL**.
В рамках StatsHub были приняты и реализованы следующие ключевые архитектурные и технологические решения:

    CQRS + MediatR
    — все запросы к данным и команды вынесены в отдельный слой StatsHub.Application с помощью библиотеки MediatR. Это позволяет чётко разделить чтение и запись, упростить поддержку и тестирование отдельных сценариев.

    Валидация через FluentValidation
    — входные DTO для API проверяются при помощи декларативных валидаторов, настроенных в StatsHub.Application.Validators. В случае ошибок возвращается 400 Bad Request с подробным списком нарушенных правил.

    Хранение данных в PostgreSQL + EF Core
    — в качестве основной СУБД выбрана PostgreSQL, доступ к ней организован через Entity Framework Core 7 в проекте StatsHub.Infrastructure. Миграции управляются автоматически при запуске.

    Кэширование через IDistributedCache и Redis
    — для долговременного кэширования агрегированных данных (например, выручки по брендам) используется IDistributedCache с бэкендом Redis. В деве можно использовать встроенный In-Memory, но в продакшне Redis обеспечивает масштабируемость и отказоустойчивость.

    Реал-тайм уведомления через SignalR
    — WebSocket-хаб RevenueHub рассылает всем подключённым клиентам обновлённые данные каждый раз, когда система получает новые заказы (событие OrdersSyncedNotification). Клиент сразу перерисовывает график без перезагрузки страницы.

    SPA-клиент на React (Create React App)
    — фронтенд реализован как одностраничное приложение (SPA) с помощью CRA, подключённого через middleware ASP.NET Core SPA Services. Для графиков выбрана библиотека Chart.js 4.

    Единый middleware для обработки ошибок
    — собственный ApiExceptionMiddleware ловит FluentValidation.ValidationException и переводит их в стандартный 400 Bad Request с деталями, а все остальные исключения возвращает с кодом 500 Internal Server Error.

    Swagger/OpenAPI
    — автоматически сгенерированная документация API через Swashbuckle доступна в Development по /swagger.

    Контейнеризация и docker-compose
    — инфраструктура (PostgreSQL и Redis) поднимается через docker-compose.yml. API и фронтенд можно запускать локально или упаковывать в Docker-контейнер.

    Каждое из этих решений было выбрано, чтобы обеспечить чёткую архитектуру, простоту тестирования, масштабируемость и удобство.
---

## ✨ Основные технологии

| Слой | Стек |
|------|------|
| Backend | ASP.NET Core 7, EF Core 7 (PostgreSQL), MediatR, SignalR, FluentValidation, Swashbuckle |
| Frontend | React 18 (Create‑React‑App), Chart.js 4, SignalR JS |
| Инфра | Docker + docker‑compose, PostgreSQL 16, Redis 7 |

---

## 📂 Структура репозитория

```
StatsHub/
├─ StatsHub.Web/                   # ASP.NET Core API + SPA hosting
│  └─ ClientApp/          # CRA фронт (npm start / build)
├─ StatsHub.Application/  # CQRS‑слой, хэндлеры, уведомления
├─ StatsHub.Infrastructure/ # EF Core, контекст, миграции
├─ StatsHub.Domain/       # Доменные сущности
└─ docker-compose.yml
```

---

## 🚀 Быстрый старт (Dev)

```bash
# 1. Запускаем инфраструктуру
docker-compose up -d                 # поднимет postgres и redis

# 2. Backend
dotnet run --project Web             # http://localhost:5000

# 3. Frontend (в другом терминале)
cd Web/ClientApp
npm install
npm start                             # http://localhost:3000
```

> В dev‑режиме CRA проксирует запросы `/orders/*` и `/hubs/*` к бэкенду.

---

## 🔧 Переменные окружения

| Переменная | По умолчанию | Описание |
|------------|--------------|----------|
| `ConnectionStrings__Default` | `Host=db;Database=statshub;Username=postgres;Password=postgres` | строка подключения Postgres |
| `ConnectionStrings__Redis` | `redis:6379` | Redis для кэша |

---

## 📑 API‑эндпоинты

| Method | URL | Description |
|--------|-----|-------------|
| GET | `/orders/daily-stats` | Суммарная выручка по дням |
| GET | `/swagger` | Swagger UI |
| WS | `/hubs/revenue` | SignalR‑хаб с событием `update` |

---

## 🗃️ Миграции

```bash
dotnet ef migrations add Init -p StatsHub.Infrastructure -s Web
dotnet ef database update -s Web
```

---

## 🛠️ Чек‑лист

- [x] CQRS + MediatR
- [x] FluentValidation → 400 Bad Request (middleware)
- [x] Redis‑кэш (IDistributedCache)
- [x] SignalR реал‑тайм график
- [x] Swagger/OpenAPI
- [x] Докеризация
