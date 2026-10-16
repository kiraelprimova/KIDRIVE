using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using TestDriveAPI.Classes;

namespace TestDrive.Windows
{
    public partial class RegistrationWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private bool isFormatting = false;

        public RegistrationWindow()
        {
            InitializeComponent();
            BirthDatePicker.DisplayDateEnd = DateTime.Today;
            BirthDatePicker.DisplayDateStart = DateTime.Today.AddYears(-100);
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

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (Password.Visibility == Visibility.Visible)
                PasswordText.Text = Password.Password;
        }

        private void PasswordText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordText.Visibility == Visibility.Visible)
                Password.Password = PasswordText.Text;
        }

        private void ShowConfirmPassword_Checked(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordText.Text = ConfirmPassword.Password;
            ConfirmPasswordText.Visibility = Visibility.Visible;
            ConfirmPassword.Visibility = Visibility.Hidden;
        }

        private void ShowConfirmPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfirmPassword.Password = ConfirmPasswordText.Text;
            ConfirmPasswordText.Visibility = Visibility.Hidden;
            ConfirmPassword.Visibility = Visibility.Visible;
        }

        private void ConfirmPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ConfirmPassword.Visibility == Visibility.Visible)
                ConfirmPasswordText.Text = ConfirmPassword.Password;
        }

        private void ConfirmPasswordText_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ConfirmPasswordText.Visibility == Visibility.Visible)
                ConfirmPassword.Password = ConfirmPasswordText.Text;
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
                    int startIdx =
                        (startsWith8 || text.StartsWith("7")) ? 1 : 0;

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
            if (sender is TextBox textBox)
            {
                textBox.TextChanged -= Capitalize_TextChanged;
                string text = textBox.Text;
                text = Regex.Replace(text, @"[^А-Яа-яЁё\-]", "");
                text = Regex.Replace(text, @"\-{2,}", "-");
                text = Regex.Replace(text, @"^\-+", "");
                text = Regex.Replace(text, @"\-+$", "");
                if (!string.IsNullOrWhiteSpace(text))
                {
                    string[] parts = text.Split('-');

                    for (int i = 0; i < parts.Length; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(parts[i]))
                        {
                            parts[i] =
                                char.ToUpper(parts[i][0]) + (parts[i].Length > 1 ? parts[i].Substring(1).ToLower() : "");
                        }
                    }

                    text = string.Join("-", parts);
                }

                textBox.Text = text;
                textBox.CaretIndex = textBox.Text.Length;
                textBox.TextChanged += Capitalize_TextChanged;
            }
        }

        private bool ValidEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void Email_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = Email.Text;
            string filtered = Regex.Replace(text, @"[А-Яа-яЁё]", "");
            if (text != filtered)
            {
                int caret = Email.CaretIndex - (text.Length - filtered.Length);
                Email.Text = filtered;
                Email.CaretIndex = Math.Max(caret, 0);
            }
        }

        private bool IsAdult(DateOnly birthDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - birthDate.Year;
            if (birthDate > today.AddYears(-age))
                age--;
            return age >= 18;
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(FirstName.Text))
            {
                MessageBox.Show(
                    "Введите имя",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                FirstName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(LastName.Text))
            {
                MessageBox.Show(
                    "Введите фамилию",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                LastName.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(PhoneNumber.Text))
            {
                MessageBox.Show(
                    "Введите номер телефона",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PhoneNumber.Focus();
                return;
            }

            string phoneDigits = new string(
                PhoneNumber.Text.Where(char.IsDigit).ToArray());

            if (phoneDigits.Length != 11)
            {
                MessageBox.Show(
                    "Введите полный номер телефона",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PhoneNumber.Focus();
                return;
            }

            if (!phoneDigits.StartsWith("7") &&
                !phoneDigits.StartsWith("8"))
            {
                MessageBox.Show(
                    "Номер должен начинаться с +7 или 8",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PhoneNumber.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Email.Text))
            {
                MessageBox.Show(
                    "Введите электронную почту",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Email.Focus();
                return;
            }

            if (Regex.IsMatch(Email.Text, @"[А-Яа-яЁё]"))
            {
                MessageBox.Show(
                    "Email не должен содержать русские буквы",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Email.Focus();
                return;
            }

            if (!ValidEmail(Email.Text))
            {
                MessageBox.Show(
                    "Введите корректный email",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Email.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(Password.Password))
            {
                MessageBox.Show(
                    "Введите пароль",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Password.Focus();
                return;
            }

            if (Password.Password.Length < 8)
            {
                MessageBox.Show(
                    "Пароль должен содержать минимум 8 символов",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Password.Focus();
                return;
            }

            bool hasDigit = Regex.IsMatch(Password.Password, @"\d");
            bool hasUpper = Regex.IsMatch(Password.Password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(Password.Password, @"[a-z]");
            bool hasSpecial = Regex.IsMatch(
                Password.Password,
                @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

            if (!hasDigit || !hasUpper || !hasLower || !hasSpecial)
            {
                MessageBox.Show(
                    "Пароль должен содержать:\n" +
                    "- минимум 8 символов\n" +
                    "- хотя бы одну цифру\n" +
                    "- хотя бы одну заглавную букву\n" +
                    "- хотя бы одну строчную букву\n" +
                    "- хотя бы один спецсимвол",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                Password.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPassword.Password))
            {
                MessageBox.Show(
                    "Подтвердите пароль",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ConfirmPassword.Focus();
                return;
            }

            if (Password.Password != ConfirmPassword.Password)
            {
                MessageBox.Show(
                    "Пароли не совпадают",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ConfirmPassword.Focus();
                return;
            }

            if (!BirthDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show(
                    "Выберите дату рождения",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            DateOnly birth = DateOnly.FromDateTime(BirthDatePicker.SelectedDate.Value);

            if (!IsAdult(birth))
            {
                MessageBox.Show(
                    "Регистрация доступна только с 18 лет",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                var dto = new UserDTO
                {
                    FirstName = FirstName.Text.Trim(),
                    LastName = LastName.Text.Trim(),
                    Patronymic = Patronymic.Text.Trim(),
                    Phone = PhoneNumber.Text,
                    Email = "fatkhlislamovad@mail.ru",
                    Password = Password.Password,
                    BirthDate = birth
                };

                string url = "http://localhost:5128/api/User/Registration";
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(url, content);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show(
                        "Поздравляем, вы успешно зарегистрировались!",
                        "Успех",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    MainWindow w = new MainWindow();
                    w.Show();

                    Close();
                }
                else
                {
                    MessageBox.Show(
                        result,
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка подключения к серверу:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AuthBT_Click(object sender, RoutedEventArgs e)
        {
            MainWindow w = new MainWindow();
            w.Show();
            Close();
        }
    }
}
