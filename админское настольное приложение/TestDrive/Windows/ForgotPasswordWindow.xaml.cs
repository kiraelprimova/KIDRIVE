using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace TestDrive.Windows
{
    public partial class ForgotPasswordWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        private string GeneratedCode = "";
        private bool isFormatting = false;

        public ForgotPasswordWindow()
        {
            InitializeComponent();
        }


        private void PhoneBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (isFormatting)
                return;

            isFormatting = true;
            string digits = new string(PhoneBox.Text.Where(char.IsDigit).ToArray());

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


        private void SendCodeButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                MessageBox.Show("Введите номер телефона");
                PhoneBox.Focus();
                return;
            }

            if (PhoneBox.Text.Length != 18)
            {
                MessageBox.Show("Введите полный номер телефона");
                PhoneBox.Focus();
                return;
            }

            Random random = new Random();

            GeneratedCode = random.Next(100000, 999999).ToString();
            System.Diagnostics.Debug.WriteLine($"Код восстановления: {GeneratedCode}");
            MessageBox.Show("Код отправлен. Проверьте окно Output.");
            CodePanel.Visibility = Visibility.Visible;
        }


        private void ConfirmCodeButton_Click(object sender,RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeBox.Text))
            {
                MessageBox.Show("Введите код");
                return;
            }

            if (CodeBox.Text != GeneratedCode)
            {
                MessageBox.Show("Неверный код");
                return;
            }

            PasswordPanel.Visibility = Visibility.Visible;
        }


        private void ShowPassword_Checked(object sender,RoutedEventArgs e)
        {
            PasswordText.Text = Password.Password;
            PasswordText.Visibility = Visibility.Visible;
            Password.Visibility = Visibility.Hidden;
        }

        private void ShowPassword_Unchecked(object sender,RoutedEventArgs e)
        {
            Password.Password = PasswordText.Text;
            PasswordText.Visibility = Visibility.Collapsed;
            Password.Visibility = Visibility.Visible;
        }


        private void Password_PasswordChanged( object sender,RoutedEventArgs e)
        {
            if (Password.Visibility == Visibility.Visible)
                PasswordText.Text = Password.Password;
        }

        private void PasswordText_TextChanged(object sender,TextChangedEventArgs e)
        {
            if (PasswordText.Visibility == Visibility.Visible)
                Password.Password = PasswordText.Text;
        }


        private void ShowConfirmPassword_Checked(object sender,RoutedEventArgs e)
        {
            ConfirmPasswordText.Text = ConfirmPassword.Password;
            ConfirmPasswordText.Visibility = Visibility.Visible;
            ConfirmPassword.Visibility = Visibility.Hidden;
        }

        private void ShowConfirmPassword_Unchecked(object sender,RoutedEventArgs e)
        {
            ConfirmPassword.Password = ConfirmPasswordText.Text;
            ConfirmPasswordText.Visibility = Visibility.Collapsed;
            ConfirmPassword.Visibility = Visibility.Visible;
        }


        private void ConfirmPassword_PasswordChanged(object sender,RoutedEventArgs e)
        {
            if (ConfirmPassword.Visibility == Visibility.Visible)
                ConfirmPasswordText.Text = ConfirmPassword.Password;
        }

        private void ConfirmPasswordText_TextChanged(object sender,TextChangedEventArgs e)
        {
            if (ConfirmPasswordText.Visibility == Visibility.Visible)
                ConfirmPassword.Password = ConfirmPasswordText.Text;
        }


        private async void ChangePasswordButton_Click(object sender,RoutedEventArgs e)
        {
            string password;

            if (Password.Visibility == Visibility.Visible)
            {
                password = Password.Password;
            }
            else
            {
                password = PasswordText.Text;
            }

            string confirmPassword;

            if (ConfirmPassword.Visibility == Visibility.Visible)
            {
                confirmPassword = ConfirmPassword.Password;
            }
            else
            {
                confirmPassword = ConfirmPasswordText.Text;
            }


            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Введите пароль");
                return;
            }


            if (password.Length < 8)
            {
                MessageBox.Show("Пароль должен содержать минимум 8 символов");
                return;
            }


            bool hasDigit = Regex.IsMatch(password, @"\d");
            bool hasUpper = Regex.IsMatch(password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(password, @"[a-z]");
            bool hasSpecial = Regex.IsMatch( password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

            if (!hasDigit ||
                !hasUpper ||
                !hasLower ||
                !hasSpecial)
            {
                MessageBox.Show(
                    "Пароль должен содержать:\n" +
                    "- минимум 8 символов\n" +
                    "- хотя бы одну цифру\n" +
                    "- хотя бы одну заглавную букву\n" +
                    "- хотя бы одну строчную букву\n" +
                    "- хотя бы один спецсимвол");

                return;
            }


            if (password != confirmPassword)
            {
                MessageBox.Show("Пароли не совпадают");
                return;
            }

            string url = $"http://localhost:5128/api/User/ChangePassword?phone={PhoneBox.Text}&newPassword={password}";
            var response = await client.PutAsync(url, null);
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                MessageBox.Show("Пароль успешно изменён");
                Close();
            }
            else
            {
                MessageBox.Show(result);
            }
        }
    }
}