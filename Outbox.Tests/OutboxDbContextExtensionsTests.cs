using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Outbox.Enums;
using Outbox.Extensions;
using Outbox.Models;
using Outbox.Utils;

namespace Outbox.Tests;

[TestFixture]
    public class OutboxDbContextExtensionsTests
    {
        private OutboxDbContext _db;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<OutboxDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _db = new OutboxDbContext(options);
        }

        [TearDown]
        public void Cleanup()
        {
            _db.Dispose();
        }
        

        [Test]
        public async Task AddOutboxMessageAsync_Should_Add_New_Message_With_Correct_Fields()
        {
            var payload = new { OrderId = Guid.NewGuid(), Amount = 100 };

            var result = await _db.AddOutboxMessageAsync("OrderCreated", payload);
            await _db.SaveChangesAsync();

            Assert.That(result.IsSuccess, Is.True);
            var added = await _db.OutboxMessages.FirstOrDefaultAsync();

            Assert.That(added, Is.Not.Null);
            Assert.That(added!.PayloadType, Is.EqualTo("OrderCreated"));
            Assert.That(added.MessageStatus, Is.EqualTo(MessageStatus.Pending));
            Assert.That(added.AttemptCount, Is.EqualTo(0));
            Assert.That(added.CreatedOnUtc, Is.Not.EqualTo(default(DateTime)));
            Assert.That(added.CanTryNextAttemptOnUtc, Is.Not.Null);
            Assert.That(added.Payload, Does.Contain("orderId"));
        }

        [Test]
        public async Task AddOutboxMessageAsync_When_Serialization_Fails_Should_Return_Failure_Result()
        {
            var contextMock = new FailingOutboxDbContext();

            var result = await contextMock.AddOutboxMessageAsync("Broken", new { Test = 1 });

            Assert.That(result.IsSuccess, Is.False);
            Assert.That(result.Error, Is.Not.Null.Or.Empty);
        }

        [Test]
        public async Task GetNextOutboxMessageAsync_Should_Return_First_Pending_Message()
        {
            var msg1 = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                PayloadType = "KnownType",
                Payload = "{}",
                MessageStatus = MessageStatus.Pending,
                AttemptCount = 0,
                CreatedOnUtc = DateTime.UtcNow.AddMinutes(-10),
                CanTryNextAttemptOnUtc = DateTime.UtcNow.AddSeconds(-5)
            };
            var msg2 = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                PayloadType = "KnownType",
                Payload = "{}",
                MessageStatus = MessageStatus.Pending,
                AttemptCount = 0,
                CreatedOnUtc = DateTime.UtcNow,
                CanTryNextAttemptOnUtc = DateTime.UtcNow.AddSeconds(-5)
            };
            _db.OutboxMessages.AddRange(msg1, msg2);
            await _db.SaveChangesAsync();
            
            MessageTypeRegistry.AddType("KnownType", typeof(object));

            var dto = await _db.GetNextOutboxMessageAsync();

            Assert.That(dto, Is.Not.Null);
            Assert.That(dto.Outcome, Is.EqualTo(GetOutboxMessageOutcome.GotNextMessage));
            Assert.That(dto.OutboxMessage, Is.Not.Null);
            Assert.That(dto.OutboxMessage!.Id, Is.EqualTo(msg1.Id));
        }

        [Test]
        public async Task GetNextOutboxMessageAsync_Should_Return_NoRecords_When_Table_Empty()
        {
            var dto = await _db.GetNextOutboxMessageAsync();

            Assert.That(dto.Outcome, Is.EqualTo(GetOutboxMessageOutcome.NoRecords));
        }

        [Test]
        public async Task GetNextOutboxMessageAsync_Should_Return_UnknownPayloadType_When_Type_NotFound()
        {
            var msg = new OutboxMessage
            {
                Id = Guid.NewGuid(),
                PayloadType = "UnknownType",
                Payload = "{}",
                MessageStatus = MessageStatus.Pending,
                AttemptCount = 0,
                CreatedOnUtc = DateTime.UtcNow,
                CanTryNextAttemptOnUtc = DateTime.UtcNow
            };
            _db.OutboxMessages.Add(msg);
            await _db.SaveChangesAsync();

            var dto = await _db.GetNextOutboxMessageAsync();

            Assert.That(dto.Outcome, Is.EqualTo(GetOutboxMessageOutcome.UnknownPayloadType));
            Assert.That(dto.OutboxMessage, Is.Not.Null);
            Assert.That(dto.OutboxMessage!.PayloadType, Is.EqualTo("UnknownType"));
        }

        [Test]
        public async Task AddProcessedMessageAsync_Should_Add_New_Processed_Record()
        {
            var messageId = Guid.NewGuid();
            await _db.AddProcessedMessageAsync(messageId, "ConsumerX");
            await _db.SaveChangesAsync();

            var processed = await _db.OutboxProcessedMessages.FirstOrDefaultAsync();

            Assert.That(processed, Is.Not.Null);
            Assert.That(processed!.MessageId, Is.EqualTo(messageId));
            Assert.That(processed.ConsumerName, Is.EqualTo("ConsumerX"));
            Assert.That(processed.ProcessedOnUtc, Is.Not.EqualTo(default(DateTime)));
        }

        [Test]
        public async Task HasProcessedMessageAsync_Should_Return_True_When_Record_Exists()
        {
            var messageId = Guid.NewGuid();
            var processed = new OutboxProcessedMessage
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                ConsumerName = "ConsumerY",
                ProcessedOnUtc = DateTime.UtcNow
            };
            _db.OutboxProcessedMessages.Add(processed);
            await _db.SaveChangesAsync();

            var exists = await _db.HasProcessedMessageAsync(messageId, "ConsumerY");

            Assert.That(exists, Is.True);
        }

        [Test]
        public async Task HasProcessedMessageAsync_Should_Return_False_When_No_Record()
        {
            var exists = await _db.HasProcessedMessageAsync(Guid.NewGuid(), "NoSuchConsumer");
            Assert.That(exists, Is.False);
        }
        

        private class FailingOutboxDbContext : DbContext, IOutboxDbContext
        {
            public DbSet<OutboxMessage> OutboxMessages { get; set; }

            public override ValueTask<EntityEntry<TEntity>> AddAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = new CancellationToken())
            {
                throw new NotImplementedException();
            }

            public DbSet<OutboxProcessedMessage> OutboxProcessedMessages { get; set; }
        }
    }

internal class OutboxDbContext(DbContextOptions<OutboxDbContext> options) : DbContext(options), IOutboxDbContext
{
    public DbSet<OutboxMessage> OutboxMessages { get; set; }
    public DbSet<OutboxProcessedMessage> OutboxProcessedMessages { get; set; }
}