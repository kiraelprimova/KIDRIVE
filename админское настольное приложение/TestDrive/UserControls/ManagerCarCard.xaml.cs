using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TestDrive.Windows;

namespace TestDrive.UserControls
{
    public partial class ManagerCarCard : UserControl
    {
        private static readonly HttpClient client = new HttpClient();
        private int CarId;
        private dynamic CurrentCar;

        public ManagerCarCard()
        {
            InitializeComponent();
        }

        public void SetData(dynamic car, string category)
        {
            CurrentCar = car;
            CarId = car.idCar;

            NameText.Text = $"{category} {car.model}";
            YearText.Text = $"Год: {car.yearCar}";
            DescriptionText.Text = car.description;
            PriceText.Text =$"{car.price} ₽";
            string imagePath = $"pack://application:,,,/Resources/{car.imagePath}";
            CarImage.Source = new BitmapImage(new Uri(imagePath));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditCarWindow window = new EditCarWindow(CurrentCar);
            window.CarUpdated += () =>
            {
                ManagerCarsWindow manager = Window.GetWindow(this) as ManagerCarsWindow;
                manager?.RefreshCars();
            };
            window.ShowDialog();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await client.PutAsync($"http://localhost:5128/api/Car/DeleteCar/{CarId}",null);
            string result = await response.Content.ReadAsStringAsync();

            MessageBox.Show(result);
            ManagerCarsWindow manager = Window.GetWindow(this) as ManagerCarsWindow;
            manager?.RefreshCars();
        }
    }
}