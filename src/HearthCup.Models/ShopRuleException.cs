namespace HearthCup.Models;

/// <summary>A rejected request that is the user's fault. Controllers turn this into a message, not a 500.</summary>
public sealed class ShopRuleException(string message) : Exception(message);
