using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace TestDrive.Windows
{
    public partial class AddCarWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        public AddCarWindow()
        {
            InitializeComponent();
            LoadCategories();
        }

        private async void LoadCategories()
        {
            var response = await client.GetAsync("http://localhost:5128/api/Category");
            string json = await response.Content.ReadAsStringAsync();
            List<dynamic> categories = JsonConvert.DeserializeObject<List<dynamic>>(json);
            CategoryBox.ItemsSource = categories;
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter =
                "Image files|*.jpg;*.jpeg;*.png;*.webp";

            if (dialog.ShowDialog() == true)
            {
                string sourcePath = dialog.FileName;

                string fileName = Path.GetFileName(sourcePath);

                string resourcesFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");

                if (!Directory.Exists(resourcesFolder))
                {
                    Directory.CreateDirectory(resourcesFolder);
                }

                string destinationPath =
                    Path.Combine(resourcesFolder, fileName);

                File.Copy(sourcePath, destinationPath, true);

                ImageBox.Text = fileName;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryBox.SelectedValue == null ||
                string.IsNullOrWhiteSpace(ModelBox.Text) ||
                string.IsNullOrWhiteSpace(YearBox.Text) ||
                string.IsNullOrWhiteSpace(DescriptionBox.Text) ||
                string.IsNullOrWhiteSpace(ImageBox.Text) ||
                string.IsNullOrWhiteSpace(PriceBox.Text))
            {
                MessageBox.Show("Заполните поля");
                return;
            }

            if (!int.TryParse(YearBox.Text, out int year))
            {
                MessageBox.Show("Введите корректный год");
                return;
            }

            if (year < 1900 || year > DateTime.Now.Year)
            {
                MessageBox.Show("Некорректный год автомобиля");
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price))
            {
                MessageBox.Show("Введите корректную цену");
                return;
            }

            var car = new
            {
                idCategory = CategoryBox.SelectedValue,
                model = ModelBox.Text,
                yearCar = year,
                description = DescriptionBox.Text,
                imagePath = ImageBox.Text,
                price = price
            };

            string json = JsonConvert.SerializeObject(car);
            StringContent content = new StringContent(json,Encoding.UTF8,"application/json");
            var response = await client.PostAsync("http://localhost:5128/api/Car",content);
            string result = await response.Content.ReadAsStringAsync();
            MessageBox.Show(result);

            if (response.IsSuccessStatusCode)
            {
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}