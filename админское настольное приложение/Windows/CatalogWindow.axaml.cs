using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TestDriveAvalonia.UserControls;

namespace TestDriveAvalonia.Windows;

public partial class CatalogWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    public static int CurrentUserId;

    private List<dynamic> cars = new List<dynamic>();
    private List<dynamic> categories = new List<dynamic>();

    public CatalogWindow()
    {
        InitializeComponent();

        BrandBox.SelectionChanged += (s, e) => { if (cars.Count > 0) UpdateCars(); };
        YearBox.SelectionChanged += (s, e) => { if (cars.Count > 0) UpdateCars(); };
        SortBox.SelectionChanged += (s, e) => { if (cars.Count > 0) UpdateCars(); };
        SearchBox.TextChanged += (s, e) => UpdateCars();

        LoadData();
        LoadUser();
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

    private async void LoadData()
    {
        await LoadCategories();
        await LoadCars();
    }

    private async Task LoadCategories()
    {
        try
        {
            var response = await client.GetAsync("http://localhost:5128/api/Category");
            string json = await response.Content.ReadAsStringAsync();
            categories = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
        }
        catch { }
    }

    private async Task LoadCars()
    {
        try
        {
            var response = await client.GetAsync("http://localhost:5128/api/Car");
            string json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                cars = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
                LoadFilters();
                UpdateCars();
            }
        }
        catch { }
    }

    private void LoadFilters()
    {
        var brandItems = new List<string> { "Все марки" };
        foreach (var category in categories)
        {
            brandItems.Add(category.nameCategory.ToString());
        }
        BrandBox.ItemsSource = brandItems;

        var yearItems = new List<string> { "Все года" };
        var years = cars.Select(c => (int)c.yearCar)
                        .Distinct()
                        .OrderByDescending(y => y);

        foreach (var year in years)
        {
            yearItems.Add(year.ToString());
        }
        YearBox.ItemsSource = yearItems;

        BrandBox.SelectedIndex = 0;
        YearBox.SelectedIndex = 0;
        SortBox.SelectedIndex = 0;
    }

    private void UpdateCars()
    {
        CarsPanel.Children.Clear();
        IEnumerable<dynamic> filteredCars = cars;

        string search = (SearchBox.Text ?? "").ToLower().Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            filteredCars = filteredCars.Where(c =>
            {
                string brand = GetCategoryName((int)c.idCategory);
                string model = c.model.ToString();
                string fullName = $"{brand} {model}".ToLower();
                return fullName.Contains(search);
            });
        }

        if (BrandBox.SelectedIndex > 0 && BrandBox.SelectedItem != null)
        {
            string selectedBrand = BrandBox.SelectedItem.ToString() ?? "";
            filteredCars = filteredCars.Where(c => GetCategoryName((int)c.idCategory) == selectedBrand);
        }

        if (YearBox.SelectedIndex > 0 && YearBox.SelectedItem != null)
        {
            if (int.TryParse(YearBox.SelectedItem.ToString(), out int selectedYear))
            {
                filteredCars = filteredCars.Where(c => (int)c.yearCar == selectedYear);
            }
        }

        if (SortBox.SelectedIndex == 1)
        {
            filteredCars = filteredCars.OrderBy(c => GetCategoryName((int)c.idCategory));
        }
        else if (SortBox.SelectedIndex == 2)
        {
            filteredCars = filteredCars.OrderByDescending(c => GetCategoryName((int)c.idCategory));
        }

        var list = filteredCars.ToList();
        if (!list.Any())
        {
            EmptyText.IsVisible = true;
            return;
        }
        else
        {
            EmptyText.IsVisible = false;
        }

        foreach (var item in list)
        {
            var card = new CarCard();
            string categoryName = GetCategoryName((int)item.idCategory);
            card.SetData(item, categoryName);
            CarsPanel.Children.Add(card);
        }
    }

    private string GetCategoryName(int id)
    {
        var category = categories.FirstOrDefault(c => (int)c.idCategory == id);
        return category != null ? category.nameCategory.ToString() : "";
    }

    private async Task<bool> CheckGuest(string message)
    {
        if (CurrentUserId != 0)
            return true;

        await MessageDialogWindow.Show(this, message, "Требуется авторизация");
        var window = new MainWindow();
        window.Show();
        Close();
        return false;
    }

    private async void BookingsButton_Click(object? sender, RoutedEventArgs e)
    {
        bool access = await CheckGuest("Чтобы просмотреть бронирования, вам нужно авторизоваться.");
        if (!access) return;

        var window = new MyBookingsWindow();
        window.Show();
        Close();
    }

    private async void ProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        bool access = await CheckGuest("Чтобы просмотреть профиль, вам нужно авторизоваться.");
        if (!access) return;

        var window = new ProfileWindow();
        window.Show();
        Close();
    }

    private void ExitButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new MainWindow();
        window.Show();
        Close();
    }
}
