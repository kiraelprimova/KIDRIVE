using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System.Net.Http;
using TestDriveAvalonia.Classes;

namespace TestDriveAvalonia.Windows;

public partial class CarDetailsWindow : Window
{
    private dynamic carData;
    private static readonly HttpClient client = new HttpClient();

    public CarDetailsWindow()
    {
        InitializeComponent();
        carData = new object();
    }

    public CarDetailsWindow(dynamic car) : this()
    {
        carData = car;
        LoadCategory();
    }

    private async void LoadCategory()
    {
        try
        {
            var response = await client.GetAsync($"http://localhost:5128/api/Category/{carData.idCategory}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic? category = JsonConvert.DeserializeObject(json);

            string categoryName = category != null ? category.nameCategory.ToString() : "";
            BrandText.Text = $"{categoryName} {carData.model}";
            YearText.Text = $"Год выпуска: {carData.yearCar}";
            DescriptionText.Text = (string)(carData.description ?? "");

            CarImage.Source = ImageHelper.LoadBitmap((string?)carData.imagePath);
        }
        catch { }
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
