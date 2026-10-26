using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TestDriveAvalonia.UserControls;

namespace TestDriveAvalonia.Windows;

public partial class ManagerCarsWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    public static int CurrentUserId;

    private List<dynamic> cars = new List<dynamic>();
    private List<dynamic> categories = new List<dynamic>();

    public ManagerCarsWindow()
    {
        InitializeComponent();

        BrandBox.SelectionChanged += (s, e) => { if (cars.Count > 0) UpdateCars(); };
        YearBox.SelectionChanged += (s, e) => { if (cars.Count > 0) UpdateCars(); };
        SortBox.SelectionChanged += (s, e) => { if (cars.Count > 0) UpdateCars(); };
        SearchBox.TextChanged += (s, e) => UpdateCars();

        LoadData();
        LoadUser();
    }

    public async void RefreshCars()
    {
        await LoadCars();
    }

    private async void LoadUser()
    {
        try
        {
            var response = await client.GetAsync($"http://localhost:5128/api/User/{CurrentUserId}");
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
            cars = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();

            LoadFilters();
            UpdateCars();
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
            var card = new ManagerCarCard();
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

    private async void AddButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new AddCarWindow();
        window.CarAdded += () => RefreshCars();
        await window.ShowDialog(this);
    }

    private void BookingsButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ManagerBookingsWindow();
        window.Show();
        Close();
    }

    private void AnalyticsButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ManagerAnalyticsWindow();
        window.Show();
        Close();
    }

    private void ProfileButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ManagerProfileWindow();
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
