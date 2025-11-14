using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application;
using PaymentService.Application.Handlers.Payment;
using PaymentService.Dto.Payment;
using PaymentService.Enums;
using PaymentService.Infrastructure;
using PaymentService.Models;

namespace PaymentService.Tests.HandlersTests.Payment;

[TestFixture]
    public class GetPaymentsHandlerTests
    {
        private PaymentDbContext _context = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _context = new PaymentDbContext(options);
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        private async Task<List<Models.Payment>> SeedPaymentsAsync()
        {
            var acc1 = new AccountId(Guid.NewGuid());
            var acc2 = new AccountId(Guid.NewGuid());
            var o1 = new OrderId(Guid.NewGuid());
            var o2 = new OrderId(Guid.NewGuid());

            var list = new[]
            {
                new CreatePaymentDto(acc1, o1, DateTime.UtcNow.AddMinutes(-10), PaymentStatus.Success, 100m),
                new CreatePaymentDto(acc1, o2, DateTime.UtcNow.AddMinutes(-5), PaymentStatus.Success, 200m),
                new CreatePaymentDto(acc2, o1, DateTime.UtcNow.AddMinutes(-1), PaymentStatus.LowBalanceFailure, 300m)
            }.Select(dto => Models.Payment.Create(dto).Value).ToList();

            await _context.Payments.AddRangeAsync(list);
            await _context.SaveChangesAsync();
            return list;
        }

        [Test]
        public async Task HandleAsync_ShouldReturnAll_WhenNoFilters()
        {
            await SeedPaymentsAsync();
            var handler = new GetPaymentsHandler(_context);
            var query = new GetPaymentsQuery(new PaymentsSearchFilter());

            var result = await handler.HandleAsync(query, CancellationToken.None);

            result.Should().HaveCount(3);
            result.Should().BeInDescendingOrder(p => p.FinishedOnUtc);
        }

        [Test]
        public async Task HandleAsync_ShouldFilterByAccountId()
        {
            var payments = await SeedPaymentsAsync();
            var accountId = payments.First().AccountId.Value;

            var handler = new GetPaymentsHandler(_context);
            var filter = new PaymentsSearchFilter(AccountId: accountId);
            var query = new GetPaymentsQuery(filter);

            var result = await handler.HandleAsync(query, CancellationToken.None);

            result.Should().OnlyContain(p => p.AccountId == accountId);
        }

        [Test]
        public async Task HandleAsync_ShouldFilterByOrderId()
        {
            var payments = await SeedPaymentsAsync();
            var orderId = payments.Last().OrderId.Value;

            var handler = new GetPaymentsHandler(_context);
            var filter = new PaymentsSearchFilter(OrderId: orderId);
            var query = new GetPaymentsQuery(filter);

            var result = await handler.HandleAsync(query, CancellationToken.None);

            result.Should().OnlyContain(p => p.OrderId == orderId);
        }

        [Test]
        public async Task HandleAsync_ShouldPaginate()
        {
            var payments = await SeedPaymentsAsync();
            var handler = new GetPaymentsHandler(_context);
            
            var filter = new PaymentsSearchFilter(Page: 2, Limit: 1);
            var query = new GetPaymentsQuery(filter);

            var result = await handler.HandleAsync(query, CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].FinishedOnUtc.Should().Be(
                payments.OrderByDescending(p => p.FinishedOnUtc).Skip(1).First().FinishedOnUtc);
        }

        [Test]
        public async Task HandleAsync_ShouldReturnEmpty_WhenNoMatch()
        {
            await SeedPaymentsAsync();
            var handler = new GetPaymentsHandler(_context);
            var query = new GetPaymentsQuery(new PaymentsSearchFilter(AccountId: Guid.NewGuid()));

            var result = await handler.HandleAsync(query, CancellationToken.None);

            result.Should().BeEmpty();
        }
    }