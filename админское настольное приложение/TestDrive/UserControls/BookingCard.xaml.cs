using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TestDrive.UserControls
{
    public partial class BookingCard : UserControl
    {
        private static readonly HttpClient client = new HttpClient();

        private int bookingId;

        public event Action BookingCancelled;

        public BookingCard()
        {
            InitializeComponent();
        }

        public void SetData(
            dynamic booking,
            dynamic car,
            dynamic category,
            dynamic status)
        {
            bookingId = booking.idBooking;

            CarText.Text = $"{category.nameCategory} {car.model}";
            DateTime bookingDate = DateTime.Parse(booking.bookingDate.ToString());
            DateText.Text = $"{bookingDate:dd.MM.yyyy} {booking.bookingTime}";
            string statusName = status.statusName.ToString();
            StatusText.Text = statusName;

            switch (statusName.ToLower())
            {
                case "в рассмотрении":

                    StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F59E0B"));
                    CancelButton.Visibility = Visibility.Visible;
                    break;

                case "подтверждено":

                    StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"));
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;

                case "отменено":

                    StatusBorder.Background =
                        new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"));
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;

                default:

                    StatusBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6B7280"));
                    CancelButton.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private async void CancelButton_Click(object sender,RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(
                    "Вы уверены, что хотите отменить бронирование?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            var response = await client.PutAsync($"http://localhost:5128/api/Booking/{bookingId}?statusId=3",null);

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show( "Бронирование отменено");
                BookingCancelled?.Invoke();
            }
            else
            {
                string error = await response.Content.ReadAsStringAsync();
                MessageBox.Show(error);
            }
        }
    }
}