using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace TestDriveAvalonia.Windows;

public partial class ForgotPasswordWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    private string GeneratedCode = "";
    private bool isFormatting = false;

    public ForgotPasswordWindow()
    {
        InitializeComponent();

        PhoneBox.TextChanged += PhoneBox_TextChanged;

        ShowPassword.IsCheckedChanged += (s, e) =>
        {
            Password.PasswordChar = ShowPassword.IsChecked == true ? '\0' : '•';
        };

        ShowConfirmPassword.IsCheckedChanged += (s, e) =>
        {
            ConfirmPassword.PasswordChar = ShowConfirmPassword.IsChecked == true ? '\0' : '•';
        };
    }

    private void PhoneBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (isFormatting)
            return;

        isFormatting = true;
        string raw = PhoneBox.Text ?? "";
        string digits = new string(raw.Where(char.IsDigit).ToArray());

        if (digits.StartsWith("8"))
            digits = "7" + digits.Substring(1);

        if (digits.StartsWith("7"))
            digits = digits.Substring(1);

        if (digits.Length > 10)
            digits = digits.Substring(0, 10);

        string formatted = "+7";

        if (digits.Length > 0)
            formatted += " (" + digits.Substring(0, Math.Min(3, digits.Length));

        if (digits.Length >= 3)
            formatted += ")";

        if (digits.Length > 3)
            formatted += " " + digits.Substring(3, Math.Min(3, digits.Length - 3));

        if (digits.Length > 6)
            formatted += "-" + digits.Substring(6, Math.Min(2, digits.Length - 6));

        if (digits.Length > 8)
            formatted += "-" + digits.Substring(8, Math.Min(2, digits.Length - 8));

        PhoneBox.Text = formatted;
        PhoneBox.CaretIndex = PhoneBox.Text.Length;
        isFormatting = false;
    }

    private async void SendCodeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(PhoneBox.Text))
        {
            await MessageDialogWindow.Show(this, "Введите номер телефона", "Ошибка");
            PhoneBox.Focus();
            return;
        }

        if (PhoneBox.Text.Length != 18)
        {
            await MessageDialogWindow.Show(this, "Введите полный номер телефона", "Ошибка");
            PhoneBox.Focus();
            return;
        }

        Random random = new Random();
        GeneratedCode = random.Next(100000, 999999).ToString();

        await MessageDialogWindow.Show(this, $"Код подтверждения: {GeneratedCode}", "Код отправлен");
        CodePanel.IsVisible = true;
    }

    private async void ConfirmCodeButton_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CodeBox.Text))
        {
            await MessageDialogWindow.Show(this, "Введите код", "Ошибка");
            return;
        }

        if (CodeBox.Text != GeneratedCode)
        {
            await MessageDialogWindow.Show(this, "Неверный код", "Ошибка");
            return;
        }

        PasswordPanel.IsVisible = true;
    }

    private async void ChangePasswordButton_Click(object? sender, RoutedEventArgs e)
    {
        string password = Password.Text ?? "";
        string confirmPassword = ConfirmPassword.Text ?? "";

        if (string.IsNullOrWhiteSpace(password))
        {
            await MessageDialogWindow.Show(this, "Введите пароль", "Ошибка");
            return;
        }

        if (password.Length < 8)
        {
            await MessageDialogWindow.Show(this, "Пароль должен содержать минимум 8 символов", "Ошибка");
            return;
        }

        bool hasDigit = Regex.IsMatch(password, @"\d");
        bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
        bool hasLower = Regex.IsMatch(password, @"[a-z]");
        bool hasSpecial = Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

        if (!hasDigit || !hasUpper || !hasLower || !hasSpecial)
        {
            await MessageDialogWindow.Show(this,
                "Пароль должен содержать:\n- минимум 8 символов\n- хотя бы одну цифру\n- хотя бы одну заглавную букву\n- хотя бы одну строчную букву\n- хотя бы один спецсимвол",
                "Ошибка");
            return;
        }

        if (password != confirmPassword)
        {
            await MessageDialogWindow.Show(this, "Пароли не совпадают", "Ошибка");
            return;
        }

        try
        {
            string url = $"http://localhost:5128/api/User/ChangePassword?phone={Uri.EscapeDataString(PhoneBox.Text ?? "")}&newPassword={Uri.EscapeDataString(password)}";
            var response = await client.PutAsync(url, null);
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                await MessageDialogWindow.Show(this, "Пароль успешно изменён", "Успех");
                Close();
            }
            else
            {
                await MessageDialogWindow.Show(this, result, "Ошибка");
            }
        }
        catch (Exception ex)
        {
            await MessageDialogWindow.Show(this, ex.Message, "Ошибка");
        }
    }
}
