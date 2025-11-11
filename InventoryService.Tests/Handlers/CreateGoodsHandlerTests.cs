using FluentAssertions;
using InventoryService.Application.Handlers;
using InventoryService.Dto;
using InventoryService.Infrastructure;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Serilog;

namespace InventoryService.Tests.Handlers;

[TestFixture]
public class CreateGoodsHandlerTests
{
    private InventoryDbContext _context;
    private CreateGoodsHandler _handler;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<InventoryDbContext>()
            .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
            .Options;

        _context = new InventoryDbContext(options);
        var logger = Substitute.For<ILogger>();
        _handler = new CreateGoodsHandler(_context, logger);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ShouldCreateGoods_WhenAllValid()
    {
        var dtos = new List<CreateGoodDto>
        {
            new("A1", "Apple", 3, 5),
            new("B2", "Banana", 5, 10)
        };
        var command = new CreateGoodCommand(dtos);

        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new[] { "A1", "B2" });
        _context.Goods.Count().Should().Be(2);
    }

    [Test]
    public async Task ExecuteAsync_ShouldSkipExistingGoods()
    {
        var existing = Good.Create(new CreateGoodDto("A1", "AlreadyExist", 5, 10)).Value;
        await _context.Goods.AddAsync(existing);
        await _context.SaveChangesAsync();

        var dtos = new List<CreateGoodDto>
        {
            new("A1", "Apple", 3, 5),
            new("B2", "Banana", 8, 10)
        };
        var command = new CreateGoodCommand(dtos);

        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(new[] { "B2" });
        _context.Goods.Count().Should().Be(2);
        _context.Goods.Any(g => g.Sku == "A1" && g.Name == "AlreadyExist").Should().BeTrue();
    }

    [Test]
    public async Task ExecuteAsync_ShouldReturnEmpty_WhenAllInvalid()
    {
        var dtos = new List<CreateGoodDto>
        {
            new("", "NoSku", 3 ,5),         
            new("C3", "", -9, 10),            
            new("D4", "Negative", 8, -5)     
        };
        var command = new CreateGoodCommand(dtos);

        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        result.Value.Should().BeEmpty();
        _context.Goods.Should().BeEmpty();
    }

    [Test]
    public async Task ExecuteAsync_ShouldNormalizeSku_AndAvoidDuplicatesByCase()
    {
        var dtos = new List<CreateGoodDto>
        {
            new("abc", "Apple", 8, 3),
            new("ABC", "AppleDuplicate", 12, 5)
        };
        var command = new CreateGoodCommand(dtos);

        var result = await _handler.ExecuteAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.Should().Contain("ABC");
        _context.Goods.Should().HaveCount(1);
    }
}