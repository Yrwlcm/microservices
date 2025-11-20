using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Outbox.Tests")]

namespace Outbox;

internal static class Const
{
    public static int MaxMessagePublishingAttemptsCount => 3;
}