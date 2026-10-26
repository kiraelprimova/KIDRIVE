using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using TestDriveAvalonia.UserControls;

namespace TestDriveAvalonia.Windows;

public partial class ManagerBookingsWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    private List<dynamic> AllBookings = new List<dynamic>();
    private List<dynamic> AllCategories = new List<dynamic>();

    public ManagerBookingsWindow()
    {
        InitializeComponent();
        StatusFilterBox.SelectionChanged += StatusFilterBox_SelectionChanged;
        StatusFilterBox.SelectedIndex = 0;
        LoadBookings();
        LoadUser();
    }

    private async void LoadBookings()
    {
        try
        {
            var bookingResponse = await client.GetAsync("http://localhost:5128/api/Booking");
            var categoryResponse = await client.GetAsync("http://localhost:5128/api/Category");

            string bookingJson = await bookingResponse.Content.ReadAsStringAsync();
            string categoryJson = await categoryResponse.Content.ReadAsStringAsync();

            AllBookings = JsonConvert.DeserializeObject<List<dynamic>>(bookingJson) ?? new List<dynamic>();
            AllCategories = JsonConvert.DeserializeObject<List<dynamic>>(categoryJson) ?? new List<dynamic>();

            ApplySortAndRender();
        }
        catch { }
    }

    private void ApplySortAndRender()
    {
        if (AllBookings == null) return;

        IEnumerable<dynamic> filtered = AllBookings;
        string? selected = (StatusFilterBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

        switch (selected)
        {
            case "В рассмотрении":
                filtered = filtered.Where(b => (int)b.idStatus == 1);
                break;
            case "Подтверждено":
                filtered = filtered.Where(b => (int)b.idStatus == 2);
                break;
            case "Отменено":
                filtered = filtered.Where(b => (int)b.idStatus == 3);
                break;
            case "Завершено":
                filtered = filtered.Where(b => (int)b.idStatus == 4);
                break;
        }

        var sorted = filtered
            .OrderBy(b => (int)b.idStatus != 1)
            .ThenByDescending(b => DateTime.Parse(b.bookingDate.ToString()))
            .ThenByDescending(b => TimeSpan.Parse(b.bookingTime.ToString()))
            .ToList();

        RenderBookings(sorted);
    }

    private void RenderBookings(IEnumerable<dynamic> bookings)
    {
        BookingsPanel.Children.Clear();

        foreach (var booking in bookings)
        {
            if (booking.idCarNavigation == null || booking.idUserNavigation == null || booking.idStatusNavigation == null)
                continue;

            var category = AllCategories.FirstOrDefault(c => (int)c.idCategory == (int)booking.idCarNavigation.idCategory);
            if (category == null)
                continue;

            var card = new ManagerBookingCard();
            card.SetData(booking, category);
            card.BookingStatusChanged += () => LoadBookings();
            BookingsPanel.Children.Add(card);
        }
    }

    private void StatusFilterBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        ApplySortAndRender();
    }

    private void CarsButton_Click(object? sender, RoutedEventArgs e)
    {
        new ManagerCarsWindow().Show();
        Close();
    }

    private void ProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        new ManagerProfileWindow().Show();
        Close();
    }

    private void AnalyticsButton_Click(object? sender, RoutedEventArgs e)
    {
        new ManagerAnalyticsWindow().Show();
        Close();
    }

    private async void LoadUser()
    {
        try
        {
            var response = await client.GetAsync($"http://localhost:5128/api/User/{ManagerCarsWindow.CurrentUserId}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic? user = JsonConvert.DeserializeObject(json);

            if (user != null)
            {
                UserText.Text = $"{user.lastName} {user.firstName} {user.patronymic}";
                EmailText.Text = (string)(user.email ?? "");
            }
        }
        catch { }
    }

    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new MainWindow();
        window.Show();
        Close();
    }
}
