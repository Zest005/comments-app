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
- **File attachments** — images (JPG, PNG, GIF) and text files (TXT) can be attached to comments
- **Image auto-resize** — images larger than 320×240 are proportionally resized
- **CAPTCHA** — required when uploading files, generated server-side with SkiaSharp
- **Real-time updates** — new comments appear instantly via SignalR (WebSocket)
- **Sorting** — by date (newest/oldest first), default LIFO
- **Pagination** — 25 comments per page
- **HTML tag support** — allowed tags: `<a>`, `<code>`, `<i>`, `<strong>` with XHTML validation
- **XSS protection** — all disallowed HTML tags are escaped
- **SQL injection protection** — Entity Framework parameterized queries
- **Client & server validation** — all inputs validated on both sides
- **Lightbox** — image preview with open/close animations
- **Localization** — English, Ukrainian, Russian with proper date formatting
- **User session** — login/logout with localStorage, auto-fill comment form
- **Preview** — real-time message preview with HTML rendering

## Architecture
CommentsApp/
├── CommentsApp.Domain/          — Entities (Comment, CommentAttachment)
├── CommentsApp.Application/     — DTOs, Services, Validators, Interfaces
├── CommentsApp.Infrastructure/  — CAPTCHA generation, File processing
├── CommentsApp.Persistence/     — EF Core DbContext, Configurations, Repositories
├── CommentsApp.Web/             — API Controllers, SignalR Hub, Program.cs
└── client-app/                  — Angular SPA

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

The database schema file is available at `database-schema.sql` and can be opened in MySQL Workbench.

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
| ParentCommentId | INT (FK, NULL)| Self-referencing for replies    |

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

| Method | URL              | Description              |
|--------|------------------|--------------------------|
| GET    | /api/comments    | Get comments (paginated) |
| POST   | /api/comments    | Create a comment         |
| GET    | /api/captcha     | Generate CAPTCHA         |
| GET    | /api/attachments/{id} | Get attachment file |

## WebSocket

SignalR Hub: `/hubs/comments`

Events:
- `NewComment` — fired when a new comment is created, broadcasts to all connected clients