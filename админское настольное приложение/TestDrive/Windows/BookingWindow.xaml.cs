using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TestDrive.Windows
{
    public partial class BookingWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private dynamic carData;
        public static int CurrentUserId;

        public class TimeItem
        {
            public string TimeString { get; set; }
            public TimeSpan TimeValue { get; set; }
            public bool IsAvailable { get; set; }
        }

        public BookingWindow(dynamic car)
        {
            InitializeComponent();
            carData = car;
            LoadCategory();
            BookingDate.SelectedDateChanged += BookingDate_SelectedDateChanged;
        }

        private async void LoadCategory()
        {
            var response = await client.GetAsync($"http://localhost:5128/api/Category/{carData.idCategory}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic category = JsonConvert.DeserializeObject(json);

            CarText.Text = $"{category.nameCategory} {carData.model}";
            string imagePath = $"pack://application:,,,/Resources/{carData.imagePath}";
            CarImage.Source = new BitmapImage(new Uri(imagePath));
            PriceText.Text = $"{carData.price} ₽";
        }

        private async void BookingDate_SelectedDateChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (BookingDate.SelectedDate == null) return;

            string formattedDate = BookingDate.SelectedDate.Value.ToString("yyyy-MM-dd");
            int carId = (int)carData.idCar;

            var response = await client.GetAsync($"http://localhost:5128/api/Car/GetBusyTimes?carId={carId}&date={formattedDate}");

            if (!response.IsSuccessStatusCode) return;

            string json = await response.Content.ReadAsStringAsync();
            var busyTimes = JsonConvert.DeserializeObject<List<TimeSpan>>(json);

            var allTimes = new List<TimeSpan>
    {
        new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0),
        new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0),
        new TimeSpan(15, 0, 0), new TimeSpan(16, 0, 0), new TimeSpan(17, 0, 0)
    };

            TimeBox.Items.Clear();

            foreach (var time in allTimes)
            {
                bool isBusy = busyTimes.Any(bt => bt == time);

                TimeBox.Items.Add(new TimeItem
                {
                    TimeString = $"{time.Hours}:00",
                    TimeValue = time,
                    IsAvailable = !isBusy
                });
            }

            TimeBox.SelectedIndex = -1;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BookingButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookingDate.SelectedDate == null || TimeBox.SelectedItem == null)
            {
                MessageBox.Show("Заполните поля");
                return;
            }

            var selectedTimeItem = (TimeItem)TimeBox.SelectedItem;
            if (!selectedTimeItem.IsAvailable)
            {
                MessageBox.Show("Это время недоступно для бронирования");
                return;
            }

            DateTime selectedDate = BookingDate.SelectedDate.Value.Date;
            DateTime today = DateTime.Today;

            if (selectedDate < today)
            {
                MessageBox.Show("Нельзя выбрать прошедшую дату");
                return;
            }

            if (selectedDate > today.AddMonths(1))
            {
                MessageBox.Show("Бронирование доступно только в пределах месяца");
                return;
            }

            if (AgreementCheck.IsChecked != true)
            {
                MessageBox.Show("Для оформления бронирования необходимо согласиться с условиями");
                return;
            }

            int payment = 0;
            if (CashCheck.IsChecked == true) payment = 1;
            if (CardCheck.IsChecked == true) payment = 2;

            if (payment == 0)
            {
                MessageBox.Show("Выберите способ оплаты");
                return;
            }

            var booking = new
            {
                idUser = CurrentUserId,
                idCar = (int)carData.idCar,
                bookingDate = BookingDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                bookingTime = selectedTimeItem.TimeValue,
                idStatus = 1,
                idPayment = payment
            };

            string json = JsonConvert.SerializeObject(booking);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5128/api/Booking", content);
            string result = await response.Content.ReadAsStringAsync();

            MessageBox.Show(result);

            if (response.IsSuccessStatusCode)
            {
                Close();
            }
        }
    }
}