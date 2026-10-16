using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TestDrive.UserControls;

namespace TestDrive.Windows
{
    public partial class CatalogWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        public static int CurrentUserId;

        private List<dynamic> cars;
        private List<dynamic> categories;

        public CatalogWindow()
        {
            InitializeComponent();
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

            var response = await client.GetAsync($"http://localhost:5128/api/User/{CurrentUserId}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic user = JsonConvert.DeserializeObject(json);

            UserNameText.Text = $"{user.lastName} {user.firstName} {user.patronymic}";
            UserEmailText.Text = user.email;
        }

        private async void LoadData()
        {
            await LoadCategories();
            await LoadCars();
        }

        private async Task LoadCategories()
        {
            var response = await client.GetAsync("http://localhost:5128/api/Category");
            string json = await response.Content.ReadAsStringAsync();
            categories = JsonConvert.DeserializeObject<List<dynamic>>(json);
        }

        private async Task LoadCars()
        {
            var response = await client.GetAsync("http://localhost:5128/api/Car");
            string json = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                cars = JsonConvert.DeserializeObject<List<dynamic>>(json);
                LoadFilters();
                UpdateCars();
            }
        }

        private void LoadFilters()
        {
            BrandBox.Items.Clear();
            YearBox.Items.Clear();

            BrandBox.Items.Add("Все марки");
            YearBox.Items.Add("Все года");

            foreach (var category in categories)
            {
                BrandBox.Items.Add(category.nameCategory.ToString());
            }

            var years = cars.Select(c => (int)c.yearCar)
                    .Distinct()
                    .OrderByDescending(y => y);

            foreach (var year in years)
            {
                YearBox.Items.Add(year.ToString());
            }

            BrandBox.SelectedIndex = 0;
            YearBox.SelectedIndex = 0;
            SortBox.SelectedIndex = 0;
        }



        private void UpdateCars()
        {
            CarsPanel.Children.Clear();
            IEnumerable<dynamic> filteredCars = cars;
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                string search = SearchBox.Text.ToLower();
                filteredCars = filteredCars.Where(c =>
            {
            string brand = GetCategoryName((int)c.idCategory);
            string model = c.model.ToString();
            string fullName = $"{brand} {model}".ToLower();
            return fullName.Contains(search);
            });
            }
            if (BrandBox.SelectedIndex > 0)
            {
                string selectedBrand = BrandBox.SelectedItem.ToString();
                filteredCars = filteredCars.Where(c => GetCategoryName((int)c.idCategory) == selectedBrand);
            }

            if (YearBox.SelectedIndex > 0)
            {
                int selectedYear = int.Parse(YearBox.SelectedItem.ToString());
                filteredCars = filteredCars.Where(c => (int)c.yearCar == selectedYear);
            }

            if (SortBox.SelectedIndex == 1)
            {
                filteredCars = filteredCars.OrderBy(c => GetCategoryName((int)c.idCategory));
            }
            else if (SortBox.SelectedIndex == 2)
            {
                filteredCars = filteredCars.OrderByDescending(c => GetCategoryName((int)c.idCategory));
            }

            if (!filteredCars.Any())
            {
                EmptyText.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                EmptyText.Visibility = Visibility.Collapsed;
            }

            foreach (var item in filteredCars)
            {
                CarCard card = new CarCard();
                string categoryName = GetCategoryName((int)item.idCategory);
                card.SetData(item, categoryName);
                CarsPanel.Children.Add(card);
            }
        }

        private void BrandBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cars != null)
                UpdateCars();
        }

        private void YearBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cars != null)
                UpdateCars();
        }

        private string GetCategoryName(int id)
        {
            var category = categories.FirstOrDefault(c => c.idCategory == id);

            if (category == null)
                return "";

            return category.nameCategory.ToString();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateCars();
        }

        private void SortBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCars();
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool CheckGuest(string message)
        {
            if (CurrentUserId != 0)
                return true;

            MessageBoxResult result = MessageBox.Show(
                    message,
                    "Требуется авторизация",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                MainWindow window = new MainWindow();
                window.Show();
                Close();
            }
            return false;
        }

        private void BookingsButton_Click(object sender, RoutedEventArgs e)
        {
            bool access = CheckGuest("Чтобы просмотреть бронирования вам нужно авторизоваться.\n\nХотите войти в систему?");

            if (!access)
                return;

            MyBookingsWindow window = new MyBookingsWindow();
            window.Show();
            Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            bool access = CheckGuest("Чтобы просмотреть профиль вам нужно авторизоваться.\n\nХотите войти в систему?");

            if (!access)
                return;

            ProfileWindow window = new ProfileWindow();
            window.Show();
            Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            Close();
        }
    }
}