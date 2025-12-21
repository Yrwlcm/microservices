using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PaymentService.Application.Handlers.Account;
using PaymentService.Dto.Account;
using PaymentService.Infrastructure;

namespace PaymentService.Tests.HandlersTests.Account;

[TestFixture]
    public class GetAccountsHandlerTests
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
        public async Task HandleAsync_ShouldReturnEmptyList_WhenNoAccounts()
        {
            var handler = new GetAccountsHandler(_context);

            var result = await handler.HandleAsync(new GetAccountsQuery(), CancellationToken.None);

            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task HandleAsync_ShouldReturnAllAccounts_WhenTheyExist()
        {
            // arrange
            var acc1 = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
            acc1.AddMoney(100);
            var acc2 = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
            acc2.AddMoney(300);
            await _context.Accounts.AddRangeAsync(acc1, acc2);
            await _context.SaveChangesAsync();

            var handler = new GetAccountsHandler(_context);

            // act
            var result = await handler.HandleAsync(new GetAccountsQuery(), CancellationToken.None);

            // assert
            result.Should().HaveCount(2);
            result.Should().ContainSingle(a => a.AccountId == acc1.Id.Value && a.Balance == 100);
            result.Should().ContainSingle(a => a.AccountId == acc2.Id.Value && a.Balance == 300);
        }

        [Test]
        public async Task HandleAsync_ShouldProjectFieldsCorrectly()
        {
            var acc = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
            acc.AddMoney(555);
            await _context.Accounts.AddAsync(acc);
            await _context.SaveChangesAsync();

            var handler = new GetAccountsHandler(_context);

            var result = await handler.HandleAsync(new GetAccountsQuery(), CancellationToken.None);

            var dto = result.Should().ContainSingle().Subject;
            dto.AccountId.Should().Be(acc.Id.Value);
            dto.Balance.Should().Be(555);
        }
    }