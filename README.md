### ENGLISH

# CommentsApp — SPA Comment System

A single-page application for leaving and managing comments with cascading replies, file attachments, CAPTCHA verification, and real-time updates via WebSocket.

## Tech Stack

**Backend:**
- .NET 10, ASP.NET Core Web API
- Entity Framework Core (Code First)
- MS SQL Server
- SignalR (WebSocket for real-time updates)
- Clean Architecture (Domain, Application, Infrastructure, Persistence, Web)

**Frontend:**
- Angular 21 (Standalone Components)
- TypeScript
- RxJS
- @microsoft/signalr

**DevOps:**
- Docker & Docker Compose
- Git

## Features

- **Comments with cascading replies** — users can reply to any comment, creating a tree structure
- **File attachments** — images (JPG, PNG, GIF) and text files (TXT) < 100KB can be attached to comments
- **Image auto-resize** — images larger than 320x240 are proportionally resized
- **CAPTCHA** — required when uploading files, generated server-side with SkiaSharp
- **Real-time updates** — new comments appear instantly via SignalR (WebSocket)
- **Sorting** — by date (newest/oldest first), default LIFO
- **Pagination** — 25 comments per page
- **HTML tag support** — allowed tags: `<a>`, `<code>`, `<i>`, `<strong>` with XHTML validation
- **XSS protection** — all disallowed HTML tags are escaped
- **SQL injection protection** — Entity Framework parameterized queries
- **Client & server validation** — all inputs validated on both sides
- **Lightbox** — image preview with open/close animations
- **Localization** — English, Ukrainian with proper date formatting
- **User session** — login/logout with localStorage, auto-fill comment form
- **Preview** — real-time message preview with HTML rendering

## Architecture

```
CommentsApp/
├── CommentsApp.Domain/          — Entities (Comment, CommentAttachment)
├── CommentsApp.Application/     — DTOs, Services, Validators, Interfaces
├── CommentsApp.Infrastructure/  — CAPTCHA generation, File processing
├── CommentsApp.Persistence/     — EF Core DbContext, Configurations, Repositories
├── CommentsApp.Web/             — API Controllers, SignalR Hub, Program.cs
└── client-app/                  — Angular SPA
```

## Running Locally (Development)

### Prerequisites

- .NET 10 SDK
- Node.js 22+
- MS SQL Server (Developer or Express)
- Angular CLI (`npm install -g @angular/cli`)

### Backend

1. Update connection string in `CommentsApp.Web/appsettings.json`
2. Apply migrations:

```bash
dotnet ef database update --project CommentsApp.Persistence --startup-project CommentsApp.Web
```

3. Run the API:

```bash
cd CommentsApp.Web
dotnet run
```

### Frontend

```bash
cd client-app
npm install
ng serve
```

Open `http://localhost:4200`

## Running with Docker

### Prerequisites

- Docker Desktop

### Start

```bash
docker-compose up --build
```

Open `http://localhost:8080`

### Stop

```bash
docker-compose down
```

## Database Schema

The database schema files are available at:
- `database-schema.sql` — SQL script (can be opened in MySQL Workbench via File → Open SQL Script)
- `database-schema.mwb` — visual EER diagram (open in MySQL Workbench)

### Tables

**Comments**

| Column          | Type          | Description                    |
|-----------------|---------------|--------------------------------|
| Id              | INT (PK)      | Auto-increment primary key     |
| UserName        | NVARCHAR(50)  | Latin letters and digits only  |
| Email           | NVARCHAR(254) | Valid email format             |
| HomePage        | NVARCHAR(2048)| Optional URL                   |
| Text            | NVARCHAR(MAX) | Comment text (sanitized HTML)  |
| CreatedAt       | DATETIME2     | UTC timestamp                  |
| ParentCommentId | INT (FK, NULL)| Self-referencing for replies   |

**CommentAttachments**

| Column         | Type          | Description                     |
|----------------|---------------|---------------------------------|
| Id             | INT (PK)      | Auto-increment primary key      |
| CommentId      | INT (FK)      | Reference to parent comment     |
| FileName       | NVARCHAR(255) | Original file name              |
| StoredFilePath | NVARCHAR(500) | Server-side file path           |
| ContentType    | NVARCHAR(100) | MIME type                       |
| FileSize       | BIGINT        | File size in bytes              |

## API Endpoints

| Method | URL                    | Description              |
|--------|------------------------|--------------------------|
| GET    | /api/comments          | Get comments (paginated) |
| POST   | /api/comments          | Create a comment         |
| GET    | /api/captcha           | Generate CAPTCHA         |
| GET    | /api/attachments/{id}  | Get attachment file      |

## WebSocket

SignalR Hub: `/hubs/comments`

Events:
- `NewComment` — fired when a new comment is created, broadcasts to all connected clients

### UKRAINIAN

# CommentsApp — SPA Система коментарів

Односторінковий додаток для створення та управління коментарями з каскадними відповідями, вкладеннями файлів, перевіркою CAPTCHA та оновленнями в реальному часі через WebSocket.

## Технології

**Backend:**
- .NET 10, ASP.NET Core Web API
- Entity Framework Core (Code First)
- MS SQL Server
- SignalR (WebSocket для оновлень в реальному часі)
- Clean Architecture (Domain, Application, Infrastructure, Persistence, Web)

