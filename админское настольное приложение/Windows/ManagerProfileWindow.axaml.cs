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

public partial class ManagerProfileWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    private string originalFirstName = "";
    private string originalLastName = "";
    private string originalPatronymic = "";
    private string originalPhone = "";
    private string originalEmail = "";
    private string originalPassword = "";
    private DateTimeOffset? originalBirthDate;

    public ManagerProfileWindow()
    {
        InitializeComponent();

        PhoneBox.TextChanged += PhoneNumber_TextChanged;
        FirstNameBox.TextChanged += Capitalize_TextChanged;
        LastNameBox.TextChanged += Capitalize_TextChanged;
        PatronymicBox.TextChanged += Capitalize_TextChanged;

        LoadProfile();
    }

    private async void LoadProfile()
    {
        try
        {
            var response = await client.GetAsync($"http://localhost:5128/api/User/{ManagerCarsWindow.CurrentUserId}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic? user = JsonConvert.DeserializeObject(json);

            if (user != null)
            {
                FirstNameBox.Text = user.firstName;
                LastNameBox.Text = user.lastName;
                PatronymicBox.Text = user.patronymic;
                PhoneBox.Text = user.phone;
                EmailBox.Text = user.email;
                originalPassword = user.password ?? "";

                if (user.birthDate != null)
                {
                    DateTime dt = DateTime.Parse(user.birthDate.ToString());
                    BirthDateBox.SelectedDate = new DateTimeOffset(dt);
                }

                SaveOriginalData();

                UserText.Text = $"{user.firstName} {user.lastName}";
                EmailText.Text = user.email;
            }
        }
        catch { }
    }

    private void SaveOriginalData()
    {
        originalFirstName = FirstNameBox.Text ?? "";
        originalLastName = LastNameBox.Text ?? "";
        originalPatronymic = PatronymicBox.Text ?? "";
        originalPhone = PhoneBox.Text ?? "";
        originalEmail = EmailBox.Text ?? "";
        originalBirthDate = BirthDateBox.SelectedDate;
    }

    private bool ValidEmail(string email)
    {
        return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    private bool IsAdult(DateOnly birthDate)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        int age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
            age--;
        return age >= 18;
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
        if (sender is TextBox textBox)
        {
            textBox.TextChanged -= Capitalize_TextChanged;

            string text = textBox.Text ?? "";
            if (text.Length > 0)
            {
                textBox.Text = char.ToUpper(text[0]) + (text.Length > 1 ? text.Substring(1).ToLower() : "");
                textBox.CaretIndex = textBox.Text.Length;
            }

            textBox.TextChanged += Capitalize_TextChanged;
        }
    }

    private async void ChangePasswordButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ForgotPasswordWindow();
        await window.ShowDialog(this);
    }

    private async void SaveButton_Click(object? sender, RoutedEventArgs e)
    {
        string firstName = FirstNameBox.Text?.Trim() ?? "";
        string lastName = LastNameBox.Text?.Trim() ?? "";
        string patronymic = PatronymicBox.Text?.Trim() ?? "";
        string phone = PhoneBox.Text ?? "";
        string email = EmailBox.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(firstName))
        {
            await MessageDialogWindow.Show(this, "Введите имя", "Предупреждение");
            return;
        }
        if (string.IsNullOrWhiteSpace(lastName))
        {
            await MessageDialogWindow.Show(this, "Введите фамилию", "Предупреждение");
            return;
        }
        if (string.IsNullOrWhiteSpace(phone))
        {
            await MessageDialogWindow.Show(this, "Введите номер телефона", "Предупреждение");
            return;
        }
        if (string.IsNullOrWhiteSpace(email) || !ValidEmail(email))
        {
            await MessageDialogWindow.Show(this, "Введите корректный адрес электронной почты", "Предупреждение");
            return;
        }

        if (!BirthDateBox.SelectedDate.HasValue)
        {
            await MessageDialogWindow.Show(this, "Выберите дату рождения", "Предупреждение");
            return;
        }

        DateOnly birth = DateOnly.FromDateTime(BirthDateBox.SelectedDate.Value.DateTime);
        if (!IsAdult(birth))
        {
            await MessageDialogWindow.Show(this, "Возраст должен быть от 18 лет", "Предупреждение");
            return;
        }

        try
        {
            var dto = new UserDTO
            {
                FirstName = firstName,
                LastName = lastName,
                Patronymic = patronymic,
                Phone = phone,
                Email = email,
                Password = originalPassword,
                BirthDate = birth,
                IdRole = 2 // менеджер
            };

            string json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"http://localhost:5128/api/User/{ManagerCarsWindow.CurrentUserId}", content);

            if (response.IsSuccessStatusCode)
            {
                await MessageDialogWindow.Show(this, "Профиль успешно обновлен!", "Успех");
                SaveOriginalData();
                UserText.Text = $"{firstName} {lastName}";
                EmailText.Text = email;
            }
            else
            {
                string res = await response.Content.ReadAsStringAsync();
                await MessageDialogWindow.Show(this, res, "Ошибка");
            }
        }
        catch (Exception ex)
        {
            await MessageDialogWindow.Show(this, ex.Message, "Ошибка");
        }
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

    private void AnalyticsButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new ManagerAnalyticsWindow();
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
