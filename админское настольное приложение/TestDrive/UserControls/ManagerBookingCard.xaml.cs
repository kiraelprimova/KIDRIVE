using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestDrive.UserControls
{
    public partial class ManagerBookingCard : UserControl
    {
        private static readonly HttpClient client = new HttpClient();
        private int BookingId;

        public event Action BookingStatusChanged;

        public ManagerBookingCard()
        {
            InitializeComponent();
        }

        public void SetData(dynamic booking, dynamic category)
        {
            BookingId = booking.idBooking;

            ClientText.Text =
                $"{booking.idUserNavigation.lastName} {booking.idUserNavigation.firstName}";

            CarText.Text =
                $"{category.nameCategory} {booking.idCarNavigation.model}";

            DateTime bookingDate = DateTime.Parse(booking.bookingDate.ToString());

            DateText.Text =
                $"{bookingDate:dd.MM.yyyy} {booking.bookingTime}";

            SetStatus((int)booking.idStatus);
        }

        private void SetStatus(int statusId)
        {
            switch (statusId)
            {
                case 1:
                    StatusBorder.Visibility = Visibility.Collapsed;
                    ButtonsPanel.Visibility = Visibility.Visible;
                    break;

                case 2:
                    StatusBorder.Visibility = Visibility.Visible;
                    StatusText.Text = "Подтверждено";
                    StatusBorder.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#10B981"));

                    ButtonsPanel.Visibility = Visibility.Collapsed;
                    break;

                case 3:
                    StatusBorder.Visibility = Visibility.Visible;
                    StatusText.Text = "Отменено";
                    StatusBorder.Background = new SolidColorBrush(
                        (Color)ColorConverter.ConvertFromString("#EF4444"));

                    ButtonsPanel.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await client.PutAsync(
                $"http://localhost:5128/api/Booking/{BookingId}?statusId=2",
                null);

            if (response.IsSuccessStatusCode)
            {
                SetStatus(2);
                BookingStatusChanged?.Invoke();
            }
        }

        private async void RejectButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await client.PutAsync(
                $"http://localhost:5128/api/Booking/{BookingId}?statusId=3",
                null);

            if (response.IsSuccessStatusCode)
            {
                SetStatus(3);
                BookingStatusChanged?.Invoke();
            }
        }
    }
}