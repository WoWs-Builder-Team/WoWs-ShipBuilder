using System.Globalization;
using WoWsShipBuilder.DataElements;
using WoWsShipBuilder.Infrastructure.Localization;

namespace WoWsShipBuilder.Features.ShipStats;

public static class FormattedTextHelper
{
    public static string ConvertFormattedText(FormattedTextDataElement formattedTextDataElement, ILocalizer localizer)
    {
        string text = formattedTextDataElement.ValueTextKind switch
        {
            DataElementTextKind.Plain => formattedTextDataElement.Text,
            DataElementTextKind.LocalizationKey => localizer.SimpleGameLocalization(formattedTextDataElement.Text),
            DataElementTextKind.AppLocalizationKey => localizer.SimpleAppLocalization(formattedTextDataElement.Text),
            _ => throw new NotSupportedException("Invalid value for ValueTextKind"),
        };

        IEnumerable<string> values = formattedTextDataElement.ArgumentsTextKind switch
        {
            DataElementTextKind.Plain => formattedTextDataElement.Arguments,
            DataElementTextKind.LocalizationKey => formattedTextDataElement.Arguments.Select(localizer.SimpleGameLocalization),
            DataElementTextKind.AppLocalizationKey => formattedTextDataElement.Arguments.Select(localizer.SimpleAppLocalization),
            _ => throw new NotSupportedException("Invalid value for ArgumentsTextKind"),
        };

        return string.Format(CultureInfo.InvariantCulture, text, values.Cast<object>().ToArray());
    }
}
