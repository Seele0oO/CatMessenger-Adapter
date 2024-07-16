FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY CatMessenger.Matrix/*.csproj ./
RUN dotnet restore

COPY CatMessenger.Matrix/. ./
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CatMessenger.Matrix.dll"]