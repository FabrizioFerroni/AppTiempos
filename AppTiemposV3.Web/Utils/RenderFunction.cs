using AppTiemposV3.Web.Components.Icons;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Utils;

public static class RenderHFunction
{
    public static void Render<TComponent>(RenderTreeBuilder builder)
        where TComponent : IComponent
    {
        builder.OpenComponent<TComponent>(0);
        builder.AddAttribute(1, "Class", "h-4 w-4");
        builder.CloseComponent();
    }

    public static void RenderProfile<TComponent>(RenderTreeBuilder builder)
        where TComponent : IComponent
    {
        builder.OpenComponent<TComponent>(0);
        builder.AddAttribute(1, "Class", "h-4 w-4 text-gray-500 dark:text-gray-400");
        builder.CloseComponent();
    }

    public static void RenderDefault(RenderTreeBuilder builder)
    {
        builder.OpenComponent<AlertCircle>(0);
        builder.AddAttribute(1, "Class", "h-4 w-4");
        builder.CloseComponent();
    }
}