using FluentAssertions;
using InventoryService.Application.Handlers;
using InventoryService.Dto;
using InventoryService.Infrastructure;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Tests.Handlers;

[TestFixture]
public class GetGoodsHandlerTests
{
    private InventoryDbContext _context;
        private GetGoodsHandler _handler;

        [SetUp]
        public async Task SetUp()
        {
            var options = new DbContextOptionsBuilder<InventoryDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new InventoryDbContext(options);

            var goodsToAdd = new[]
            {
                new CreateGoodDto("A1", "Apple", 3, 5),
                new CreateGoodDto("B2", "Banana", 5, 8),
                new CreateGoodDto("C3", "Cherry", 8, 12),
                new CreateGoodDto("D4", "Dates", 13, 2)
            };

            foreach (var dto in goodsToAdd)
            {
                var res = Good.Create(dto);
                _context.Goods.Add(res.Value);
            }
            
            await _context.SaveChangesAsync();

            _handler = new GetGoodsHandler(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Test]
        public async Task HandleAsync_ShouldReturnAll_WhenWithinLimit()
        {
            var query = new GetGoodsQuery(new GoodsFilter(Page: 1, Limit: 10));

            var result = await _handler.HandleAsync(query, CancellationToken.None);

            result.Should().HaveCount(4);
            result.Select(x => x.Sku).Should().Contain(new[] { "A1", "B2", "C3", "D4" });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnPagedResults_WhenLimitSmaller()
        {
            var query = new GetGoodsQuery(new GoodsFilter(Page: 2, Limit: 2));

            var result = await _handler.HandleAsync(query, CancellationToken.None);

            result.Should().HaveCount(2);
            result.Select(x => x.Sku).Should().Contain(new[] { "C3", "D4" });
        }

        [Test]
        public async Task HandleAsync_ShouldReturnEmpty_WhenNoGoods()
        {
            _context.Goods.RemoveRange(_context.Goods);
            await _context.SaveChangesAsync();

            var query = new GetGoodsQuery(new GoodsFilter(Page: 1, Limit: 10));
            var result = await _handler.HandleAsync(query, CancellationToken.None);

            result.Should().BeEmpty();
        }
}