FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY Workbook.Core/Workbook.Core.csproj Workbook.Core/
COPY Workbook.Application/Workbook.Application.csproj Workbook.Application/
COPY Workbook.Infrastructure/Workbook.Infrastructure.csproj Workbook.Infrastructure/
COPY Workbook.WebApp/Workbook.WebApp.csproj Workbook.WebApp/
RUN dotnet restore Workbook.WebApp/Workbook.WebApp.csproj

COPY Workbook.Core/ Workbook.Core/
COPY Workbook.Application/ Workbook.Application/
COPY Workbook.Infrastructure/ Workbook.Infrastructure/
COPY Workbook.WebApp/ Workbook.WebApp/
RUN dotnet publish Workbook.WebApp/Workbook.WebApp.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

# Render (and most container platforms) inject PORT at runtime; fall back to 8080 for local `docker run`.
ENV PORT=8080
EXPOSE 8080
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT} dotnet Workbook.WebApp.dll"]
