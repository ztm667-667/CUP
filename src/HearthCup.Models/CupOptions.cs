namespace HearthCup.Models;

/// <summary>Allowed option values. Keeping them in one place stops a forged form posting an unapproved image or colour.</summary>
public static class CupOptions
{
    public static readonly string[] Categories = ["Mugs", "Tumblers", "Glassware", "Travel cups"];

    public static readonly string[] Materials = ["Ceramic", "Glass", "Stainless steel"];

    public static readonly string[] Colours =
    [
        "Oat", "Sage", "Terracotta", "Rose", "Clear", "Slate", "Almond Birch", "Blue", "Blue Corn", "Dark Blue",
        "Darkstone", "Gloss Grey", "Grey", "Lavender Mist", "Multicolour", "Pink", "Rose Quartz", "Sage Ash",
        "Vivacious", "Volcanic"
    ];

    public static readonly string[] Designs =
    [
        "oat-mug", "sage-mug", "clay-mug", "rose-mug", "clear-glass", "sage-tumbler", "oat-travel", "slate-travel",
        "amazon-b0dpgf2nx7", "amazon-b07mb5n887", "amazon-b07l8jmw7d", "amazon-b07ylmsfgf", "amazon-b0f7x26wff",
        "amazon-b003wp77z6", "amazon-b018mhr366", "amazon-b077y5mj6v", "amazon-b01cpka96c", "amazon-b09hst5jwc",
        "amazon-b0b2tjjjdm", "amazon-b0cnpmfpt2", "amazon-b0d9hrp7gv", "amazon-b08r1qz7kd", "amazon-b0d5hc811z",
        "amazon-b0b7p2h383", "amazon-b07trdwhxg", "amazon-b07xyc37fs"
    ];
}
