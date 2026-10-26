using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace TestDriveAvalonia.Windows;

public partial class ManagerAnalyticsWindow : Window
{
    private static readonly HttpClient client = new HttpClient();

    public ManagerAnalyticsWindow()
    {
        InitializeComponent();

        LoadDashboard();
        LoadPopularCars();
        LoadStatuses();
        LoadBookingsByDate();
        LoadBrands();
        LoadUser();
    }

    private async void LoadDashboard()
    {
        try
        {
            string json = await client.GetStringAsync("http://localhost:5128/api/Analytics/Dashboard");
            dynamic? data = JsonConvert.DeserializeObject(json);

            if (data != null)
            {
                CarsCountText.Text = data.totalCars?.ToString() ?? "0";
                BookingsCountText.Text = data.totalBookings?.ToString() ?? "0";
                ClientsCountText.Text = data.totalClients?.ToString() ?? "0";
                ConfirmedText.Text = data.confirmedBookings?.ToString() ?? "0";
                RevenueText.Text = (data.totalRevenue?.ToString() ?? "0") + " ₽";
            }
        }
        catch { }
    }

    private async void LoadPopularCars()
    {
        try
        {
            string json = await client.GetStringAsync("http://localhost:5128/api/Analytics/PopularCars");
            var data = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();

            CarsListPanel.Children.Clear();
            int max = data.Any() ? data.Max(x => (int)x.total) : 1;
            if (max == 0) max = 1;

            foreach (var item in data)
            {
                int val = (int)item.total;
                string name = (string)item.name;

                var row = new Grid
                {
                    Margin = new Avalonia.Thickness(0, 4, 0, 8),
                    ColumnDefinitions = new ColumnDefinitions("140,*,50")
                };

                row.Children.Add(new TextBlock
                {
                    Text = name,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brush.Parse("#374151")
                });

                var bar = new ProgressBar
                {
                    Value = val,
                    Maximum = max,
                    Height = 14,
                    CornerRadius = new Avalonia.CornerRadius(6),
                    Foreground = Brush.Parse("#1E3A8A"),
                    Background = Brush.Parse("#E5E7EB")
                };
                Grid.SetColumn(bar, 1);
                row.Children.Add(bar);

                var txt = new TextBlock
                {
                    Text = val.ToString(),
                    FontWeight = FontWeight.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brush.Parse("#111827")
                };
                Grid.SetColumn(txt, 2);
                row.Children.Add(txt);

                CarsListPanel.Children.Add(row);
            }
        }
        catch { }
    }

    private async void LoadStatuses()
    {
        try
        {
            string json = await client.GetStringAsync("http://localhost:5128/api/Analytics/BookingStatuses");
            var data = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();

            StatusListPanel.Children.Clear();
            foreach (var item in data)
            {
                string status = item.status.ToString();
                int total = (int)item.total;

                var border = new Border
                {
                    Background = Brush.Parse("#F9FAFB"),
                    BorderBrush = Brush.Parse("#E5E7EB"),
                    BorderThickness = new Avalonia.Thickness(1),
                    CornerRadius = new Avalonia.CornerRadius(10),
                    Padding = new Avalonia.Thickness(14, 10, 14, 10),
                    Margin = new Avalonia.Thickness(0, 0, 0, 8)
                };

                var grid = new Grid
                {
                    ColumnDefinitions = new ColumnDefinitions("*,Auto")
                };

                grid.Children.Add(new TextBlock
                {
                    Text = status,
                    FontWeight = FontWeight.SemiBold,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brush.Parse("#1F2937")
                });

                var badge = new TextBlock
                {
                    Text = total.ToString(),
                    FontWeight = FontWeight.Bold,
                    FontSize = 16,
                    Foreground = Brush.Parse("#1E3A8A"),
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(badge, 1);
                grid.Children.Add(badge);

                border.Child = grid;
                StatusListPanel.Children.Add(border);
            }
        }
        catch { }
    }

    private async void LoadBookingsByDate()
    {
        try
        {
            string json = await client.GetStringAsync("http://localhost:5128/api/Analytics/BookingsByDate");
            var data = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();

            DatesListPanel.Children.Clear();
            foreach (var item in data.Take(7))
            {
                string dateStr = DateTime.Parse(item.date.ToString()).ToString("dd.MM.yyyy");
                int total = (int)item.total;

                var row = new Grid
                {
                    Margin = new Avalonia.Thickness(0, 6, 0, 6),
                    ColumnDefinitions = new ColumnDefinitions("*,Auto")
                };

                row.Children.Add(new TextBlock
                {
                    Text = dateStr,
                    Foreground = Brush.Parse("#4B5563")
                });

                var countTxt = new TextBlock
                {
                    Text = $"{total} шт.",
                    FontWeight = FontWeight.SemiBold,
                    Foreground = Brush.Parse("#111827")
                };
                Grid.SetColumn(countTxt, 1);
                row.Children.Add(countTxt);

                DatesListPanel.Children.Add(row);
            }
        }
        catch { }
    }

    private async void LoadBrands()
    {
        try
        {
            string json = await client.GetStringAsync("http://localhost:5128/api/Analytics/PopularBrands");
            var data = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();

            BrandsListPanel.Children.Clear();
            int max = data.Any() ? data.Max(x => (int)x.total) : 1;
            if (max == 0) max = 1;

            foreach (var item in data)
            {
                int val = (int)item.total;
                string brand = (string)item.brand;

                var row = new Grid
                {
                    Margin = new Avalonia.Thickness(0, 4, 0, 8),
                    ColumnDefinitions = new ColumnDefinitions("120,*,50")
                };

                row.Children.Add(new TextBlock
                {
                    Text = brand,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brush.Parse("#374151")
                });

                var bar = new ProgressBar
                {
                    Value = val,
                    Maximum = max,
                    Height = 14,
                    CornerRadius = new Avalonia.CornerRadius(6),
                    Foreground = Brush.Parse("#2563EB"),
                    Background = Brush.Parse("#E5E7EB")
                };
                Grid.SetColumn(bar, 1);
                row.Children.Add(bar);

                var txt = new TextBlock
                {
                    Text = val.ToString(),
                    FontWeight = FontWeight.Bold,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = Brush.Parse("#111827")
                };
                Grid.SetColumn(txt, 2);
                row.Children.Add(txt);

                BrandsListPanel.Children.Add(row);
            }
        }
        catch { }
    }

    private void CarsButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ManagerCarsWindow();
        window.Show();
        Close();
    }

    private void BookingsButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ManagerBookingsWindow();
        window.Show();
        Close();
    }

    private void ProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ManagerProfileWindow();
        window.Show();
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
