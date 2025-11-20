using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NSubstitute;
using PaymentService.Application.Handlers.Payment;
using PaymentService.Dto.Account;
using PaymentService.Enums;
using PaymentService.Infrastructure;
using Serilog;

namespace PaymentService.Tests.HandlersTests.Payment;

[TestFixture]
public class CreatePaymentHandlerTests
{
    private PaymentDbContext _context = null!;
    private ILogger _logger = null!;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<PaymentDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(warningsBuilder => warningsBuilder.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _context = new PaymentDbContext(options);
        _logger = Substitute.For<ILogger>();
    }

    [TearDown]
    public void TearDown() => _context.Dispose();
    
    [Test]
    public async Task ExecuteAsync_ShouldCreatePaymentWithMissingAccount_WhenAccountNotFound()
    {
        var handler = new CreatePaymentHandler(_context, _logger);
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var cmd = new CreatePaymentCommand(transaction, Guid.NewGuid(), Guid.NewGuid(), 500);

        var result = await handler.ExecuteAsync(cmd);
        await transaction.CommitAsync();
        result.Result.IsFailure.Should().BeTrue();
        var payment = await _context.Payments.SingleAsync();
        payment.PaymentStatus.Should().Be(PaymentStatus.MissingAccountFailure);
        payment.OrderPrice.Should().Be(500);
    }

    [Test]
    public async Task ExecuteAsync_ShouldRecordFailure_WhenLowBalance()
    {
        var acc = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
        await _context.Accounts.AddAsync(acc);
        await _context.SaveChangesAsync();
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var handler = new CreatePaymentHandler(_context, _logger);
        var cmd = new CreatePaymentCommand(transaction, acc.Id.Value, Guid.NewGuid(), 500);

        var result = await handler.ExecuteAsync(cmd);
        await transaction.CommitAsync();
        result.Result.IsFailure.Should().BeTrue();
        (await _context.Payments.CountAsync()).Should().Be(1);
        var payment = await _context.Payments.FirstAsync();
        payment.PaymentStatus.Should().Be(PaymentStatus.LowBalanceFailure);
    }

    [Test]
    public async Task ExecuteAsync_ShouldWithdrawAndRecordSuccess_WhenEnoughBalance()
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        var acc = Models.Account.Create(new CreateAccountDto(Guid.NewGuid()));
        acc.AddMoney(1000);
        await _context.Accounts.AddAsync(acc);
        await _context.SaveChangesAsync();

        var handler = new CreatePaymentHandler(_context, _logger);
        var cmd = new CreatePaymentCommand(transaction, acc.Id.Value, Guid.NewGuid(), 400);

        var result = await handler.ExecuteAsync(cmd);
        
        result.Result.IsSuccess.Should().BeTrue();
        await transaction.CommitAsync();
        var updatedAcc = await _context.Accounts.FirstAsync();
        updatedAcc.Balance.Should().Be(600);
        var payment = await _context.Payments.SingleAsync();
        payment.PaymentStatus.Should().Be(PaymentStatus.Success);
        payment.OrderPrice.Should().Be(400);
    }
}