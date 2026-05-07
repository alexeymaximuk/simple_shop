using System.ComponentModel.DataAnnotations;

namespace Shop.Shared.Extensions;

public static class ConfiguratorExtensions
{
    public static T GetRequiredSettings<T>(this IConfiguration configuration, string sectionName)
        where T : class
    {
        var section = configuration.GetSection(sectionName).Get<T>() 
                      ?? throw new InvalidOperationException($"Missing required setting: {sectionName}");

        return section;
    }
}