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
public class ReleaseOrAddGoodsQuantityHandlerTests
{
    private InventoryDbContext _context;
        private ReleaseOrAddGoodsQuantityHandler _handler;
        private ILogger _logger;

        [SetUp]
        public async Task SetUp()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new InventoryDbContext(options);
            _logger = Substitute.For<ILogger>();

            var apple = Good.Create(new CreateGoodDto("A1", "Apple", 5)).Value;
            var banana = Good.Create(new CreateGoodDto("B2", "Banana", 10)).Value;
            _context.Goods.AddRange(apple, banana);
            await _context.SaveChangesAsync();

            _handler = new ReleaseOrAddGoodsQuantityHandler(_context, _logger);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task ExecuteAsync_ShouldIncreaseAvailable_WhenNormalAdd()
        {
            var command = new ReleaseOrAddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("A1", 3) });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            var good = await _context.Goods.FirstAsync(g => g.Sku == "A1");
            good.AvailableQuantity.Should().Be(8);
            good.ReservedQuantity.Should().Be(0);
        }

        [Test]
        public async Task ExecuteAsync_ShouldMoveFromReserved_WhenFromReservedTrue()
        {
            var good = await _context.Goods.FirstAsync(g => g.Sku == "B2");
            good.ReserveGoods(4);
            await _context.SaveChangesAsync();

            var command = new ReleaseOrAddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("B2", 2) },
                FromReserved: true);

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            good.AvailableQuantity.Should().Be(8);
            good.ReservedQuantity.Should().Be(2);
        }

        [Test]
        public async Task ExecuteAsync_ShouldLogWarning_WhenGoodNotFound()
        {
            var command = new ReleaseOrAddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("Z9", 5) });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _logger.Received().Warning(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public async Task ExecuteAsync_ShouldLogWarning_WhenAddFails()
        {
            var command = new ReleaseOrAddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("A1", -5) });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _logger.Received().Warning(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            var good = await _context.Goods.FirstAsync(g => g.Sku == "A1");
            good.AvailableQuantity.Should().Be(5);
        }
    
}