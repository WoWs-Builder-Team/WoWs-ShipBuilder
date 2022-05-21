﻿using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using DynamicData;
using WoWsShipBuilder.Core.Translations;
using WoWsShipBuilder.UI.Settings;

namespace WoWsShipBuilder.UI.CustomControls;

public class DepthChargeDamageDistributionChart : TemplatedControl
{
    private const int MaxRadius = 250;

    /// <summary>
    /// Styled Property for the depth charge damage.
    /// </summary>
    public static readonly StyledProperty<int> DcDmgProperty =
        AvaloniaProperty.Register<DepthChargeDamageDistributionChart, int>(nameof(DcDmg));

    /// <summary>
    /// Styled Property for the depth charge damage.
    /// </summary>
    public static readonly StyledProperty<float> SplashRadiusProperty =
        AvaloniaProperty.Register<DepthChargeDamageDistributionChart, float>(nameof(SplashRadius));

    /// <summary>
    /// Styled Property for the distribution of the depth charge damage.
    /// </summary>
    public static readonly StyledProperty<Dictionary<float, List<float>>> PointsOfDmgProperty =
        AvaloniaProperty.Register<DepthChargeDamageDistributionChart, Dictionary<float, List<float>>>(nameof(PointsOfDmg));

    /// <summary>
    /// Gets or sets the depth charge damage.
    /// </summary>
    public int DcDmg
    {
        get => GetValue(DcDmgProperty);
        set => SetValue(DcDmgProperty, value);
    }

    /// <summary>
    /// Gets or sets the depth charge damage.
    /// </summary>
    public float SplashRadius
    {
        get => GetValue(SplashRadiusProperty);
        set => SetValue(SplashRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the depth charge damage distribution.
    /// </summary>
    public Dictionary<float, List<float>> PointsOfDmg
    {
        get => GetValue(PointsOfDmgProperty);
        set => SetValue(PointsOfDmgProperty, value);
    }

    /// <summary>
    /// Renders the content of the control, drawing the areas.
    /// For more details, see <see cref="Visual.Render"/>.
    /// </summary>
    /// <param name="context">The <see cref="DrawingContext"/> used to draw the visualization.</param>
    public override void Render(DrawingContext context)
    {
        var rect = new Rect(0, 0, Bounds.Width, Bounds.Height);
        var center = rect.Center;

        DrawBackGround(context, center);

        foreach (var data in PointsOfDmg)
        {
            int index = PointsOfDmg.IndexOf(data);
            double opacity = 1.0 / (PointsOfDmg.Count - index);

            DrawDistribution(context, center, data.Value.First(), opacity);
            DrawExtraElements(context, center);
            DrawText(context, center, data.Key, data.Value.First());
        }
    }

    private static void DrawDistribution(DrawingContext context, Point center, float radiusCoeff, double opacity)
    {
        var filling = new SolidColorBrush(Colors.DarkOrange, opacity);

        var rad = radiusCoeff * MaxRadius;
        Point circleCenter = new(center.X - rad, center.Y - rad);
        Rect circleRectangle = new(circleCenter, new Size(rad * 2, rad * 2));
        EllipseGeometry circle = new(circleRectangle);

        context.DrawGeometry(filling, new Pen(Brushes.Black, 3), circle);
    }

    private static void DrawBackGround(DrawingContext context, Point center)
    {
        const int offset = 25;

        var filling = new SolidColorBrush(Colors.Black, 0.1);

        Point circleCenter = new(center.X - MaxRadius - offset, center.Y - MaxRadius - offset);
        Rect circleRectangle = new(circleCenter, new Size((MaxRadius + offset ) * 2, (MaxRadius + offset) * 2));
        RectangleGeometry rect = new(circleRectangle);

        context.DrawGeometry(filling, new Pen(Brushes.Black, 5), rect);
    }

    private void DrawExtraElements(DrawingContext context, Point center)
    {
        const int delta = 20;
        context.DrawLine(new Pen(Brushes.Black, 2, DashStyle.DashDotDot), new Point(center.X, center.Y - MaxRadius - delta), new Point(center.X, center.Y + MaxRadius + delta));
        context.DrawLine(new Pen(Brushes.Black, 2, DashStyle.DashDotDot), new Point(center.X + MaxRadius + delta, center.Y), new Point(center.X - MaxRadius - delta, center.Y));
    }

    private void DrawText(DrawingContext context, Point center, float dmgCoeff, float radiusCoeff)
    {
        double rad = radiusCoeff * MaxRadius * Math.Sqrt(2) / 2;

        var typeface = AppSettingsHelper.Settings.SelectedLanguage.LocalizationFileName == "ja" ? new("Yu Gothic UI", Typeface.Default.Style, FontWeight.Bold) : new Typeface(FontFamily.DefaultFontFamilyName, Typeface.Default.Style, FontWeight.Bold);
        var brush = Foreground;
        const int fontSize = 11;

        var dmg = new FormattedText($"{Math.Round(DcDmg * dmgCoeff)} dmg", typeface, fontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);
        var range = new FormattedText($"{Math.Round(SplashRadius * radiusCoeff, 1)} {Translation.Unit_M}", typeface, fontSize, TextAlignment.Left, TextWrapping.NoWrap, Size.Infinity);

        context.DrawText(brush, new (center.X + rad - dmg.Bounds.Width, center.Y - rad), dmg);
        context.DrawText(brush, new (center.X - rad - range.Bounds.Width, center.Y - rad - range.Bounds.Height), range);
    }
}
