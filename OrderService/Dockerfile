FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OrderSample.sln ./
COPY Contracts/Contracts.csproj src/Contracts/
COPY OrderService/OrderService.csproj src/OrderService/
COPY InventoryWorker/InventoryWorker.csproj src/InventoryWorker/

RUN dotnet restore src/OrderService/OrderService.csproj

COPY . ./src/

RUN dotnet publish src/OrderService/OrderService.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderService.dll"]
