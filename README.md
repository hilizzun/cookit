# CookIt

CookIt — полнофункциональное веб-приложение для публикации и поиска кулинарных рецептов. Проект объединяет каталог рецептов, социальные функции, персональные подборки, инструменты модерации и административную панель.

## Возможности

- регистрация, вход по JWT, refresh-токены и подтверждение email;
- роли пользователя, модератора и администратора, блокировка учетных записей;
- создание, редактирование, удаление и повторная отправка рецептов на модерацию;
- фильтрация, пагинация, полнотекстовый поиск, подбор случайного рецепта и списки лучших и новых рецептов;
- загрузка изображений рецептов и аватаров в MinIO;
- избранное, оценки, древовидные комментарии и жалобы на комментарии;
- расчет пищевой ценности и управление справочниками ингредиентов, единиц измерения, оборудования и типов блюд;
- список покупок с пересчетом порций и экспорт рецептов и списка покупок в PDF;
- публичные профили, пользовательская статистика и достижения;
- генерация интересных фактов и фоновая модерация комментариев с помощью локальной модели Ollama;
- адаптивный интерфейс и защищенные пользовательские и административные маршруты.

## Технологический стек

### Backend

- .NET 10, ASP.NET Core Web API;
- Entity Framework Core 10 и PostgreSQL;
- ASP.NET Core Identity, JWT и MailKit;
- MinIO для объектного хранилища;
- OllamaSharp и Microsoft.Extensions.AI;
- Serilog, Seq и Swagger/OpenAPI.

### Frontend

- React 19 и Vite 6;
- React Router 7;
- Tailwind CSS 4 и Radix UI;
- TipTap для форматированного текста;
- `@react-pdf/renderer` для экспорта PDF.

## Архитектура

Backend разделен на четыре проекта:

- `CookIt.Core` — сущности, DTO, настройки и интерфейсы;
- `CookIt.Application` — прикладные сервисы и фоновые задачи;
- `CookIt.Infrastructure` — Entity Framework Core, репозитории, хранилище и интеграции;
- `CookIt.Api` — HTTP API, аутентификация, middleware и конфигурация зависимостей.

Конфигурации моделей Entity Framework Core находятся в `CookIt.Infrastructure/Configuration/EntityFramework` и автоматически применяются при построении модели.

## Структура репозитория

```text
.
├── backend/CookIt/              # решение .NET
│   ├── CookIt.Api/
│   ├── CookIt.Application/
│   ├── CookIt.Core/
│   └── CookIt.Infrastructure/
├── frontend/cookIt/             # приложение React
├── docker-compose.yml           # локальный MinIO
└── Толмачева Е. Д._Веб.pptx.pdf # презентация проекта
```

## Локальный запуск

### Требования

- .NET 10 SDK;
- Node.js и npm;
- PostgreSQL;
- Docker с поддержкой Docker Compose;
- Ollama с моделью `qwen2.5:3b` — для генерации фактов и модерации комментариев;
- Seq — необязательно, для просмотра структурированных логов.

### 1. Запустите MinIO

Из корня репозитория:

```bash
docker compose up -d
```

MinIO API будет доступен на `http://localhost:9000`, консоль — на `http://localhost:9001`.

### 2. Подготовьте PostgreSQL

Создайте базу данных `CookIt` и укажите строку подключения `ConnectionStrings:DefaultConnection` в `backend/CookIt/CookIt.Api/appsettings.json` или через переменную окружения `ConnectionStrings__DefaultConnection`.

Для локального окружения также настройте:

- `JwtSettings:Secret`, `JwtSettings:Issuer` и `JwtSettings:Audience`;
- параметры SMTP в секции `SmtpSettings`;
- при необходимости `BaseUrl`, интервалы фоновых задач и адреса внешних сервисов.

Не используйте значения из локальной конфигурации как производственные секреты.

### 3. Примените миграции и запустите API

```bash
cd backend/CookIt/CookIt.Api
dotnet ef database update
dotnet run --launch-profile https
```

API запускается на `https://localhost:7031`, Swagger UI — на `https://localhost:7031/swagger`.

### 4. Запустите frontend

В отдельном терминале:

```bash
cd frontend/cookIt
npm ci
npm run dev
```

Интерфейс будет доступен на `http://localhost:5173`. Адрес API задается в `frontend/cookIt/src/config/settings.js`.

## Проверка проекта

```bash
dotnet build backend/CookIt/CookIt.sln
cd frontend/cookIt
npm run lint
npm run build
```

## Документация

- Swagger/OpenAPI генерируется при запуске backend в режиме разработки.
- Обзор проекта и интерфейса находится в файле `Толмачева Е. Д._Веб.pptx.pdf`.
