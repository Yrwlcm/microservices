using Contracts;
using Contracts.Messages;

namespace Outbox.Utils;

internal static class MessageTypeRegistry
{
    private static readonly Dictionary<string, Type> Contracts;

    static MessageTypeRegistry()
    {
        var contractsAssembly = typeof(OrderCreated).Assembly;
        Contracts = contractsAssembly.GetTypes()
            .Where(t => typeof(IEvent).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false })
            .ToDictionary(t => t.Name, t => t,  StringComparer.OrdinalIgnoreCase);
    }
    
    public static Type? GetContractOrDefault(string type) => Contracts.GetValueOrDefault(type);
}