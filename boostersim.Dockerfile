FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
COPY BoosterPackSimulator/BoosterPackSimulator.csproj .
RUN dotnet restore BoosterPackSimulator.csproj
COPY . .
RUN dotnet build BoosterPackSimulator.sln -c Release -o /app/build

FROM build AS publish
RUN dotnet publish BoosterPackSimulator.sln -c Release -o /app/publish --self-contained true

#FROM base AS final
#WORKDIR /app
#COPY --from=publish /app ./publish
#COPY --from=build /app ./build
#ENTRYPOINT ["dotnet", "BoosterPackSimulator.dll"]

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=publish /app/publish/wwwroot .
COPY nginx.conf /etc/nginx/nginx.conf
