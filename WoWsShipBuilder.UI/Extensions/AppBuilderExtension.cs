using Avalonia.Controls;
using Avalonia.ReactiveUI;
using Avalonia.Threading;
using ReactiveUI;
using Splat;

namespace WoWsShipBuilder.UI.Extensions
{
    public static class AppBuilderExtension
    {
        /// <summary>
        /// <see cref="AppBuilderExtensions.UseReactiveUI{TAppBuilder}"/>
        /// Modified version to ensure Avalonia is initialized instead of Blazor due to changed enum ordinals in reactive ui.
        /// </summary>
        /// <inheritdoc cref="AppBuilderExtensions.UseReactiveUI{TAppBuilder}"/>
        public static TAppBuilder UseUpdatedReactiveUI<TAppBuilder>(this TAppBuilder builder)
            where TAppBuilder : AppBuilderBase<TAppBuilder>, new() =>
            builder.AfterPlatformServicesSetup(_ => Locator.RegisterResolverCallbackChanged(() =>
            {
                if (Locator.CurrentMutable is null)
                {
                    return;
                }

                PlatformRegistrationManager.SetRegistrationNamespaces(RegistrationNamespace.Avalonia);
                RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
                Locator.CurrentMutable.RegisterConstant(new AvaloniaActivationForViewFetcher(), typeof(IActivationForViewFetcher));
                Locator.CurrentMutable.RegisterConstant(new AutoDataTemplateBindingHook(), typeof(IPropertyBindingHook));
            }));

        public static void InitializeAvalonia(this IMutableDependencyResolver resolver)
        {
            RxApp.MainThreadScheduler = AvaloniaScheduler.Instance;
            resolver.RegisterConstant(new AvaloniaActivationForViewFetcher(), typeof(IActivationForViewFetcher));
            resolver.RegisterConstant(new AutoDataTemplateBindingHook(), typeof(IPropertyBindingHook));
        }
    }
}
