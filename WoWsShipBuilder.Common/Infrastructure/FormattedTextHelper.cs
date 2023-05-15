using WoWsShipBuilder.Common.Infrastructure.Localization;
using WoWsShipBuilder.DataElements.DataElements;

namespace WoWsShipBuilder.Common.Infrastructure;

public static class FormattedTextHelper
{
    public static string ConvertFormattedText(FormattedTextDataElement formattedTextDataElement, ILocalizer localizer)
    {
        string text = formattedTextDataElement.Text;
        if (formattedTextDataElement.IsTextKey)
        {
            text = formattedTextDataElement.IsTextAppLocalization ? localizer.GetAppLocalization(text).Localization : localizer.GetGameLocalization(text).Localization;
        }

        IEnumerable<string> values = formattedTextDataElement.Values;
        if (formattedTextDataElement.AreValuesKeys)
        {
            values = formattedTextDataElement.AreValuesAppLocalization ? values.Select(x => localizer.GetAppLocalization(x).Localization) : values.Select(x => localizer.GetGameLocalization(x).Localization);
        }

        return string.Format(text, values.Cast<object>().ToArray());
    }
}
