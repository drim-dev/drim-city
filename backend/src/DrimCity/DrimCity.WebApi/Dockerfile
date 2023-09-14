FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

WORKDIR /src
COPY ./DrimCity.WebApi/DrimCity.WebApi.csproj ./DrimCity.WebApi/
RUN dotnet restore ./DrimCity.WebApi/DrimCity.WebApi.csproj

COPY . .
RUN dotnet build ./DrimCity.WebApi/DrimCity.WebApi.csproj -c Release -o /app/build

RUN dotnet publish DrimCity.WebApi.csproj -c Release -o /app/publish

FROM base AS final

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DrimCity.WebApi.dll"]
