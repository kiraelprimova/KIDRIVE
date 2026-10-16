using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace TestDrive.Windows
{
    public partial class ProfileWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();
        public static int CurrentUserId;
        private bool isEdit = false;
        private string originalFirstName;
        private string originalLastName;
        private string originalPatronymic;
        private string originalPhone;
        private string originalEmail;
        private string originalPassword;
        private DateTime? originalBirthDate;

        public ProfileWindow()
        {
            InitializeComponent();
            LoadProfile();
            BirthDateBox.DisplayDateEnd = DateTime.Today;
            BirthDateBox.DisplayDateStart = DateTime.Today.AddYears(-100);
        }

        private async void LoadProfile()
        {
            var response = await client.GetAsync($"http://localhost:5128/api/User/{CurrentUserId}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic user = JsonConvert.DeserializeObject(json);

            FirstNameBox.Text = user.firstName;
            LastNameBox.Text = user.lastName;
            PatronymicBox.Text = user.patronymic;
            PhoneBox.Text = user.phone;
            EmailBox.Text = user.email;
            PasswordBox.Password = user.password;
            ConfirmPasswordBox.Password = user.password;
            PasswordTextBox.Text = user.password;
            ConfirmPasswordTextBox.Text = user.password;
            BirthDateBox.SelectedDate = DateTime.Parse(user.birthDate.ToString());

            SaveOriginalData();

            UserNameText.Text = $"{user.firstName} {user.lastName}";
            UserEmailText.Text = user.email;
        }

        private void SaveOriginalData()
        {
            originalFirstName = FirstNameBox.Text;
            originalLastName = LastNameBox.Text;
            originalPatronymic = PatronymicBox.Text;
            originalPhone = PhoneBox.Text;
            originalEmail = EmailBox.Text;
            originalPassword = PasswordBox.Password;
            originalBirthDate = BirthDateBox.SelectedDate;
        }

        private void RestoreOriginalData()
        {
            FirstNameBox.Text = originalFirstName;
            LastNameBox.Text = originalLastName;
            PatronymicBox.Text = originalPatronymic;
            PhoneBox.Text = originalPhone;
            EmailBox.Text = originalEmail;
            PasswordBox.Password = originalPassword;
            ConfirmPasswordBox.Password = originalPassword;
            PasswordTextBox.Text = originalPassword;
            ConfirmPasswordTextBox.Text = originalPassword;
            BirthDateBox.SelectedDate = originalBirthDate;
        }

        private void SetEdit(bool value)
        {
            FirstNameBox.IsEnabled = value;
            LastNameBox.IsEnabled = value;
            PatronymicBox.IsEnabled = value;
            PhoneBox.IsEnabled = value;
            EmailBox.IsEnabled = value;
            PasswordBox.IsEnabled = value;
            ConfirmPasswordBox.IsEnabled = value;
            PasswordTextBox.IsEnabled = value;
            ConfirmPasswordTextBox.IsEnabled = value;
            BirthDateBox.IsEnabled = value;
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

        private void Email_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = EmailBox.Text;
            string filtered = Regex.Replace(text, @"[А-Яа-яЁё]", "");

            if (text != filtered)
            {
                int caret = EmailBox.CaretIndex - (text.Length - filtered.Length);
                EmailBox.Text = filtered;
                EmailBox.CaretIndex = Math.Max(caret, 0);
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isEdit)
            {
                SaveOriginalData();
                isEdit = true;
                SetEdit(true);
                EditButtonText.Text = "Сохранить";
                CancelButton.Visibility = Visibility.Visible;
                return;
            }

            if (string.IsNullOrWhiteSpace(FirstNameBox.Text))
            {
                MessageBox.Show(
                    "Введите имя",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                FirstNameBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(LastNameBox.Text))
            {
                MessageBox.Show(
                    "Введите фамилию",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                LastNameBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(PhoneBox.Text))
            {
                MessageBox.Show(
                    "Введите номер телефона",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PhoneBox.Focus();
                return;
            }

            if (PhoneBox.Text.Length != 18)
            {
                MessageBox.Show(
                    "Введите полный номер телефона",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PhoneBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(EmailBox.Text))
            {
                MessageBox.Show(
                    "Введите электронную почту",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                EmailBox.Focus();
                return;
            }

            if (Regex.IsMatch(EmailBox.Text, @"[А-Яа-яЁё]"))
            {
                MessageBox.Show(
                    "Email не должен содержать русские буквы",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                EmailBox.Focus();
                return;
            }

            if (!ValidEmail(EmailBox.Text))
            {
                MessageBox.Show(
                    "Введите корректный email",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                EmailBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show(
                    "Введите пароль",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PasswordBox.Focus();
                return;
            }

            if (PasswordBox.Password.Length < 8)
            {
                MessageBox.Show(
                    "Пароль должен содержать минимум 8 символов",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                PasswordBox.Focus();
                return;
            }

            bool hasDigit = Regex.IsMatch(PasswordBox.Password, @"\d");
            bool hasUpper = Regex.IsMatch(PasswordBox.Password, @"[A-Z]");
            bool hasLower = Regex.IsMatch(PasswordBox.Password, @"[a-z]");
            bool hasSpecial = Regex.IsMatch(
                PasswordBox.Password,
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

                PasswordBox.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(ConfirmPasswordBox.Password))
            {
                MessageBox.Show(
                    "Подтвердите пароль",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ConfirmPasswordBox.Focus();
                return;
            }

            if (PasswordBox.Password != ConfirmPasswordBox.Password)
            {
                MessageBox.Show(
                    "Пароли не совпадают",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                ConfirmPasswordBox.Focus();
                return;
            }

            if (!BirthDateBox.SelectedDate.HasValue)
            {
                MessageBox.Show(
                    "Выберите дату рождения",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            DateOnly birth = DateOnly.FromDateTime(BirthDateBox.SelectedDate.Value);

            if (!IsAdult(birth))
            {
                MessageBox.Show(
                    "Редактирование доступно только с 18 лет",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            var user = new
            {
                firstName = FirstNameBox.Text,
                lastName = LastNameBox.Text,
                patronymic = PatronymicBox.Text,
                phone = PhoneBox.Text,
                email = EmailBox.Text,
                password = PasswordBox.Password,
                birthDate = BirthDateBox.SelectedDate.Value.ToString("yyyy-MM-dd")
            };

            string json = JsonConvert.SerializeObject(user);
            StringContent content = new StringContent(json,Encoding.UTF8,"application/json");
            var response = await client.PutAsync($"http://localhost:5128/api/User/{CurrentUserId}",content);
            string result = await response.Content.ReadAsStringAsync();
            MessageBox.Show(result);

            if (response.IsSuccessStatusCode)
            {
                SaveOriginalData();
                isEdit = false;
                SetEdit(false);
                EditButtonText.Text = "Редактировать";
                CancelButton.Visibility = Visibility.Collapsed;
                UserNameText.Text = $"{FirstNameBox.Text} {LastNameBox.Text}";
                UserEmailText.Text = EmailBox.Text;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            RestoreOriginalData();
            isEdit = false;
            SetEdit(false);
            EditButtonText.Text = "Редактировать";
            CancelButton.Visibility = Visibility.Collapsed;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var response = await client.PutAsync($"http://localhost:5128/api/User/DeleteUser/{CurrentUserId}",null);

            if (response.IsSuccessStatusCode)
            {
                MainWindow window = new MainWindow();
                window.Show();
                Close();
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PasswordBox.Visibility == Visibility.Visible)
                PasswordTextBox.Text = PasswordBox.Password;
        }

        private void PasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PasswordTextBox.Visibility == Visibility.Visible)
                PasswordBox.Password = PasswordTextBox.Text;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ConfirmPasswordBox.Visibility == Visibility.Visible)
                ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
        }

        private void ConfirmPasswordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ConfirmPasswordTextBox.Visibility == Visibility.Visible)
                ConfirmPasswordBox.Password = ConfirmPasswordTextBox.Text;
        }

        private void ShowPasswordBox_Checked(object sender, RoutedEventArgs e)
        {
            PasswordTextBox.Text = PasswordBox.Password;
            PasswordTextBox.Visibility = Visibility.Visible;
            PasswordBox.Visibility = Visibility.Hidden;
        }

        private void ShowPasswordBox_Unchecked(object sender, RoutedEventArgs e)
        {
            PasswordBox.Password = PasswordTextBox.Text;
            PasswordTextBox.Visibility = Visibility.Collapsed;
            PasswordBox.Visibility = Visibility.Visible;
        }

        private void ShowConfirmPasswordBox_Checked(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordTextBox.Text = ConfirmPasswordBox.Password;
            ConfirmPasswordTextBox.Visibility = Visibility.Visible;
            ConfirmPasswordBox.Visibility = Visibility.Hidden;
        }

        private void ShowConfirmPasswordBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfirmPasswordBox.Password =ConfirmPasswordTextBox.Text;
            ConfirmPasswordTextBox.Visibility =Visibility.Collapsed;
            ConfirmPasswordBox.Visibility = Visibility.Visible;
        }

        private void CatalogButton_Click(object sender, RoutedEventArgs e)
        {
            CatalogWindow window = new CatalogWindow();
            window.Show();
            Close();
        }

        private void BookingsButton_Click(object sender, RoutedEventArgs e)
        {
            MyBookingsWindow window = new MyBookingsWindow();
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