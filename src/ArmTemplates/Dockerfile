#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/ArmTemplates/ArmTemplates.csproj", "src/ArmTemplates/"]
RUN dotnet restore "src/ArmTemplates/ArmTemplates.csproj"
COPY . .
WORKDIR "/src/src/ArmTemplates"
RUN dotnet build "ArmTemplates.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ArmTemplates.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ArmTemplates.dll"]