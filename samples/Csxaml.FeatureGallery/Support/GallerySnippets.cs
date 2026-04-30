namespace Csxaml.Samples.FeatureGallery;

public static class GallerySnippets
{
    public const string NamedSlots =
        """
        component Element GallerySection(...) {
            render <Border>
                <Slot Name="Actions" />
                <Slot />
            </Border>;
        }

        <GallerySection ...>
            <GallerySection.Actions>
                <Button Content="Run" />
            </GallerySection.Actions>
            <TextBlock Text="Default body" />
        </GallerySection>
        """;

    public const string RefsAndEvents =
        """
        ElementRef<TextBox> SearchBox = new ElementRef<TextBox>();

        render <TextBox
            Ref={SearchBox}
            Text={Query.Value}
            OnKeyDown={args => LastEvent.Value = args.Key.ToString()} />;
        """;

    public const string PropertyContent =
        """
        <ControlExample>
            <ControlExample.Example>
                <TextBlock Text="Example body" />
            </ControlExample.Example>
            <ControlExample.Options>
                <CheckBox Content="Enable option" />
            </ControlExample.Options>
        </ControlExample>
        """;

    public const string AttachedProperties =
        """
        <Canvas>
            <Border
                Canvas.Left={24.0}
                Canvas.Top={20.0}
                Canvas.ZIndex={2} />
        </Canvas>
        """;

    public const string Virtualization =
        """
        foreach is retained child rendering.
        Use it for small visible lists.

        Use native virtualized controls for large collections:

        <ListView ItemsSource={Rows} />
        """;
}
