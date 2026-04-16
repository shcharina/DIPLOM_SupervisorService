FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /app

# Копируем файлы проекта и восстанавливаем зависимости

COPY *.csproj ./

RUN dotnet restore

# Копируем весь код и собираем

COPY . ./

RUN dotnet publish -c Release -o /out

# Финальный лёгкий образ для запуска

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app

COPY --from=build /out .

# Создаём не-root пользователя для безопасности

RUN adduser --disabled-password --gecos '' --no-create-home appuser

USER appuser

EXPOSE 5002

ENV ASPNETCORE_URLS=http://+:5002

ENTRYPOINT ["dotnet", "InternshipManager.Api.dll"]