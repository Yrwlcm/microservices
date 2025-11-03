# Order Sample

Минимальный пример сервиcа заказов на .NET 8, который публикует сообщения в RabbitMQ через Rebus и обрабатывает их отдельным воркером.

## Структура

- `OrderSample.sln` — solution, объединяющий все проекты.
- `src/Contracts` — общие контракты сообщений (`OrderCreated`).
- `src/OrderService` — Minimal API с эндпоинтами `POST /order` и `GET /healthz`.
- `src/InventoryWorker` — консольный подписчик на `OrderCreated`.
- `docker-compose.yml` — среда запуска с RabbitMQ, API и воркером.

## Быстрый старт

```bash
dotnet --version
dotnet restore OrderSample.sln
dotnet build OrderSample.sln
dotnet test OrderSample.sln
docker compose up -d --build
```

Проверить состояние RabbitMQ можно по адресу http://localhost:15672 (логин/пароль: `guest/guest`).
Swagger UI доступен после запуска по адресу http://localhost:8080/swagger (главная страница сервиса автоматически редиректит туда).

### Тестовый запрос

```bash
curl -X POST http://localhost:8080/order \
  -H "Content-Type: application/json" \
  -d '{"orderId":"8f6a7e6a-3f7f-4f0b-b1ea-1f1b1a1a1a1a","items":[{"sku":"ABC","qty":2},{"sku":"XYZ","qty":1}]}'
```

Ожидается ответ `202 Accepted`, а в логах контейнера `inventory-worker` должно появиться сообщение о получении заказа.

## Отладка

- Убедитесь, что `Rabbit__ConnectionString` корректно прокинут в контейнеры (`docker compose config`).
- Проверьте доступность RabbitMQ по `docker compose logs rabbitmq` и в UI `http://localhost:15672`.
- Очереди `orders-api` и `inventory-worker` должны появиться после первого запуска сервисов.

Локальная разработка
- Для локальной проверки без Docker достаточно запустить `dotnet run --project src/OrderService` и `dotnet run --project src/InventoryWorker`, предварительно подняв RabbitMQ.
