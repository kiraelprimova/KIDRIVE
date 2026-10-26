using Avalonia.Controls;
using Avalonia.Interactivity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using TestDriveAvalonia.Classes;

namespace TestDriveAvalonia.Windows;

public partial class BookingWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    private dynamic carData;
    public static int CurrentUserId;

    public class TimeItem
    {
        public string TimeString { get; set; } = "";
        public TimeSpan TimeValue { get; set; }
        public bool IsAvailable { get; set; }

        public override string ToString() => IsAvailable ? TimeString : $"{TimeString} (занято)";
    }

    public BookingWindow()
    {
        InitializeComponent();
        carData = new object();
    }

    public BookingWindow(dynamic car) : this()
    {
        carData = car;
        LoadCategory();
        BookingDate.SelectedDateChanged += BookingDate_SelectedDateChanged;
    }

    private async void LoadCategory()
    {
        try
        {
            var response = await client.GetAsync($"http://localhost:5128/api/Category/{carData.idCategory}");
            string json = await response.Content.ReadAsStringAsync();
            dynamic? category = JsonConvert.DeserializeObject(json);

            string categoryName = category != null ? category.nameCategory.ToString() : "";
            CarText.Text = $"{categoryName} {carData.model}";
            CarImage.Source = ImageHelper.LoadBitmap((string?)carData.imagePath);
            PriceText.Text = $"{carData.price} ₽";
        }
        catch { }
    }

    private async void BookingDate_SelectedDateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
    {
        if (BookingDate.SelectedDate == null) return;

        string formattedDate = BookingDate.SelectedDate.Value.ToString("yyyy-MM-dd");
        int carId = (int)carData.idCar;

        try
        {
            var response = await client.GetAsync($"http://localhost:5128/api/Car/GetBusyTimes?carId={carId}&date={formattedDate}");
            if (!response.IsSuccessStatusCode) return;

            string json = await response.Content.ReadAsStringAsync();
            var busyTimes = JsonConvert.DeserializeObject<List<TimeSpan>>(json) ?? new List<TimeSpan>();

            var allTimes = new List<TimeSpan>
            {
                new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0), new TimeSpan(11, 0, 0),
                new TimeSpan(12, 0, 0), new TimeSpan(13, 0, 0), new TimeSpan(14, 0, 0),
                new TimeSpan(15, 0, 0), new TimeSpan(16, 0, 0), new TimeSpan(17, 0, 0)
            };

            var items = new List<TimeItem>();
            foreach (var time in allTimes)
            {
                bool isBusy = busyTimes.Any(bt => bt == time);
                items.Add(new TimeItem
                {
                    TimeString = $"{time.Hours}:00",
                    TimeValue = time,
                    IsAvailable = !isBusy
                });
            }

            TimeBox.ItemsSource = items;
            TimeBox.SelectedIndex = -1;
        }
        catch { }
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private async void BookingButton_Click(object? sender, RoutedEventArgs e)
    {
        if (BookingDate.SelectedDate == null || TimeBox.SelectedItem == null)
        {
            await MessageDialogWindow.Show(this, "Заполните дату и время", "Ошибка");
            return;
        }

        var selectedTimeItem = (TimeItem)TimeBox.SelectedItem;
        if (!selectedTimeItem.IsAvailable)
        {
            await MessageDialogWindow.Show(this, "Это время недоступно для бронирования", "Ошибка");
            return;
        }

        DateTime selectedDate = BookingDate.SelectedDate.Value.Date;
        DateTime today = DateTime.Today;

        if (selectedDate < today)
        {
            await MessageDialogWindow.Show(this, "Нельзя выбрать прошедшую дату", "Ошибка");
            return;
        }

        if (selectedDate > today.AddMonths(1))
        {
            await MessageDialogWindow.Show(this, "Бронирование доступно только в пределах месяца", "Ошибка");
            return;
        }

        if (AgreementCheck.IsChecked != true)
        {
            await MessageDialogWindow.Show(this, "Для оформления бронирования необходимо согласиться с условиями", "Ошибка");
            return;
        }

        int payment = 0;
        if (CashCheck.IsChecked == true) payment = 1;
        if (CardCheck.IsChecked == true) payment = 2;

        if (payment == 0)
        {
            await MessageDialogWindow.Show(this, "Выберите способ оплаты", "Ошибка");
            return;
        }

        try
        {
            var booking = new
            {
                idUser = CurrentUserId,
                idCar = (int)carData.idCar,
                bookingDate = BookingDate.SelectedDate.Value.ToString("yyyy-MM-dd"),
                bookingTime = selectedTimeItem.TimeValue,
                idStatus = 1,
                idPayment = payment
            };

            string json = JsonConvert.SerializeObject(booking);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("http://localhost:5128/api/Booking", content);
            string result = await response.Content.ReadAsStringAsync();

            await MessageDialogWindow.Show(this, result, "Результат");
            if (response.IsSuccessStatusCode)
            {
                Close();
            }
        }
        catch (Exception ex)
        {
            await MessageDialogWindow.Show(this, ex.Message, "Ошибка");
        }
    }
}
