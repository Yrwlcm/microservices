using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using PaymentService.Application.Handlers.Account;
using PaymentService.Dto.Account;
using PaymentService.Infrastructure;
using Serilog;

namespace PaymentService.Tests.HandlersTests.Account;

[TestFixture]
public class CreateAccountHandlerTests
{
    private PaymentDbContext _context;
    private ILogger _logger;

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
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ShouldFail_WhenAccountAlreadyExists()
    {
        var existing = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
        await _context.Accounts.AddAsync(existing);
        await _context.SaveChangesAsync();

        var handler = new CreateAccountHandler(_context, _logger);
        var command = new CreateAccountCommand(existing.Id.Value);

        var result = await handler.ExecuteAsync(command);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("существует");
        (await _context.Accounts.CountAsync()).Should().Be(1);
    }

    [Test]
    public async Task ExecuteAsync_ShouldCreateAccount_WhenNotExists()
    {
        var handler = new CreateAccountHandler(_context, _logger);
        var cmd = new CreateAccountCommand(Guid.NewGuid());

        var result = await handler.ExecuteAsync(cmd);

        result.IsSuccess.Should().BeTrue();
        var accountInDb = await _context.Accounts.FirstAsync(a => a.Id.Value == result.Value);
        accountInDb.Balance.Should().Be(0);
    }
}