using System.Reflection;

namespace Csxaml.FeatureGallery.UiTests;

internal static class FeatureGalleryPaths
{
    public static string ExecutablePath
    {
        get
        {
            var metadata = Assembly.GetExecutingAssembly()
                .GetCustomAttributes<AssemblyMetadataAttribute>()
                .FirstOrDefault(attribute => attribute.Key == "FeatureGalleryExecutable");

            Assert.IsNotNull(metadata?.Value, "FeatureGalleryExecutable assembly metadata was not generated.");
            return metadata.Value;
        }
    }
}
