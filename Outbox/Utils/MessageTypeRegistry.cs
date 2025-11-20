using System.Runtime.CompilerServices;
using Contracts;
using Contracts.Messages;
using Contracts.Messages.Events;

[assembly: InternalsVisibleTo("Outbox.Tests")]

namespace Outbox.Utils;

internal static class MessageTypeRegistry
{
    private static readonly Dictionary<string, Type> Contracts;

    static MessageTypeRegistry()
    {
        var contractsAssembly = typeof(OrderCreated).Assembly;
        Contracts = contractsAssembly.GetTypes()
            .Where(t => (typeof(IEvent).IsAssignableFrom(t) || typeof(IRequest).IsAssignableFrom(t)) && t is { IsInterface: false, IsAbstract: false })
            .ToDictionary(t => t.Name, t => t,  StringComparer.OrdinalIgnoreCase);
    }

    public static bool AddType(string typeName, Type type) => Contracts.TryAdd(typeName, type);
    
    public static Type? GetContractOrDefault(string type) => Contracts.GetValueOrDefault(type);
}