using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TestDrive.Windows;
using TestDriveAPI.Classes;

namespace TestDrive.UserControls
{
    public partial class CarCard : UserControl
    {
        private static readonly HttpClient client = new HttpClient();
        private dynamic currentCar;
        public int CarId { get; set; }
        

        public CarCard()
        {
            InitializeComponent();
        }

        public void SetData(dynamic car, string categoryName)
        {
            currentCar = car;
            CarId = car.idCar;

            BrandText.Text = $"{categoryName} {car.model}";
            YearText.Text = $"Год выпуска: {car.yearCar}";
            DescriptionText.Text = car.description;
            PriceText.Text = $"{car.price} ₽ за час";

            string imagePath = $"pack://application:,,,/Resources/{car.imagePath}";

            CarImage.Source = new BitmapImage(new Uri(imagePath));
        }

        private async void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await client.GetAsync($"http://localhost:5128/api/Car/{CarId}");
            string json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                dynamic car = JsonConvert.DeserializeObject(json);
                CarDetailsWindow window = new CarDetailsWindow(car);
                window.ShowDialog();
            }
            else
            {
                MessageBox.Show(json);
            }
        }

        private void BronButton_Click(object sender, RoutedEventArgs e)
        {
            if (BookingWindow.CurrentUserId == 0)
            {
                MessageBoxResult result = MessageBox.Show(
                        "Чтобы забронировать автомобиль вам нужно авторизоваться.\n\nХотите войти в систему?",
                        "Требуется авторизация",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    MainWindow window = new MainWindow();
                    window.Show();
                    Window.GetWindow(this)?.Close();
                }
                return;
            }

            BookingWindow windowBooking = new BookingWindow(currentCar);
            windowBooking.ShowDialog();
        }
    }
}