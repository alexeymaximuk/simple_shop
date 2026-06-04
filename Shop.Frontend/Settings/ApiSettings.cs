namespace Shop.Frontend.Settings;

public class ApiSettings
{
    public const string ApiSettingsName = "ApiSettings";
    public required string UsersAPI { get; set; }
    public required string ProductsAPI { get; set; }
}