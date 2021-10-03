using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Media;

namespace WoWsShipBuilder.UI.CustomControls
{
    public class NumberedButton : ToggleButton
    {
        /// <summary>
        /// Styled Property to indicate the order number to draw.
        /// </summary>
        public static readonly StyledProperty<string> NumberProperty =
            AvaloniaProperty.Register<NumberedButton, string>(nameof(Number));

        /// <summary>
        /// Style Property to indicate the image path.
        /// </summary>
        public static readonly StyledProperty<IImage> BackgroundImageProperty =
            AvaloniaProperty.Register<NumberedButton, IImage>(nameof(BackgroundImage));

        /// <summary>
        /// Style Property to indicate the unselected border brush.
        /// </summary>
        public static readonly StyledProperty<SolidColorBrush> UnselectedBorderProperty =
            AvaloniaProperty.Register<NumberedButton, SolidColorBrush>(nameof(UnselectedBorder));

        /// <summary>
        /// Style Property to indicate the image path.
        /// </summary>
        public static readonly StyledProperty<double> UnselectedBorderThicknessProperty =
            AvaloniaProperty.Register<NumberedButton, double>(nameof(UnselectedBorderThicknessProperty));

        /// <summary>
        /// Styled Property to indicate the Horizontal spacing from the top for the Number.
        /// </summary>
        public static readonly StyledProperty<double> NumberXSpacingProperty =
            AvaloniaProperty.Register<NumberedButton, double>(nameof(NumberXSpacing), 45);

        /// <summary>
        /// Style Property to indicate the Vertical spacing from the top for the Number.
        /// </summary>
        public static readonly StyledProperty<double> NumberYSpacingProperty =
            AvaloniaProperty.Register<NumberedButton, double>(nameof(NumberYSpacing), 2.5);

        static NumberedButton()
        {
            IsCheckedProperty.Changed.AddClassHandler<NumberedButton>((x, e) => x.UpdateVisual(e));
            NumberProperty.Changed.AddClassHandler<NumberedButton>((x, e) => x.UpdateVisual(e));
            HeightProperty.OverrideDefaultValue(typeof(NumberedButton), 60);
            WidthProperty.OverrideDefaultValue(typeof(NumberedButton), 60);
            CornerRadiusProperty.OverrideDefaultValue(typeof(NumberedButton), new CornerRadius(15));
        }

        /// <summary>
        /// Gets or Sets the number to print.
        /// </summary>
        public string Number
        {
            get => GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        /// <summary>
        /// Gets or Sets the background image brush.
        /// </summary>
        public IImage BackgroundImage
        {
            get => GetValue(BackgroundImageProperty);
            set => SetValue(BackgroundImageProperty, value);
        }

        /// <summary>
        /// Gets or Sets Horizontal spacing from the top for the Number.
        /// </summary>
        public double NumberXSpacing
        {
            get => GetValue(NumberXSpacingProperty);
            set => SetValue(NumberXSpacingProperty, value);
        }

        /// <summary>
        /// Gets or Sets the Vertical spacing from the top for the Number.
        /// </summary>
        public double NumberYSpacing
        {
            get => GetValue(NumberYSpacingProperty);
            set => SetValue(NumberYSpacingProperty, value);
        }

        /// <summary>
        /// Gets or Sets the unselected border brush.
        /// </summary>
        public SolidColorBrush UnselectedBorder
        {
            get => GetValue(UnselectedBorderProperty);
            set => SetValue(UnselectedBorderProperty, value);
        }

        /// <summary>
        /// Gets or Sets the unselected border brush.
        /// </summary>
        public double UnselectedBorderThickness
        {
            get => GetValue(UnselectedBorderThicknessProperty);
            set => SetValue(UnselectedBorderThicknessProperty, value);
        }

        public override void Render(DrawingContext context)
        {
            // get control bounds size
            var rect = new Rect(Bounds.Size);
            var imageRect = rect.Deflate(BorderThickness * 2.5);
            context.DrawImage(BackgroundImage, new Rect(BackgroundImage.Size), imageRect, Avalonia.Visuals.Media.Imaging.BitmapInterpolationMode.HighQuality);

            // if it's true, draw the number. Draw the border if present)
            if (IsChecked.HasValue && IsChecked.Value)
            {
                if (BorderThickness.IsUniform && !BorderThickness.IsEmpty)
                {
                    rect = rect.Deflate(BorderThickness * 0.5);
                    var pen = new Pen(BorderBrush, BorderThickness.Top);
                    if (!CornerRadius.IsEmpty)
                    {
                        context.DrawRectangle(pen, rect, (float)CornerRadius.TopLeft);
                    }
                    else
                    {
                        context.DrawRectangle(pen, rect);
                    }
                }

                var text = new FormattedText(Number, new Typeface(FontFamily, FontStyle, FontWeight), FontSize, TextAlignment.Left, TextWrapping.NoWrap, new Size(Width, Height));
                var point = new Point(NumberXSpacing, NumberYSpacing);
                context.DrawText(Foreground, point, text);
            }
            else
            {
                if (UnselectedBorderThickness > 0)
                {
                    var pen = new Pen(UnselectedBorder, UnselectedBorderThickness);
                    context.DrawRectangle(pen, rect, (float)CornerRadius.TopLeft);
                }
            }
        }

        private void UpdateVisual(AvaloniaPropertyChangedEventArgs e)
        {
            InvalidateVisual();
        }
    }
}
