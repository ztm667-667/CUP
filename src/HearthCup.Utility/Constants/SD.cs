namespace HearthCup.Utility.Constants;

/// <summary>
/// Shared constants. The fourth project of the n-tier layout exists so that a limit or a key is written
/// once instead of being copied into the repository, the service and the controller.
/// </summary>
public static class SD
{
    // Catalogue paging and search.
    public const int PageSize = 8;
    public const int MaxSearchLength = 80;

    // Basket limits.
    public const int MaxQuantity = 99;
    public const int MaxBasketLines = 50;
    public const string SessionBasket = "basket";

    // User feedback handed between actions.
    public const string TempDataSuccess = "Success";
    public const string TempDataError = "Error";

    // Security.
    public const string RoleManager = "Manager";
    public const string LoginRateLimitPolicy = "login";
}
