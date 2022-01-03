using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using WoWsShipBuilder.UI.ViewModels;

namespace WoWsShipBuilder.UI
{
    public class ViewLocator : IDataTemplate
    {
        public bool SupportsRecycling => false;

        [UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "All types referenced are manually included or referenced elsewhere.")]
        public IControl Build(object data)
        {
            var name = data.GetType().FullName!.Replace("ViewModel", "View");
            var type = Type.GetType(name);

            if (type != null)
            {
                return (Control)Activator.CreateInstance(type)!;
            }
            else
            {
                return new TextBlock { Text = "Not Found: " + name };
            }
        }

        public bool Match(object data)
        {
            return data is ViewModelBase;
        }
    }
}
