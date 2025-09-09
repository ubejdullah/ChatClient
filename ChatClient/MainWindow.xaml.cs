using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Windows.System;

namespace ChatClient
{
    public sealed partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<ChatMessage> Messages { get; set; }
        private DispatcherTimer typingTimer;

        public MainWindow()
        {
            this.InitializeComponent();
            Messages = new ObservableCollection<ChatMessage>();
            InitializeChat();
            SetupTypingAnimation();
        }

        private void InitializeChat()
        {
            // Beispiel-Nachrichten hinzufügen
            Messages.Add(new ChatMessage
            {
                Text = "Hey! Wie geht's dir denn? 😊",
                Timestamp = DateTime.Now.AddMinutes(-28),
                IsFromMe = false,
                SenderName = "Anna Schmidt"
            });

            Messages.Add(new ChatMessage
            {
                Text = "Hallo Anna! Mir geht's super, danke! Und dir?",
                Timestamp = DateTime.Now.AddMinutes(-27),
                IsFromMe = true,
                SenderName = "Du"
            });

            Messages.Add(new ChatMessage
            {
                Text = "Mir auch! Hast du Lust heute Abend ins Kino zu gehen? Der neue Marvel Film läuft 🎬",
                Timestamp = DateTime.Now.AddMinutes(-25),
                IsFromMe = false,
                SenderName = "Anna Schmidt"
            });
        }

        private void SetupTypingAnimation()
        {
            typingTimer = new DispatcherTimer();
            typingTimer.Interval = TimeSpan.FromSeconds(3);
            typingTimer.Tick += (s, e) =>
            {
                TypingIndicator.Visibility = TypingIndicator.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
            };
            typingTimer.Start();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter && !Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift).HasFlag(Windows.UI.Core.CoreVirtualKeyStates.Down))
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private async System.Threading.Tasks.Task SendMessage()
        {
            var messageText = MessageTextBox.Text.Trim();
            if (string.IsNullOrEmpty(messageText))
                return;

            // Nachricht zur Liste hinzufügen
            var newMessage = new ChatMessage
            {
                Text = messageText,
                Timestamp = DateTime.Now,
                IsFromMe = true,
                SenderName = "Du",
                IsDelivered = true
            };

            AddMessageToUI(newMessage);
            MessageTextBox.Text = string.Empty;

            // Nach unten scrollen
            await System.Threading.Tasks.Task.Delay(100);
            MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight);

            // Simulierte Antwort nach kurzer Verzögerung
            await SimulateResponse(messageText);
        }

        private void AddMessageToUI(ChatMessage message)
        {
            var messageGrid = CreateMessageUI(message);
            MessagesPanel.Children.Add(messageGrid);
        }

        private Grid CreateMessageUI(ChatMessage message)
        {
            var grid = new Grid();
            grid.Margin = new Thickness(0, 0, 0, 15);
            grid.MaxWidth = 400;

            if (message.IsFromMe)
            {
                grid.HorizontalAlignment = HorizontalAlignment.Right;
                var border = CreateSentMessageBorder(message);
                grid.Children.Add(border);
            }
            else
            {
                grid.HorizontalAlignment = HorizontalAlignment.Left;
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var profilePic = new PersonPicture
                {
                    Width = 28,
                    Height = 28,
                    DisplayName = message.SenderName,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 10, 0)
                };
                Grid.SetColumn(profilePic, 0);

                var border = CreateReceivedMessageBorder(message);
                Grid.SetColumn(border, 1);

                grid.Children.Add(profilePic);
                grid.Children.Add(border);
            }

            return grid;
        }

        private Border CreateSentMessageBorder(ChatMessage message)
        {
            var border = new Border();
            border.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["AccentFillColorDefaultBrush"];
            border.CornerRadius = new CornerRadius(18, 18, 4, 18);
            border.Padding = new Thickness(15, 10, 15, 10);

            var stackPanel = new StackPanel();

            var messageText = new TextBlock
            {
                Text = message.Text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White)
            };

            var timePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 4, 0, 0),
                Spacing = 5
            };

            var timeText = new TextBlock
            {
                Text = message.Timestamp.ToString("HH:mm"),
                FontSize = 11,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                Opacity = 0.8
            };

            var checkIcon = new SymbolIcon
            {
                Symbol = Symbol.Accept,
                Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.White),
                Opacity = 0.8
            };

            timePanel.Children.Add(timeText);
            if (message.IsDelivered)
                timePanel.Children.Add(checkIcon);

            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(timePanel);
            border.Child = stackPanel;

            return border;
        }

        private Border CreateReceivedMessageBorder(ChatMessage message)
        {
            var border = new Border();
            border.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardBackgroundFillColorDefaultBrush"];
            border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["CardStrokeColorDefaultBrush"];
            border.BorderThickness = new Thickness(1);
            border.CornerRadius = new CornerRadius(18, 18, 18, 4);
            border.Padding = new Thickness(15, 10, 15, 10);

            var stackPanel = new StackPanel();

            var messageText = new TextBlock
            {
                Text = message.Text,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 14
            };

            var timeText = new TextBlock
            {
                Text = message.Timestamp.ToString("HH:mm"),
                FontSize = 11,
                Opacity = 0.6,
                Margin = new Thickness(0, 4, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(timeText);
            border.Child = stackPanel;

            return border;
        }

        private async System.Threading.Tasks.Task SimulateResponse(string userMessage)
        {
            // Typing-Indikator anzeigen
            TypingIndicator.Visibility = Visibility.Visible;
            await System.Threading.Tasks.Task.Delay(2000);

            // Antwort generieren basierend auf der Nachricht
            string response = GenerateResponse(userMessage);

            var responseMessage = new ChatMessage
            {
                Text = response,
                Timestamp = DateTime.Now,
                IsFromMe = false,
                SenderName = "Anna Schmidt"
            };

            AddMessageToUI(responseMessage);

            // Typing-Indikator verstecken
            TypingIndicator.Visibility = Visibility.Collapsed;

            // Nach unten scrollen
            await System.Threading.Tasks.Task.Delay(100);
            MessagesScrollViewer.ScrollToVerticalOffset(MessagesScrollViewer.ScrollableHeight);
        }

        private string GenerateResponse(string userMessage)
        {
            var responses = new[]
            {
                "Das klingt interessant! Erzähl mir mehr davon 😊",
                "Auf jeden Fall! Das machen wir 👍",
                "Hmm, das muss ich mir überlegen...",
                "Perfekt! Ich freue mich schon darauf! ✨",
                "Oh wow, das hätte ich nicht gedacht! 😮",
                "Ja genau, das sehe ich genauso!",
                "Das ist eine super Idee! 💡",
                "Haha, du bist echt witzig! 😄"
            };

            var random = new Random();
            return responses[random.Next(responses.Length)];
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ChatMessage
    {
        public string Text { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsFromMe { get; set; }
        public string SenderName { get; set; }
        public bool IsDelivered { get; set; }
    }
}