FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OrderSample.sln ./
COPY src/Contracts/Contracts.csproj src/Contracts/
COPY src/OrderService/OrderService.csproj src/OrderService/
COPY src/InventoryWorker/InventoryWorker.csproj src/InventoryWorker/

RUN dotnet restore src/InventoryWorker/InventoryWorker.csproj

COPY src/ ./src/

RUN dotnet publish src/InventoryWorker/InventoryWorker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "InventoryWorker.dll"]
