FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS backend-build

WORKDIR /backend

COPY . ./

WORKDIR /backend/Spawn.Demo.WebApi

RUN dotnet restore

RUN dotnet publish -c Release -o out

# build final image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1.5-alpine3.12 AS runtime

RUN apk add icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /app

COPY --from=backend-build /backend/Spawn.Demo.WebApi/out ./

# Run this container as non-root user
# 5000 is an arbitrarily chosen, non-reserved Linux UID
USER 5000

EXPOSE 8080
ENV ASPNETCORE_URLS=http://*:8080

ENTRYPOINT ["dotnet", "Spawn.Demo.WebApi.dll"]