**Frontend:**
- Angular 21 (Standalone Components)
- TypeScript
- RxJS
- @microsoft/signalr

**DevOps:**
- Docker & Docker Compose
- Git

## Функціонал

- **Коментарі з каскадними відповідями** — користувачі можуть відповідати на будь-який коментар, створюючи деревоподібну структуру
- **Вкладення файлів** — до коментарів можна додавати зображення (JPG, PNG, GIF) та текстові файли (TXT) < 100KB
- **Автоматичне зменшення зображень** — зображення більші за 320x240 пропорційно зменшуються
- **CAPTCHA** — обов'язкова при завантаженні файлів, генерується на сервері за допомогою SkiaSharp
- **Оновлення в реальному часі** — нові коментарі з'являються миттєво через SignalR (WebSocket)
- **Сортування** — за датою (спочатку нові / спочатку старі), за замовчуванням LIFO
- **Пагінація** — 25 коментарів на сторінку
- **Підтримка HTML тегів** — дозволені теги: `<a>`, `<code>`, `<i>`, `<strong>` з валідацією XHTML
- **Захист від XSS** — всі недозволені HTML теги екрануються
- **Захист від SQL-ін'єкцій** — параметризовані запити Entity Framework
- **Валідація на клієнті та сервері** — всі дані перевіряються на обох сторонах
- **Lightbox** — перегляд зображень з анімаціями відкриття/закриття
- **Локалізація** — англійська, українська з коректним форматуванням дати
- **Сесія користувача** — вхід/вихід через localStorage, автозаповнення форми коментаря
- **Попередній перегляд** — перегляд повідомлення в реальному часі з рендерингом HTML

## Архітектура

```
CommentsApp/
├── CommentsApp.Domain/          — Сутності (Comment, CommentAttachment)
├── CommentsApp.Application/     — DTO, Сервіси, Валідатори, Інтерфейси
├── CommentsApp.Infrastructure/  — Генерація CAPTCHA, Обробка файлів
├── CommentsApp.Persistence/     — EF Core DbContext, Конфігурації, Репозиторії
├── CommentsApp.Web/             — API Контролери, SignalR Hub, Program.cs
└── client-app/                  — Angular SPA
```

## Локальний запуск (Розробка)

### Необхідне ПЗ

- .NET 10 SDK
- Node.js 22+
- MS SQL Server (Developer або Express)
- Angular CLI (`npm install -g @angular/cli`)

### Backend

1. Оновіть рядок підключення в `CommentsApp.Web/appsettings.json`
2. Застосуйте міграції:

```bash
dotnet ef database update --project CommentsApp.Persistence --startup-project CommentsApp.Web
```

3. Запустіть API:

```bash
cd CommentsApp.Web
dotnet run
```

### Frontend

```bash
cd client-app
npm install
ng serve
```

Відкрийте `http://localhost:4200`

## Запуск через Docker

### Необхідне ПЗ

- Docker Desktop

### Запуск

```bash
docker-compose up --build
```

Відкрийте `http://localhost:8080`

### Зупинка

```bash
docker-compose down
```

## Схема бази даних

Файли схеми бази даних:
- `database-schema.sql` — SQL скрипт (можна відкрити в MySQL Workbench через File → Open SQL Script)
- `database-schema.mwb` — візуальна EER діаграма (відкрити в MySQL Workbench)

### Таблиці

**Comments (Коментарі)**

| Стовпець        | Тип           | Опис                            |
|-----------------|---------------|---------------------------------|
| Id              | INT (PK)      | Автоінкрементний первинний ключ |
| UserName        | NVARCHAR(50)  | Тільки латинські літери та цифри|
| Email           | NVARCHAR(254) | Валідний формат email           |
| HomePage        | NVARCHAR(2048)| Необов'язкове URL               |
| Text            | NVARCHAR(MAX) | Текст коментаря (очищений HTML) |
| CreatedAt       | DATETIME2     | Мітка часу UTC                  |
| ParentCommentId | INT (FK, NULL)| Самопосилання для відповідей    |

**CommentAttachments (Вкладення)**

| Стовпець       | Тип           | Опис                            |
|----------------|---------------|---------------------------------|
| Id             | INT (PK)      | Автоінкрементний первинний ключ |
| CommentId      | INT (FK)      | Посилання на коментар           |
| FileName       | NVARCHAR(255) | Оригінальна назва файлу         |
| StoredFilePath | NVARCHAR(500) | Шлях до файлу на сервері        |
| ContentType    | NVARCHAR(100) | MIME тип                        |
| FileSize       | BIGINT        | Розмір файлу в байтах           |

## API Ендпоінти

| Метод  | URL                    | Опис                              |
|--------|------------------------|-----------------------------------|
| GET    | /api/comments          | Отримати коментарі (з пагінацією) |
| POST   | /api/comments          | Створити коментар                 |
| GET    | /api/captcha           | Згенерувати CAPTCHA               |
| GET    | /api/attachments/{id}  | Отримати вкладений файл           |

## WebSocket

SignalR Hub: `/hubs/comments`

Події:
- `NewComment` — спрацьовує при створенні нового коментаря, надсилається всім підключеним клієнтам
