FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app    

# Copy csproj and restore as distinct layers
COPY /apimtemplate/*.csproj ./
RUN dotnet restore

# Copy everything else and build
COPY . ./
WORKDIR /app/apimtemplate
RUN dotnet publish apimtemplate.sln -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /
COPY --from=build-env /app/apimtemplate/out .
