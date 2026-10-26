using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Newtonsoft.Json;
using System.Net.Http;
using TestDriveAvalonia.Classes;
using TestDriveAvalonia.Windows;

namespace TestDriveAvalonia.UserControls;

public partial class CarCard : UserControl
{
    private static readonly HttpClient client = new HttpClient();
    private dynamic? currentCar;
    public int CarId { get; set; }

    public CarCard()
    {
        InitializeComponent();
    }

    public void SetData(dynamic car, string categoryName)
    {
        currentCar = car;
        CarId = (int)car.idCar;

        BrandText.Text = $"{categoryName} {car.model}";
        YearText.Text = $"Год выпуска: {car.yearCar}";
        DescriptionText.Text = (string)(car.description ?? "");
        PriceText.Text = $"{car.price} ₽ за час";

        CarImage.Source = ImageHelper.LoadBitmap((string?)car.imagePath);
    }

    private async void MoreButton_Click(object? sender, RoutedEventArgs e)
    {
        var response = await client.GetAsync($"http://localhost:5128/api/Car/{CarId}");
        string json = await response.Content.ReadAsStringAsync();

        var owner = this.FindAncestorOfType<Window>();
        if (response.IsSuccessStatusCode)
        {
            dynamic? car = JsonConvert.DeserializeObject(json);
            if (car != null)
            {
                var window = new CarDetailsWindow(car);
                if (owner != null)
                    await window.ShowDialog(owner);
                else
                    window.Show();
            }
        }
        else
        {
            await MessageDialogWindow.Show(owner, json, "Информация");
        }
    }

    private async void BronButton_Click(object? sender, RoutedEventArgs e)
    {
        var owner = this.FindAncestorOfType<Window>();
        if (BookingWindow.CurrentUserId == 0)
        {
            await MessageDialogWindow.Show(owner,
                "Чтобы забронировать автомобиль, вам нужно авторизоваться.",
                "Требуется авторизация");

            var window = new MainWindow();
            window.Show();
            owner?.Close();
            return;
        }

        if (currentCar != null)
        {
            var windowBooking = new BookingWindow(currentCar);
            if (owner != null)
                await windowBooking.ShowDialog(owner);
            else
                windowBooking.Show();
        }
    }
}
