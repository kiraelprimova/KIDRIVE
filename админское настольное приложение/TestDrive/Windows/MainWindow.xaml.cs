using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using TestDriveAPI.Models;

namespace TestDrive.Windows
{
    public partial class MainWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private bool isFormatting = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            PasswordText.Text = Password.Password;
            PasswordText.Visibility = Visibility.Visible;
            Password.Visibility = Visibility.Hidden;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            Password.Password = PasswordText.Text;
            PasswordText.Visibility = Visibility.Hidden;
            Password.Visibility = Visibility.Visible;
        }

        private void PhoneNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string text = new string(textBox.Text.Where(char.IsDigit).ToArray());

                if (string.IsNullOrEmpty(text))
                {
                    textBox.TextChanged -= PhoneNumber_TextChanged;
                    textBox.Text = "";
                    textBox.TextChanged += PhoneNumber_TextChanged;
                    return;
                }

                StringBuilder formatted = new StringBuilder();
                bool startsWith8 = textBox.Text.StartsWith("8");

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

        private void Capitalize_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
            {
                textBox.TextChanged -= Capitalize_TextChanged;
                string text = textBox.Text;

                if (text.Length > 0)
                {
                    textBox.Text =
                        char.ToUpper(text[0]) +
                        (text.Length > 1
                        ? text.Substring(1).ToLower()
                        : "");

                    textBox.CaretIndex = textBox.Text.Length;
                }

                textBox.TextChanged += Capitalize_TextChanged;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string phone = PhoneNumber.Text;
                string password;

                if (Password.Visibility == Visibility.Visible)
                {
                    password = Password.Password;
                }
                else
                {
                    password = PasswordText.Text;
                }

                if (string.IsNullOrWhiteSpace(phone) ||
                    string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show(
                        "Заполните все поля",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return;
                }

                string url = $"http://localhost:5128/api/User/Authorization?phone={phone}&password={password}";
                HttpResponseMessage response = await client.PostAsync(url, null);
                string responseText = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    User user = JsonConvert.DeserializeObject<User>(responseText);

                    BookingWindow.CurrentUserId = user.IdUser;
                    ProfileWindow.CurrentUserId = user.IdUser;
                    MyBookingsWindow.CurrentUserId = user.IdUser;

                    if (user.IdRole == 1)
                    {
                        CatalogWindow.CurrentUserId = user.IdUser;
                        CatalogWindow window = new CatalogWindow();
                        window.Show();
                    }
                    else if (user.IdRole == 2)
                    {
                        ManagerCarsWindow.CurrentUserId = user.IdUser;
                        ManagerCarsWindow window = new ManagerCarsWindow();
                        window.Show();
                    }

                    Close();
                }
                else
                {
                    MessageBox.Show(responseText);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void RegisterBT_Click(object sender, RoutedEventArgs e)
        {
            RegistrationWindow window = new RegistrationWindow();
            window.Show();
            Close();
        }

        private void ForgotPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ForgotPasswordWindow window = new ForgotPasswordWindow();
            window.ShowDialog();
        }

        private void GuestButton_Click(object sender, RoutedEventArgs e)
        {
            CatalogWindow.CurrentUserId = 0;
            BookingWindow.CurrentUserId = 0;
            ProfileWindow.CurrentUserId = 0;
            MyBookingsWindow.CurrentUserId = 0;

            CatalogWindow window = new CatalogWindow();
            window.Show();
            Close();
        }
    }
}
