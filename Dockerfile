FROM node:22-alpine AS frontend-build

WORKDIR /app/client-app
COPY client-app/package*.json ./
RUN npm ci
COPY client-app/ ./
RUN npm run build -- --configuration=production



FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend-build

WORKDIR /app
COPY CommentsApp.slnx ./
COPY CommentsApp.Domain/*.csproj CommentsApp.Domain/
COPY CommentsApp.Application/*.csproj CommentsApp.Application/
COPY CommentsApp.Infrastructure/*.csproj CommentsApp.Infrastructure/
COPY CommentsApp.Persistence/*.csproj CommentsApp.Persistence/
COPY CommentsApp.Web/*.csproj CommentsApp.Web/
RUN dotnet restore CommentsApp.slnx

COPY . .
RUN dotnet publish CommentsApp.Web/CommentsApp.Web.csproj -c Release -o /app/publish



FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    libfontconfig1 \
    libfreetype6 \
    libx11-6 \
    libgl1 \
    fonts-dejavu-core \
    && rm -rf /var/lib/apt/lists/*

COPY --from=backend-build /app/publish .
COPY --from=frontend-build /app/client-app/dist/client-app/browser wwwroot

RUN mkdir -p /app/uploads

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "CommentsApp.Web.dll"]