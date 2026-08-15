using Microsoft.AspNetCore.Components.Rendering;

namespace CymruBlazor.Components.Core;

public static class RenderTreeBuilderExtensions
{
    public static int AddAttributes(
        this RenderTreeBuilder builder,
        int sequence,
        AriaAttributes attributes)
    {
        foreach (var attribute in attributes.AsDictionary())
        {
            builder.AddAttribute(
                sequence++,
                attribute.Key,
                attribute.Value);
        }

        return sequence;
    }
}
