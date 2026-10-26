using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using System;
using System.Net.Http;
using TestDriveAvalonia.Windows;

namespace TestDriveAvalonia.UserControls;

public partial class BookingCard : UserControl
{
    private static readonly HttpClient client = new HttpClient();
    private int bookingId;
    public event Action? BookingCancelled;

    public BookingCard()
    {
        InitializeComponent();
    }

    public void SetData(dynamic booking, dynamic car, dynamic category, dynamic status)
    {
        bookingId = (int)booking.idBooking;

        CarText.Text = $"{category.nameCategory} {car.model}";
        DateTime bookingDate = DateTime.Parse(booking.bookingDate.ToString());
        DateText.Text = $"{bookingDate:dd.MM.yyyy} {booking.bookingTime}";
        string statusName = status.statusName.ToString();
        StatusText.Text = statusName;

        switch (statusName.ToLower())
        {
            case "в рассмотрении":
                StatusBorder.Background = Brush.Parse("#F59E0B");
                CancelButton.IsVisible = true;
                break;
            case "подтверждено":
                StatusBorder.Background = Brush.Parse("#10B981");
                CancelButton.IsVisible = false;
                break;
            case "отменено":
                StatusBorder.Background = Brush.Parse("#EF4444");
                CancelButton.IsVisible = false;
                break;
            default:
                StatusBorder.Background = Brush.Parse("#6B7280");
                CancelButton.IsVisible = false;
                break;
        }
    }

    private async void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        var owner = this.FindAncestorOfType<Window>();
        var response = await client.PutAsync($"http://localhost:5128/api/Booking/{bookingId}?statusId=3", null);

        if (response.IsSuccessStatusCode)
        {
            await MessageDialogWindow.Show(owner, "Бронирование отменено", "Статус");
            BookingCancelled?.Invoke();
        }
        else
        {
            string error = await response.Content.ReadAsStringAsync();
            await MessageDialogWindow.Show(owner, error, "Ошибка");
        }
    }
}
