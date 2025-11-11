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
public class ReserveGoodsHandlerTests
{
    private InventoryDbContext _context;
        private ReserveGoodsHandler _handler;
        private ILogger _logger;

        [SetUp]
        public async Task SetUp()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
                .Options;

            _context = new InventoryDbContext(options);
            _logger = Substitute.For<ILogger>();

            var apple = Good.Create(new CreateGoodDto("A1", "Apple", 250, 5)).Value;
            var banana = Good.Create(new CreateGoodDto("B2", "Banana", 150, 10)).Value;
            await _context.Goods.AddRangeAsync(apple, banana);
            await _context.SaveChangesAsync();

            _handler = new ReserveGoodsHandler(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_ShouldReserveGoods_WhenEnoughInStock()
        {
            var command = new ReserveGoodsCommand(new List<GoodQuantityDto>
            {
                new("A1", 3),
                new("B2", 5)
            });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Should().Be(1500);

            var apple = await _context.Goods.FirstAsync(g => g.Sku == "A1");
            apple.AvailableQuantity.Should().Be(2);
            var banana = await _context.Goods.FirstAsync(g => g.Sku == "B2");
            banana.AvailableQuantity.Should().Be(5);
        }

        [Test]
        public async Task ExecuteAsync_ShouldFail_WhenNotEnoughQuantity()
        {
            var command = new ReserveGoodsCommand(new List<GoodQuantityDto>
            {
                new("A1", 20)
            });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain("Недостаточно товара");
        }

        [Test]
        public async Task ExecuteAsync_ShouldFail_WhenSomeGoodsNotFound()
        {
            var command = new ReserveGoodsCommand(new List<GoodQuantityDto>
            {
                new("A1", 2),
                new("Z9", 3)
            });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain("отсутствуют в учёте");
        }
        
}