using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PaymentService.Application.Handlers.Account;
using PaymentService.Dto.Account;
using PaymentService.Infrastructure;
using Serilog;

namespace PaymentService.Tests.HandlersTests.Account;

[TestFixture]
    public class TopUpAccountHandlerTests
    {
        private PaymentDbContext _context = null!;
        private ILogger _logger = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<PaymentDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new PaymentDbContext(options);
            _logger = Substitute.For<ILogger>();
        }

        [TearDown]
        public void TearDown() => _context.Dispose();

        [Test]
        public async Task ExecuteAsync_ShouldReturnFailure_WhenAccountNotFound()
        {
            var handler = new TopUpAccountHandler(_context, _logger);
            var cmd = new TopUpAccountCommand(Guid.NewGuid(), 100);

            var result = await handler.ExecuteAsync(cmd);

            result.IsFailure.Should().BeTrue();
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnFailure_WhenAmountZeroOrNegative()
        {
            var account = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var handler = new TopUpAccountHandler(_context, _logger);
            var cmd = new TopUpAccountCommand(account.Id.Value, -33);

            var result = await handler.ExecuteAsync(cmd);

            result.IsFailure.Should().BeTrue();
            account.Balance.Should().Be(0);
        }

        [Test]
        public async Task ExecuteAsync_ShouldIncreaseBalance_AndReturnSuccess()
        {
            var account = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
            await _context.Accounts.AddAsync(account);
            await _context.SaveChangesAsync();

            var handler = new TopUpAccountHandler(_context, _logger);
            var cmd = new TopUpAccountCommand(account.Id.Value, 100);

            var result = await handler.ExecuteAsync(cmd);

            result.IsSuccess.Should().BeTrue();

            var updated = await _context.Accounts.FirstAsync(a => a.Id == account.Id);
            updated.Balance.Should().Be(100);
        }
    }