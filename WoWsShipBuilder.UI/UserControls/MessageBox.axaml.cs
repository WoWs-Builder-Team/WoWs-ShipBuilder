using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace WoWsShipBuilder.UI.UserControls
{
    public partial class MessageBox : Window
    {
        public MessageBox()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public enum MessageBoxButtons
        {
            Ok,
            OkCancel,
            YesNo,
            YesNoCancel,
        }

        public enum MessageBoxResult
        {
            Ok,
            Cancel,
            Yes,
            No,
        }

        public enum MessageBoxIcon
        {
            None,
            Error,
            Info,
            Question,
            Warning,
        }

        public static Task<MessageBoxResult> Show(Window? parent, string text, string title, MessageBoxButtons buttons, MessageBoxIcon icon = MessageBoxIcon.None, int width = 300, int height = 150, SizeToContent sizeToContent = SizeToContent.Manual)
        {
            var msgbox = new MessageBox
            {
                ShowActivated = true,
                ShowInTaskbar = true,
                Title = title,
                SizeToContent = sizeToContent,
            };

            msgbox.FindControl<AppHeader>("Header").Title = title;

            msgbox.Width = width;
            msgbox.Height = height;

            if (icon != MessageBoxIcon.None)
            {
                var iconControl = msgbox.FindControl<Image>("Icon");
                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                Bitmap? bitmap = null;
                switch (icon)
                {
                    case MessageBoxIcon.Error:
                        bitmap = new Bitmap(assets.Open(new Uri("avares://WoWsShipBuilder/Assets/Icons/Error.png")));
                        break;
                    case MessageBoxIcon.Info:
                        bitmap = new Bitmap(assets.Open(new Uri("avares://WoWsShipBuilder/Assets/Icons/Info.png")));
                        break;
                    case MessageBoxIcon.Question:
                        bitmap = new Bitmap(assets.Open(new Uri("avares://WoWsShipBuilder/Assets/Icons/Question.png")));
                        break;
                    case MessageBoxIcon.Warning:
                        bitmap = new Bitmap(assets.Open(new Uri("avares://WoWsShipBuilder/Assets/Icons/Warning.png")));
                        break;
                }

                iconControl.Source = bitmap;
            }

            msgbox.FindControl<TextBlock>("Text").Text = text;
            var buttonPanel = msgbox.FindControl<StackPanel>("Buttons");

            var res = MessageBoxResult.Ok;

            void AddButton(string caption, MessageBoxResult r, bool def = false)
            {
                var btn = new Button { Content = caption };
                btn.Click += (_, _) =>
                {
                    res = r;
                    msgbox.Close();
                };
                btn.Width = 60;

                switch (r)
                {
                    case MessageBoxResult.Ok:
                    case MessageBoxResult.Yes:
                        btn.IsDefault = true;
                        break;
                    case MessageBoxResult.Cancel:
                        btn.IsCancel = true;
                        break;
                }

                if (r == MessageBoxResult.Cancel)
                {
                    btn.IsCancel = true;
                }

                buttonPanel.Children.Add(btn);
                if (def)
                {
                    res = r;
                }
            }

            if (buttons == MessageBoxButtons.Ok || buttons == MessageBoxButtons.OkCancel)
            {
                AddButton("Ok", MessageBoxResult.Ok, true);
            }

            if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.YesNoCancel)
            {
                AddButton("Yes", MessageBoxResult.Yes);
                AddButton("No", MessageBoxResult.No, true);
            }

            if (buttons == MessageBoxButtons.OkCancel || buttons == MessageBoxButtons.YesNoCancel)
            {
                AddButton("Cancel", MessageBoxResult.Cancel, true);
            }

            var tcs = new TaskCompletionSource<MessageBoxResult>();
            msgbox.Closed += (sender, e) => { tcs.TrySetResult(res); };
            if (parent != null)
            {
                msgbox.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                msgbox.ShowDialog(parent);
            }
            else
            {
                msgbox.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                msgbox.Show();
            }

            return tcs.Task;
        }
    }
}
