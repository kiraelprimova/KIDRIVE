using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Net.Http;

namespace TestDriveAvalonia.UserControls;

public partial class ManagerBookingCard : UserControl
{
    private static readonly HttpClient client = new HttpClient();
    private int BookingId;
    public event Action? BookingStatusChanged;

    public ManagerBookingCard()
    {
        InitializeComponent();
    }

    public void SetData(dynamic booking, dynamic category)
    {
        BookingId = (int)booking.idBooking;

        ClientText.Text = $"{booking.idUserNavigation.lastName} {booking.idUserNavigation.firstName}";
        CarText.Text = $"{category.nameCategory} {booking.idCarNavigation.model}";

        DateTime bookingDate = DateTime.Parse(booking.bookingDate.ToString());
        DateText.Text = $"{bookingDate:dd.MM.yyyy} {booking.bookingTime}";

        SetStatus((int)booking.idStatus);
    }

    private void SetStatus(int statusId)
    {
        switch (statusId)
        {
            case 1:
                StatusBorder.IsVisible = false;
                ButtonsPanel.IsVisible = true;
                break;
            case 2:
                StatusBorder.IsVisible = true;
                StatusText.Text = "Подтверждено";
                StatusBorder.Background = Brush.Parse("#10B981");
                ButtonsPanel.IsVisible = false;
                break;
            case 3:
                StatusBorder.IsVisible = true;
                StatusText.Text = "Отменено";
                StatusBorder.Background = Brush.Parse("#EF4444");
                ButtonsPanel.IsVisible = false;
                break;
            case 4:
                StatusBorder.IsVisible = true;
                StatusText.Text = "Завершено";
                StatusBorder.Background = Brush.Parse("#6B7280");
                ButtonsPanel.IsVisible = false;
                break;
        }
    }

    private async void ApproveButton_Click(object? sender, RoutedEventArgs e)
    {
        var response = await client.PutAsync($"http://localhost:5128/api/Booking/{BookingId}?statusId=2", null);
        if (response.IsSuccessStatusCode)
        {
            SetStatus(2);
            BookingStatusChanged?.Invoke();
        }
    }

    private async void RejectButton_Click(object? sender, RoutedEventArgs e)
    {
        var response = await client.PutAsync($"http://localhost:5128/api/Booking/{BookingId}?statusId=3", null);
        if (response.IsSuccessStatusCode)
        {
            SetStatus(3);
            BookingStatusChanged?.Invoke();
        }
    }
}
