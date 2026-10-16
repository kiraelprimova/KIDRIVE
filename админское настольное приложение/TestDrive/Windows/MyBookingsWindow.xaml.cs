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
    public partial class MyBookingsWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        public static int CurrentUserId;

        private List<dynamic> AllBookings;
        private List<dynamic> AllCategories;

        public MyBookingsWindow()
        {
            InitializeComponent();
            StatusFilterBox.SelectedIndex = 0;
            LoadBookings();
        }

        public void RefreshBookings()
        {
            LoadBookings();
        }

        private async void LoadBookings()
        {
            BookingsPanel.Children.Clear();

            var bookingResponse = await client.GetAsync("http://localhost:5128/api/Booking");
            var categoryResponse = await client.GetAsync("http://localhost:5128/api/Category");
            string bookingJson = await bookingResponse.Content.ReadAsStringAsync();
            string categoryJson = await categoryResponse.Content.ReadAsStringAsync();
            List<dynamic> bookings = JsonConvert.DeserializeObject<List<dynamic>>(bookingJson);
            AllCategories = JsonConvert.DeserializeObject<List<dynamic>>(categoryJson);

            AllBookings = bookings
                .Where(b => (int)b.idUser == CurrentUserId)
                .OrderByDescending(
                    b => DateTime.Parse(
                        b.bookingDate.ToString()))
                .ThenByDescending(
                    b => TimeSpan.Parse(
                        b.bookingTime.ToString()))
                .ToList();

            RenderBookings(AllBookings);
        }

        private void RenderBookings(IEnumerable<dynamic> bookings)
        {
            BookingsPanel.Children.Clear();

            if (!bookings.Any())
            {
                EmptyBookingsText.Visibility = Visibility.Visible;
                return;
            }

            EmptyBookingsText.Visibility = Visibility.Collapsed;

            foreach (var booking in bookings)
            {
                if (booking.idCarNavigation == null)
                    continue;

                if (booking.idStatusNavigation == null)
                    continue;

                var category = AllCategories.FirstOrDefault(
                        c =>
                        c.idCategory ==
                        booking.idCarNavigation.idCategory);

                if (category == null)
                    continue;

                BookingCard card = new BookingCard();

                card.SetData(
                    booking,
                    booking.idCarNavigation,
                    category,
                    booking.idStatusNavigation);

                card.BookingCancelled += () =>
                {
                    RefreshBookings();
                };

                BookingsPanel.Children.Add(card);
            }
        }

        private void StatusFilterBox_SelectionChanged(
            object sender,
            SelectionChangedEventArgs e)
        {
            if (AllBookings == null)
                return;

            string selected =
                (StatusFilterBox.SelectedItem
                as ComboBoxItem)?
                .Content?
                .ToString();

            IEnumerable<dynamic> filtered =
                AllBookings;

            switch (selected)
            {
                case "В рассмотрении":

                    filtered = AllBookings.Where(b => b.idStatus == 1);
                    break;

                case "Подтверждено":

                    filtered = AllBookings.Where(b => b.idStatus == 2);
                    break;

                case "Отменено":

                    filtered = AllBookings.Where(b => b.idStatus == 3);
                    break;
            }

            RenderBookings(filtered);
        }

        private void CatalogButton_Click(object sender,RoutedEventArgs e)
        {
            CatalogWindow window = new CatalogWindow();
            window.Show();
            Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ProfileWindow window = new ProfileWindow();
            window.Show();
            Close();
        }

        private void ExitButton_Click(object sender,RoutedEventArgs e)
        {
            MainWindow window = new MainWindow();
            window.Show();
            Close();
        }
    }
}