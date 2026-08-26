using ReactiveUI.Builder;

namespace WoWsShipBuilder.Test;

/// <summary>
/// Initializes ReactiveUI once for the whole assembly.
/// </summary>
/// <remarks>
/// ReactiveUI 23 requires explicit initialization through the builder, which the apps do in their own entry points
/// (see WoWsShipBuilder.Web/Program.cs). Without it, constructing any ReactiveObject-derived view model throws a
/// TypeInitializationException the first time a property-changed helper is touched.
/// </remarks>
[SetUpFixture]
public class ReactiveUiSetup
{
    [OneTimeSetUp]
    public void InitializeReactiveUi()
    {
        RxAppBuilder.CreateReactiveUIBuilder().BuildApp();
    }
}
