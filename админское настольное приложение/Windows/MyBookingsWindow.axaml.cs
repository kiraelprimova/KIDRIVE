using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using TestDriveAvalonia.UserControls;

namespace TestDriveAvalonia.Windows;

public partial class MyBookingsWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    public static int CurrentUserId;

    private List<dynamic> AllBookings = new List<dynamic>();
    private List<dynamic> AllCategories = new List<dynamic>();

    public MyBookingsWindow()
    {
        InitializeComponent();
        StatusFilterBox.SelectionChanged += StatusFilterBox_SelectionChanged;
        StatusFilterBox.SelectedIndex = 0;
        LoadBookings();
        LoadUser();
    }

    public void RefreshBookings()
    {
        LoadBookings();
    }

    private async void LoadBookings()
    {
        BookingsPanel.Children.Clear();

        try
        {
            var bookingResponse = await client.GetAsync("http://localhost:5128/api/Booking");
            var categoryResponse = await client.GetAsync("http://localhost:5128/api/Category");
            string bookingJson = await bookingResponse.Content.ReadAsStringAsync();
            string categoryJson = await categoryResponse.Content.ReadAsStringAsync();

            List<dynamic> bookings = JsonConvert.DeserializeObject<List<dynamic>>(bookingJson) ?? new List<dynamic>();
            AllCategories = JsonConvert.DeserializeObject<List<dynamic>>(categoryJson) ?? new List<dynamic>();

            AllBookings = bookings
                .Where(b => (int)b.idUser == CurrentUserId)
                .OrderByDescending(b => DateTime.Parse(b.bookingDate.ToString()))
                .ThenByDescending(b => TimeSpan.Parse(b.bookingTime.ToString()))
                .ToList();

            RenderBookings(AllBookings);
        }
        catch { }
    }

    private void RenderBookings(IEnumerable<dynamic> bookings)
    {
        BookingsPanel.Children.Clear();
        var list = bookings.ToList();

        if (!list.Any())
        {
            EmptyBookingsText.IsVisible = true;
            return;
        }

        EmptyBookingsText.IsVisible = false;

        foreach (var booking in list)
        {
            if (booking.idCarNavigation == null || booking.idStatusNavigation == null)
                continue;

            var category = AllCategories.FirstOrDefault(c => (int)c.idCategory == (int)booking.idCarNavigation.idCategory);
            if (category == null)
                continue;

            var card = new BookingCard();
            card.SetData(booking, booking.idCarNavigation, category, booking.idStatusNavigation);
            card.BookingCancelled += () => RefreshBookings();
            BookingsPanel.Children.Add(card);
        }
    }

    private void StatusFilterBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (AllBookings == null || StatusFilterBox.SelectedItem == null)
            return;

        string? selected = (StatusFilterBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        IEnumerable<dynamic> filtered = AllBookings;

        switch (selected)
        {
            case "В рассмотрении":
                filtered = AllBookings.Where(b => (int)b.idStatus == 1);
                break;
            case "Подтверждено":
                filtered = AllBookings.Where(b => (int)b.idStatus == 2);
                break;
            case "Отменено":
                filtered = AllBookings.Where(b => (int)b.idStatus == 3);
                break;
            case "Завершено":
                filtered = AllBookings.Where(b => (int)b.idStatus == 4);
                break;
        }

        RenderBookings(filtered);
    }

    private void CatalogButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new CatalogWindow();
        window.Show();
        Close();
    }

    private void ProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ProfileWindow();
        window.Show();
        Close();
    }

    private async void LoadUser()
    {
        if (CurrentUserId == 0)
        {
            UserNameText.Text = "Гость";
            UserEmailText.Text = "Не авторизован";
            return;
        }

        try
        {
            var response = await client.GetAsync($"http://localhost:5128/api/User/{CurrentUserId}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic? user = JsonConvert.DeserializeObject(json);

            if (user != null)
            {
                UserNameText.Text = $"{user.lastName} {user.firstName} {user.patronymic}";
                UserEmailText.Text = (string)(user.email ?? "");
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
