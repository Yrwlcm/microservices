using Outbox.Enums;
using Outbox.Extensions;
using Outbox.Models;

namespace Outbox.Tests;

[TestFixture]
    public class OutboxMessageExtensionsTests
    {
        [Test]
        public void MarkAsFailed_Should_Set_Status_Error_And_Clear_CanTryNext()
        {
            // arrange
            var message = new OutboxMessage
            {
                MessageStatus = MessageStatus.Pending,
                Error = null,
                CanTryNextAttemptOnUtc = DateTime.UtcNow.AddMinutes(5)
            };

            // act
            message.MarkAsFailed("Ошибка публицации");

            // assert
            Assert.That(message.MessageStatus, Is.EqualTo(MessageStatus.Failed));
            Assert.That(message.Error, Is.EqualTo("Ошибка публицации"));
            Assert.That(message.CanTryNextAttemptOnUtc, Is.Null);
        }

        [Test]
        public void MarkAsPublished_Should_Set_Status_Clear_Error_And_Set_ProcessedOnUtc()
        {
            // arrange
            var message = new OutboxMessage
            {
                MessageStatus = MessageStatus.Pending,
                Error = "Ошибка при прошлой попытке",
                CanTryNextAttemptOnUtc = DateTime.UtcNow.AddMinutes(3)
            };

            // act
            message.MarkAsPublished();

            // assert
            Assert.That(message.MessageStatus, Is.EqualTo(MessageStatus.Published));
            Assert.That(message.Error, Is.Null);
            Assert.That(message.CanTryNextAttemptOnUtc, Is.Null);
            Assert.That(message.ProcessedOnUtc, Is.Not.Null);
            Assert.That(message.ProcessedOnUtc!.Value, Is.GreaterThan(DateTime.UtcNow.AddSeconds(-5)));
        }

        [Test]
        public void TryPlanPublishingRetry_When_Below_Max_Attempts_Should_Increment_And_Set_CanTryNext()
        {
            // arrange
            var message = new OutboxMessage
            {
                AttemptCount = 1,
                MessageStatus = MessageStatus.Pending,
                CanTryNextAttemptOnUtc = null,
                Error = null
            };
            const string error = "Ошибка 500";

            // act
            message.TryPlanPublishingRetry(error);

            // assert
            Assert.That(message.AttemptCount, Is.EqualTo(2));
            Assert.That(message.Error, Is.EqualTo(error));
            Assert.That(message.CanTryNextAttemptOnUtc, Is.Not.Null);

            var expectedMinutes = (int)Math.Pow(2, message.AttemptCount);
            var delta = message.CanTryNextAttemptOnUtc!.Value - DateTime.UtcNow;
            Assert.That(delta.TotalMinutes, Is.EqualTo(expectedMinutes).Within(0.5));
        }

        [Test]
        public void TryPlanPublishingRetry_When_Reaches_Max_Attempts_Should_MarkAsFailed()
        {
            // arrange
            var message = new OutboxMessage
            {
                AttemptCount = Const.MaxMessagePublishingAttemptsCount - 1,
                MessageStatus = MessageStatus.Pending,
                Error = null
            };

            // act
            message.TryPlanPublishingRetry("Любая ошибка");

            // assert
            Assert.That(message.MessageStatus, Is.EqualTo(MessageStatus.Failed));
            Assert.That(message.Error, Is.EqualTo("Превышено число попыток публикации сообщения"));
            Assert.That(message.CanTryNextAttemptOnUtc, Is.Null);
        }
    }