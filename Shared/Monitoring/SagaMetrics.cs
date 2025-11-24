// using System.Diagnostics.Metrics;
//
// namespace Shared.Monitoring;
//
// public static class SagaMetrics
// {
//     public static readonly Meter SagaMeter = new("Saga.Metrics");
//
//     public static readonly Counter<long> OrderCreatedCount = SagaMeter.CreateCounter<long>(
//         "saga.order.created",
//         "Count",
//         "Number of orders created");
//     
//     public static readonly Counter<long> OrderCompletedCount = SagaMeter.CreateCounter<long>(
//         "saga.order.completed",
//         "Count", 
//         "Number of orders completed successfully");
//     
//     public static readonly Counter<long> OrderFailedCount = SagaMeter.CreateCounter<long>(
//         "saga.order.failed",
//         "Count",
//         "Number of orders failed");
//     
//     public static readonly Counter<long> CompensationTriggeredCount = SagaMeter.CreateCounter<long>(
//         "saga.compensation.triggered",
//         "Count",
//         "Number of compensations triggered");
//     
//     public static readonly Counter<long> StockReservedCount = SagaMeter.CreateCounter<long>(
//         "saga.stock.reserved", 
//         "Count",
//         "Number of stock reservations");
//     
//     public static readonly Counter<long> PaymentProcessedCount = SagaMeter.CreateCounter<long>(
//         "saga.payment.processed",
//         "Count",
//         "Number of payments processed");
//
//     public static readonly Histogram<double> OrderProcessingTime = SagaMeter.CreateHistogram<double>(
//         "saga.order.processing.time",
//         "seconds",
//         "Time taken to process order");
//     
//     public static readonly Histogram<double> StockReservationTime = SagaMeter.CreateHistogram<double>(
//         "saga.stock.reservation.time",
//         "seconds", 
//         "Time taken to reserve stock");
//     
//     public static readonly Histogram<double> PaymentProcessingTime = SagaMeter.CreateHistogram<double>(
//         "saga.payment.processing.time",
//         "seconds",
//         "Time taken to process payment");
//
//     public static readonly ObservableGauge<int> ActiveOrdersGauge = SagaMeter.CreateObservableGauge<int>(
//         "saga.orders.active",
//         () => new Measurement<int>(OrderStateService.ActiveOrdersCount),
//         "Count",
//         "Number of active orders in progress");
//     
//     public static readonly ObservableGauge<int> FailedOrdersGauge = SagaMeter.CreateObservableGauge<int>(
//         "saga.orders.failed",
//         () => new Measurement<int>(OrderStateService.FailedOrdersCount),
//         "Count",
//         "Number of failed orders");
// }