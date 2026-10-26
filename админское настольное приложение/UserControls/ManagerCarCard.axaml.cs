using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using System.Net.Http;
using TestDriveAvalonia.Classes;
using TestDriveAvalonia.Windows;

namespace TestDriveAvalonia.UserControls;

public partial class ManagerCarCard : UserControl
{
    private static readonly HttpClient client = new HttpClient();
    private int CarId;
    private dynamic? CurrentCar;

    public ManagerCarCard()
    {
        InitializeComponent();
    }

    public void SetData(dynamic car, string category)
    {
        CurrentCar = car;
        CarId = (int)car.idCar;

        NameText.Text = $"{category} {car.model}";
        YearText.Text = $"Год: {car.yearCar}";
        DescriptionText.Text = (string)(car.description ?? "");
        PriceText.Text = $"{car.price} ₽";

        CarImage.Source = ImageHelper.LoadBitmap((string?)car.imagePath);
    }

    private async void EditButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CurrentCar == null) return;

        var owner = this.FindAncestorOfType<Window>();
        var window = new EditCarWindow(CurrentCar);
        window.CarUpdated += () =>
        {
            var manager = owner as ManagerCarsWindow;
            manager?.RefreshCars();
        };

        if (owner != null)
            await window.ShowDialog(owner);
        else
            window.Show();
    }

    private async void DeleteButton_Click(object? sender, RoutedEventArgs e)
    {
        var owner = this.FindAncestorOfType<Window>();
        var response = await client.PutAsync($"http://localhost:5128/api/Car/DeleteCar/{CarId}", null);
        string result = await response.Content.ReadAsStringAsync();

        await MessageDialogWindow.Show(owner, result, "Удаление");
        var manager = owner as ManagerCarsWindow;
        manager?.RefreshCars();
    }
}
