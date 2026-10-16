using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace TestDrive.Windows
{
    public partial class CarDetailsWindow : Window
    {
        private dynamic carData;
        private static readonly HttpClient client = new HttpClient();

        public CarDetailsWindow(dynamic car)
        {
            InitializeComponent();
            carData = car;
            LoadCategory();
        }

        private async void LoadCategory()
        {
            var response = await client.GetAsync($"http://localhost:5128/api/Category/{carData.idCategory}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic category = JsonConvert.DeserializeObject(json);

            BrandText.Text = $"{category.nameCategory} {carData.model}";
            YearText.Text = $"Год выпуска: {carData.yearCar}";
            DescriptionText.Text = carData.description;
            string imagePath = $"pack://application:,,,/Resources/{carData.imagePath}";
            CarImage.Source = new BitmapImage(new Uri(imagePath));
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BookingButton_Click(object sender, RoutedEventArgs e)
        {
            BookingWindow window = new BookingWindow(carData);
            window.ShowDialog();
        }
    }
}