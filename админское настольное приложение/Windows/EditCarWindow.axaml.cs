using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace TestDriveAvalonia.Windows;

public partial class EditCarWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    private int CarId;
    public event Action? CarUpdated;

    public class CategoryItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public override string ToString() => Name;
    }

    public EditCarWindow()
    {
        InitializeComponent();
    }

    public EditCarWindow(dynamic car) : this()
    {
        CarId = (int)car.idCar;
        LoadCategories(car);

        ModelBox.Text = car.model.ToString();
        YearBox.Text = car.yearCar.ToString();
        PriceBox.Text = car.price.ToString();
        DescriptionBox.Text = (string)(car.description ?? "");
        ImageBox.Text = (string)(car.imagePath ?? "");
    }

    private async void LoadCategories(dynamic car)
    {
        try
        {
            var response = await client.GetAsync("http://localhost:5128/api/Category");
            string json = await response.Content.ReadAsStringAsync();
            List<dynamic> categories = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();

            var list = new List<CategoryItem>();
            int targetCatId = (int)car.idCategory;
            CategoryItem? selectedItem = null;

            foreach (var c in categories)
            {
                var item = new CategoryItem { Id = (int)c.idCategory, Name = (string)c.nameCategory };
                list.Add(item);
                if (item.Id == targetCatId)
                    selectedItem = item;
            }

            CategoryBox.ItemsSource = list;
            if (selectedItem != null)
                CategoryBox.SelectedItem = selectedItem;
            else if (list.Count > 0)
                CategoryBox.SelectedIndex = 0;
        }
        catch { }
    }

    private async void SelectImageButton_Click(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Выберите изображение",
            AllowMultiple = false,
            FileTypeFilter = new List<FilePickerFileType>
            {
                new FilePickerFileType("Изображения")
                {
                    Patterns = new[] { "*.jpg", "*.jpeg", "*.png", "*.webp" }
                }
            }
        });

        if (files != null && files.Count > 0)
        {
            var file = files[0];
            string sourcePath = file.Path.LocalPath;
            string fileName = Path.GetFileName(sourcePath);

            string resourcesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
            if (!Directory.Exists(resourcesFolder))
            {
                Directory.CreateDirectory(resourcesFolder);
            }

            string destinationPath = Path.Combine(resourcesFolder, fileName);
            File.Copy(sourcePath, destinationPath, true);

            ImageBox.Text = fileName;
        }
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CategoryBox.SelectedItem == null ||
            string.IsNullOrWhiteSpace(ModelBox.Text) ||
            string.IsNullOrWhiteSpace(YearBox.Text) ||
            string.IsNullOrWhiteSpace(DescriptionBox.Text) ||
            string.IsNullOrWhiteSpace(ImageBox.Text) ||
            string.IsNullOrWhiteSpace(PriceBox.Text))
        {
            await MessageDialogWindow.Show(this, "Заполните все поля", "Ошибка");
            return;
        }

        if (!int.TryParse(YearBox.Text, out int year))
        {
            await MessageDialogWindow.Show(this, "Введите корректный год", "Ошибка");
            return;
        }

        if (year < 1900 || year > DateTime.Now.Year + 1)
        {
            await MessageDialogWindow.Show(this, "Некорректный год автомобиля", "Ошибка");
            return;
        }

        if (!decimal.TryParse(PriceBox.Text, out decimal price))
        {
            await MessageDialogWindow.Show(this, "Введите корректную цену", "Ошибка");
            return;
        }

        try
        {
            var selectedCat = (CategoryItem)CategoryBox.SelectedItem;
            var car = new
            {
                idCategory = selectedCat.Id,
                model = ModelBox.Text.Trim(),
                yearCar = year,
                description = DescriptionBox.Text.Trim(),
                imagePath = ImageBox.Text.Trim(),
                price = price
            };

            string json = JsonConvert.SerializeObject(car);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"http://localhost:5128/api/Car/{CarId}", content);
            string result = await response.Content.ReadAsStringAsync();

            await MessageDialogWindow.Show(this, result, "Результат");
            if (response.IsSuccessStatusCode)
            {
                CarUpdated?.Invoke();
                Close();
            }
        }
        catch (Exception ex)
        {
            await MessageDialogWindow.Show(this, ex.Message, "Ошибка");
        }
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}
