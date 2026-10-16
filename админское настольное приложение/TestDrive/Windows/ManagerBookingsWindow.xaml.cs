using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using TestDrive.UserControls;

namespace TestDrive.Windows
{
    public partial class ManagerBookingsWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        private List<dynamic> AllBookings;
        private List<dynamic> AllCategories;

        public ManagerBookingsWindow()
        {
            InitializeComponent();
            StatusFilterBox.SelectedIndex = 0;
            LoadBookings();
        }

        private async void LoadBookings()
        {
            var bookingResponse = await client.GetAsync("http://localhost:5128/api/Booking");
            var categoryResponse = await client.GetAsync("http://localhost:5128/api/Category");

            string bookingJson = await bookingResponse.Content.ReadAsStringAsync();
            string categoryJson = await categoryResponse.Content.ReadAsStringAsync();

            AllBookings = JsonConvert.DeserializeObject<List<dynamic>>(bookingJson);
            AllCategories = JsonConvert.DeserializeObject<List<dynamic>>(categoryJson);

            ApplySortAndRender();
        }

        private void ApplySortAndRender()
        {
            if (AllBookings == null) return;

            IEnumerable<dynamic> filtered = AllBookings;

            string selected = (StatusFilterBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

            switch (selected)
            {
                case "В рассмотрении":
                    filtered = filtered.Where(b => b.idStatus == 1);
                    break;

                case "Подтверждено":
                    filtered = filtered.Where(b => b.idStatus == 2);
                    break;

                case "Отменено":
                    filtered = filtered.Where(b => b.idStatus == 3);
                    break;
            }

            var sorted = filtered
                .OrderBy(b => b.idStatus != 1) // 1 всегда сверху
                .ThenByDescending(b => DateTime.Parse(b.bookingDate.ToString()))
                .ThenByDescending(b => TimeSpan.Parse(b.bookingTime.ToString()))
                .ToList();

            RenderBookings(sorted);
        }

        private void RenderBookings(IEnumerable<dynamic> bookings)
        {
            BookingsPanel.Children.Clear();

            foreach (var booking in bookings)
            {
                if (booking.idCarNavigation == null ||
                    booking.idUserNavigation == null ||
                    booking.idStatusNavigation == null)
                    continue;

                var category = AllCategories
                    .FirstOrDefault(c => c.idCategory == booking.idCarNavigation.idCategory);

                if (category == null)
                    continue;

                ManagerBookingCard card = new ManagerBookingCard();
                card.SetData(booking, category);

                card.BookingStatusChanged += () =>
                {
                    LoadBookings(); // пересортировка после изменения статуса
                };

                BookingsPanel.Children.Add(card);
            }
        }

        private void StatusFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplySortAndRender();
        }

        private void CarsButton_Click(object sender, RoutedEventArgs e)
        {
            new ManagerCarsWindow().Show();
            Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            new ManagerProfileWindow().Show();
            Close();
        }

        private void AnalyticsButton_Click(object sender, RoutedEventArgs e)
        {
            new ManagerAnalyticsWindow().Show();
            Close();
        }
    }
}