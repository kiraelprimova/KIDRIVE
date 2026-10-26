using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using TestDriveAPI.Classes;

namespace TestDriveAvalonia.Windows;

public partial class RegistrationWindow : Window
{
    private static readonly HttpClient client = new HttpClient();

    public RegistrationWindow()
    {
        InitializeComponent();

        PhoneNumber.TextChanged += PhoneNumber_TextChanged;
        FirstName.TextChanged += Capitalize_TextChanged;
        LastName.TextChanged += Capitalize_TextChanged;
        Patronymic.TextChanged += Capitalize_TextChanged;

        ShowPassword.IsCheckedChanged += (s, e) =>
        {
            Password.PasswordChar = ShowPassword.IsChecked == true ? '\0' : '•';
        };

        ShowConfirmPassword.IsCheckedChanged += (s, e) =>
        {
            ConfirmPassword.PasswordChar = ShowConfirmPassword.IsChecked == true ? '\0' : '•';
        };
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

    private void Capitalize_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
        {
            textBox.TextChanged -= Capitalize_TextChanged;
            string text = textBox.Text;

            if (text.Length > 0)
            {
                textBox.Text = char.ToUpper(text[0]) + (text.Length > 1 ? text.Substring(1).ToLower() : "");
                textBox.CaretIndex = textBox.Text.Length;
            }

            textBox.TextChanged += Capitalize_TextChanged;
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

    private async void RegisterButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(FirstName.Text))
        {
            await MessageDialogWindow.Show(this, "Введите имя", "Ошибка");
            FirstName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(LastName.Text))
        {
            await MessageDialogWindow.Show(this, "Введите фамилию", "Ошибка");
            LastName.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(PhoneNumber.Text))
        {
            await MessageDialogWindow.Show(this, "Введите номер телефона", "Ошибка");
            PhoneNumber.Focus();
            return;
        }

        string phoneDigits = new string((PhoneNumber.Text ?? "").Where(char.IsDigit).ToArray());
        if (phoneDigits.Length != 11)
        {
            await MessageDialogWindow.Show(this, "Введите полный номер телефона", "Ошибка");
            PhoneNumber.Focus();
            return;
        }

        if (!phoneDigits.StartsWith("7") && !phoneDigits.StartsWith("8"))
        {
            await MessageDialogWindow.Show(this, "Номер должен начинаться с +7 или 8", "Ошибка");
            PhoneNumber.Focus();
            return;
        }

        string pass = Password.Text ?? "";
        if (string.IsNullOrWhiteSpace(pass))
        {
            await MessageDialogWindow.Show(this, "Введите пароль", "Ошибка");
            Password.Focus();
            return;
        }

        if (pass.Length < 8)
        {
            await MessageDialogWindow.Show(this, "Пароль должен содержать минимум 8 символов", "Ошибка");
            Password.Focus();
            return;
        }

        bool hasDigit = Regex.IsMatch(pass, @"\d");
        bool hasUpper = Regex.IsMatch(pass, @"[A-Z]");
        bool hasLower = Regex.IsMatch(pass, @"[a-z]");
        bool hasSpecial = Regex.IsMatch(pass, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

        if (!hasDigit || !hasUpper || !hasLower || !hasSpecial)
        {
            await MessageDialogWindow.Show(this,
                "Пароль должен содержать:\n- минимум 8 символов\n- хотя бы одну цифру\n- хотя бы одну заглавную букву\n- хотя бы одну строчную букву\n- хотя бы один спецсимвол",
                "Ошибка");
            Password.Focus();
            return;
        }

        string confirmPass = ConfirmPassword.Text ?? "";
        if (string.IsNullOrWhiteSpace(confirmPass))
        {
            await MessageDialogWindow.Show(this, "Подтвердите пароль", "Ошибка");
            ConfirmPassword.Focus();
            return;
        }

        if (pass != confirmPass)
        {
            await MessageDialogWindow.Show(this, "Пароли не совпадают", "Ошибка");
            ConfirmPassword.Focus();
            return;
        }

        if (!BirthDatePicker.SelectedDate.HasValue)
        {
            await MessageDialogWindow.Show(this, "Выберите дату рождения", "Ошибка");
            return;
        }

        DateOnly birth = DateOnly.FromDateTime(BirthDatePicker.SelectedDate.Value.DateTime);
        if (!IsAdult(birth))
        {
            await MessageDialogWindow.Show(this, "Регистрация доступна только с 18 лет", "Ошибка");
            return;
        }

        try
        {
            var dto = new UserDTO
            {
                FirstName = FirstName.Text?.Trim() ?? "",
                LastName = LastName.Text?.Trim() ?? "",
                Patronymic = Patronymic.Text?.Trim() ?? "",
                Phone = PhoneNumber.Text ?? "",
                Email = "fatkhlislamovad@mail.ru",
                Password = pass,
                BirthDate = birth,
                IdRole = 1
            };

            string json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("http://localhost:5128/api/User/Registration", content);
            string responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                await MessageDialogWindow.Show(this, "Регистрация прошла успешно!", "Успех");
                var authWindow = new MainWindow();
                authWindow.Show();
                Close();
            }
            else
            {
                await MessageDialogWindow.Show(this, responseText, "Ошибка");
            }
        }
        catch (Exception ex)
        {
            await MessageDialogWindow.Show(this, ex.Message, "Ошибка");
        }
    }

    private void AuthBT_Click(object? sender, RoutedEventArgs e)
    {
        var authWindow = new MainWindow();
        authWindow.Show();
        Close();
    }
}
