FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY OrderSample.sln ./
COPY Contracts/Contracts.csproj src/Contracts/
COPY OrderService/OrderService.csproj src/OrderService/
COPY InventoryWorker/InventoryWorker.csproj src/InventoryWorker/

RUN dotnet restore src/InventoryWorker/InventoryWorker.csproj

COPY . ./src/

RUN dotnet publish src/InventoryWorker/InventoryWorker.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "InventoryWorker.dll"]
