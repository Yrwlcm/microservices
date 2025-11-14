using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Handlers.Account;
using PaymentService.Dto.Account;
using PaymentService.Infrastructure;

namespace PaymentService.Tests.HandlersTests.Account;

[TestFixture]
    public class GetAccountHandlerTests
    {
        private PaymentDbContext _context;

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

        [Test]
        public async Task HandleAsync_ShouldReturnDto_WhenAccountExists()
        {
            // arrange
            var acc = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
            acc.AddMoney(250);
            await _context.Accounts.AddAsync(acc);
            await _context.SaveChangesAsync();

            var handler = new GetAccountHandler(_context);
            var query = new GetAccountQuery(acc.Id.Value);

            // act
            var result = await handler.HandleAsync(query, CancellationToken.None);

            // assert
            result.Should().NotBeNull();
            result!.AccountId.Should().Be(acc.Id.Value);
            result.Balance.Should().Be(acc.Balance);
        }

        [Test]
        public async Task HandleAsync_ShouldReturnNull_WhenAccountNotFound()
        {
            var handler = new GetAccountHandler(_context);
            var query = new GetAccountQuery(Guid.NewGuid());

            var result = await handler.HandleAsync(query, CancellationToken.None);

            result.Should().BeNull();
        }

        [Test]
        public async Task HandleAsync_ShouldSelectCorrectAccount_WhenSeveralExist()
        {
            var acc1 = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
            acc1.AddMoney(100);
            var acc2 = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
            acc2.AddMoney(500);
            await _context.Accounts.AddRangeAsync(acc1, acc2);
            await _context.SaveChangesAsync();

            var handler = new GetAccountHandler(_context);
            var query = new GetAccountQuery(acc2.Id.Value);

            var result = await handler.HandleAsync(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Balance.Should().Be(500);
        }
    }