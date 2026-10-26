using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using TestDriveAPI.Models;

namespace TestDriveAvalonia.Windows;

public partial class MainWindow : Window
{
    private static readonly HttpClient client = new HttpClient();

    public MainWindow()
    {
        InitializeComponent();

        PhoneNumber.TextChanged += PhoneNumber_TextChanged;
        ShowPassword.IsCheckedChanged += ShowPassword_IsCheckedChanged;
    }

    private void ShowPassword_IsCheckedChanged(object? sender, RoutedEventArgs e)
    {
        if (ShowPassword.IsChecked == true)
        {
            Password.PasswordChar = '\0';
        }
        else
        {
            Password.PasswordChar = '•';
        }
    }

    private void PhoneNumber_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            string raw = textBox.Text ?? "";
            string text = new string(raw.Where(char.IsDigit).ToArray());

            if (string.IsNullOrEmpty(text))
            {
                textBox.TextChanged -= PhoneNumber_TextChanged;
                textBox.Text = "";
                textBox.TextChanged += PhoneNumber_TextChanged;
                return;
            }

            StringBuilder formatted = new StringBuilder();
            bool startsWith8 = raw.StartsWith("8");

            if (startsWith8)
            {
                formatted.Append("8");
                if (text.Length > 1)
                    formatted.Append(" ");
            }
            else
            {
                formatted.Append("+7");
                if (text.Length > (text.StartsWith("7") ? 1 : 0))
                    formatted.Append(" ");
            }

            if (text.Length > 1)
            {
                int startIdx = (startsWith8 || text.StartsWith("7")) ? 1 : 0;
                string digits = text.Substring(startIdx);

                if (digits.Length > 0)
                    formatted.Append("(" + digits.Substring(0, Math.Min(3, digits.Length)));

                if (digits.Length > 3)
                    formatted.Append(") " + digits.Substring(3, Math.Min(3, digits.Length - 3)));

                if (digits.Length > 6)
                    formatted.Append("-" + digits.Substring(6, Math.Min(2, digits.Length - 6)));

                if (digits.Length > 8)
                    formatted.Append("-" + digits.Substring(8, Math.Min(2, digits.Length - 8)));
            }

            textBox.TextChanged -= PhoneNumber_TextChanged;
            textBox.Text = formatted.ToString();
            textBox.CaretIndex = textBox.Text.Length;
            textBox.TextChanged += PhoneNumber_TextChanged;
        }
    }

    private async void LoginButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            string phone = PhoneNumber.Text ?? "";
            string password = Password.Text ?? "";

            if (string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(password))
            {
                await MessageDialogWindow.Show(this, "Заполните все поля", "Ошибка");
                return;
            }

            string url = $"http://localhost:5128/api/User/Authorization?phone={Uri.EscapeDataString(phone)}&password={Uri.EscapeDataString(password)}";
            HttpResponseMessage response = await client.PostAsync(url, null);
            string responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                User? user = JsonConvert.DeserializeObject<User>(responseText);

                if (user != null)
                {
                    BookingWindow.CurrentUserId = user.IdUser;
                    ProfileWindow.CurrentUserId = user.IdUser;
                    MyBookingsWindow.CurrentUserId = user.IdUser;

                    if (user.IdRole == 1)
                    {
                        CatalogWindow.CurrentUserId = user.IdUser;
                        var catalog = new CatalogWindow();
                        catalog.Show();
                    }
                    else if (user.IdRole == 2)
                    {
                        ManagerCarsWindow.CurrentUserId = user.IdUser;
                        var managerWindow = new ManagerCarsWindow();
                        managerWindow.Show();
                    }

                    Close();
                }
            }
            else
            {
                await MessageDialogWindow.Show(this, responseText, "Ошибка входа");
            }
        }
        catch (Exception ex)
        {
            await MessageDialogWindow.Show(this, ex.Message, "Ошибка");
        }
    }

    private void RegisterBT_Click(object? sender, RoutedEventArgs e)
    {
        var regWindow = new RegistrationWindow();
        regWindow.Show();
        Close();
    }

    private async void ForgotPasswordButton_Click(object? sender, RoutedEventArgs e)
    {
        var forgotWindow = new ForgotPasswordWindow();
        await forgotWindow.ShowDialog(this);
    }

    private void GuestButton_Click(object? sender, RoutedEventArgs e)
    {
        CatalogWindow.CurrentUserId = 0;
        BookingWindow.CurrentUserId = 0;
        ProfileWindow.CurrentUserId = 0;
        MyBookingsWindow.CurrentUserId = 0;

        var window = new CatalogWindow();
        window.Show();
        Close();
    }
}
