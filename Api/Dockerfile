# Используем официальный образ .NET 9 для сборки
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем файлы проектов
COPY ChessWar.Api/ChessWar.Api.csproj ChessWar.Api/
COPY ChessWar.Application/ChessWar.Application.csproj ChessWar.Application/
COPY ChessWar.Domain/ChessWar.Domain.csproj ChessWar.Domain/
COPY ChessWar.Infrastructure/ChessWar.Infrastructure.csproj ChessWar.Infrastructure/
COPY ChessWar.Persistence.Core/ChessWar.Persistence.Core.csproj ChessWar.Persistence.Core/
COPY ChessWar.Persistence.Sqlite/ChessWar.Persistence.Sqlite.csproj ChessWar.Persistence.Sqlite/

# Восстанавливаем зависимости
RUN dotnet restore ChessWar.Api/ChessWar.Api.csproj

# Копируем весь исходный код
COPY . .

# Собираем приложение
WORKDIR /src/ChessWar.Api
RUN dotnet build "ChessWar.Api.csproj" -c Release -o /app/build

# Публикуем приложение
FROM build AS publish
RUN dotnet publish "ChessWar.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Финальный образ для запуска
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Копируем опубликованное приложение
COPY --from=publish /app/publish .

# Создаем директории для данных и логов
RUN mkdir -p App_Data Logs

# Устанавливаем права доступа
RUN chmod 755 App_Data Logs

# Открываем порт 80
EXPOSE 80

# Устанавливаем переменные окружения
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

# Запускаем приложение
ENTRYPOINT ["dotnet", "ChessWar.Api.dll"]
