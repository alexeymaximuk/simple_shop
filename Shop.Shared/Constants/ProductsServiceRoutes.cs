namespace Shop.Shared.Constants;

public static class ProductsServiceRoutes
{
    public const string HideUserProducts = "api/products/users/{0}/deactivate";
    public const string ShowUserProducts = "api/products/users/{0}/reactivate";
    public const string DeleteUserProducts = "api/products/users/{0}/delete";
}