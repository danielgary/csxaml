using Csxaml.ProjectSystem.Components;
using Csxaml.ProjectSystem.Consumer;
using AdminComponents = Csxaml.ProjectSystem.Components.Admin;

namespace Csxaml.ProjectSystem.Tests;

[TestClass]
public sealed class ProjectSystemIntegrationTests
{
    [TestMethod]
    public void ComponentLibrary_EmitsStableNamespacesAndManifestMetadata()
    {
        Assert.AreEqual("Csxaml.ProjectSystem.Components", typeof(GreetingCardComponent).Namespace);
        Assert.AreEqual("Csxaml.ProjectSystem.Components.Admin", typeof(AdminComponents.AdminBadgeComponent).Namespace);

        var assembly = typeof(GreetingCardComponent).Assembly;
        var attribute = assembly.GetCustomAttribute<CsxamlComponentManifestProviderAttribute>();

        Assert.IsNotNull(attribute);
        Assert.IsTrue(typeof(IComponentManifestProvider).IsAssignableFrom(attribute.ProviderType));

        var provider = (IComponentManifestProvider?)Activator.CreateInstance(attribute.ProviderType, true);
        Assert.IsNotNull(provider);

        var manifest = provider.GetManifest();

        Assert.IsTrue(
            manifest.Components.Any(
                component =>
                    component.Name == "GreetingCard" &&
                    component.NamespaceName == "Csxaml.ProjectSystem.Components" &&
                    component.ComponentTypeName == "Csxaml.ProjectSystem.Components.GreetingCardComponent"));
        Assert.IsTrue(
            manifest.Components.Any(
                component =>
                    component.Name == "PanelSurface" &&
                    component.SupportsDefaultSlot &&
                    component.Parameters.Any(parameter => parameter.Name == "HeaderText")));
        Assert.IsTrue(
            manifest.Components.Any(
                component =>
                    component.Name == "AdminBadge" &&
                    component.NamespaceName == "Csxaml.ProjectSystem.Components.Admin"));
    }

    [TestMethod]
    public void ConsumerBoard_RendersReferencedComponentsThroughNormalProjectReferences()
    {
        var tree = (NativeElementNode)new ComponentTreeCoordinator(new ConsumerBoardComponent()).Render();

        Assert.AreEqual("Border", tree.TagName);
        Assert.IsNotNull(ProjectSystemTreeHelpers.FindByAutomationName(tree, "Panel Surface"));
        Assert.AreEqual(
            "Imported Components",
            ProjectSystemTreeHelpers.GetProperty<string>(
                ProjectSystemTreeHelpers.FindByAutomationName(tree, "Panel Header")!,
                "Text"));
        Assert.AreEqual(
            "Hello from project references",
            ProjectSystemTreeHelpers.GetProperty<string>(
                ProjectSystemTreeHelpers.FindByAutomationName(tree, "Greeting Title")!,
                "Text"));
        Assert.AreEqual(
            "Verified alias import",
            ProjectSystemTreeHelpers.GetProperty<string>(
                ProjectSystemTreeHelpers.FindByAutomationName(tree, "Admin Badge")!,
                "Text"));
    }
}
