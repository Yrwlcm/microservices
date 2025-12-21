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
public class AddGoodsQuantityHandlerTests
{
    private InventoryDbContext _context;
        private AddGoodsQuantityHandler _handler;
        private ILogger _logger;

        [SetUp]
        public async Task SetUp()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new InventoryDbContext(options);
            _logger = Substitute.For<ILogger>();

            var apple = Good.Create(new CreateGoodDto("A1", "Apple", 3, 5)).Value;
            var banana = Good.Create(new CreateGoodDto("B2", "Banana", 5, 10)).Value;
            _context.Goods.AddRange(apple, banana);
            await _context.SaveChangesAsync();

            _handler = new AddGoodsQuantityHandler(_context, _logger);
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
            var command = new AddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("A1", 3) });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            var good = await _context.Goods.FirstAsync(g => g.Sku == "A1");
            good.AvailableQuantity.Should().Be(8);
        }
        
        [Test]
        public async Task ExecuteAsync_ShouldLogWarning_WhenGoodNotFound()
        {
            var command = new AddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("Z9", 5) });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _logger.Received().Warning(Arg.Any<string>(), Arg.Any<string>());
        }

        [Test]
        public async Task ExecuteAsync_ShouldLogWarning_WhenAddFails()
        {
            var command = new AddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("A1", -5) });

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            _logger.Received().Warning(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            var good = await _context.Goods.FirstAsync(g => g.Sku == "A1");
            good.AvailableQuantity.Should().Be(5);
        }
    
    
        [Test]
        public async Task ExecuteAsync_ShouldReturnFailure_WhenGoodNotFound_Strict()
        {
            var command = new AddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("Z9", 5) },
                StrictMode: true);

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain("не все позиции существуют");
            _logger.DidNotReceive().Warning(Arg.Any<string>(), Arg.Any<object[]>());
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnFailure_WhenAddFails_Strict()
        {
            var command = new AddGoodsQuantityCommand(
                new List<GoodQuantityDto> { new("A1", -5) },
                StrictMode: true);

            var result = await _handler.ExecuteAsync(command, CancellationToken.None);

            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain("не удалось увеличить количество");
            _logger.DidNotReceive().Warning(Arg.Any<string>(), Arg.Any<object[]>());
        }
}