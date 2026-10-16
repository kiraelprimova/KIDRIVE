using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;

namespace TestDrive.Windows
{
    public partial class ManagerAnalyticsWindow : Window
    {
        private static readonly HttpClient client = new HttpClient();

        public ManagerAnalyticsWindow()
        {
            InitializeComponent();

            LoadDashboard();
            LoadPopularCars();
            LoadStatuses();
            LoadBookingsByDate();
            LoadBrands();
        }

        private async void LoadDashboard()
        {
            string json = await client.GetStringAsync(
                "http://localhost:5128/api/Analytics/Dashboard");

            dynamic data = JsonConvert.DeserializeObject(json);

            CarsCountText.Text = data.totalCars.ToString();
            BookingsCountText.Text = data.totalBookings.ToString();
            ClientsCountText.Text = data.totalClients.ToString();
            ConfirmedText.Text = data.confirmedBookings.ToString();
            RevenueText.Text = data.totalRevenue + " ₽";
        }

        private async void LoadPopularCars()
        {
            string json = await client.GetStringAsync(
                "http://localhost:5128/api/Analytics/PopularCars");

            var data = JsonConvert.DeserializeObject<List<dynamic>>(json);

            CarsChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Values = new ChartValues<int>(
                        data.Select(x => (int)x.total))
                }
            };

            CarsChart.AxisX.Add(new Axis
            {
                Labels = data.Select(x => (string)x.name).ToList(),
                Separator = new Separator { Step = 1 }
            });

        }

        private async void LoadStatuses()
        {
            string json = await client.GetStringAsync(
                "http://localhost:5128/api/Analytics/BookingStatuses");

            var data = JsonConvert.DeserializeObject<List<dynamic>>(json);

            SeriesCollection series = new SeriesCollection();

            foreach (var item in data)
            {
                series.Add(new PieSeries
                {
                    Title = item.status.ToString(),
                    Values = new ChartValues<int>
                    {
                        (int)item.total
                    }
                });
            }

            StatusChart.Series = series;
        }

        private async void LoadBookingsByDate()
        {
            string json = await client.GetStringAsync(
                "http://localhost:5128/api/Analytics/BookingsByDate");

            var data = JsonConvert.DeserializeObject<List<dynamic>>(json);

            BookingsChart.Series = new SeriesCollection
            {
                new LineSeries
                {
                    Values = new ChartValues<int>(
                        data.Select(x => (int)x.total))
                }
            };

            List<string> labels = new List<string>();

            foreach (var item in data)
            {
                labels.Add(
                    DateTime.Parse(item.date.ToString())
                    .ToString("dd.MM.yyyy"));
            }

            BookingsChart.AxisX.Clear();

            BookingsChart.AxisX.Add(new Axis
            {
                Labels = labels
            });
        }

        private async void LoadBrands()
        {
            string json = await client.GetStringAsync(
                "http://localhost:5128/api/Analytics/PopularBrands");

            var data = JsonConvert.DeserializeObject<List<dynamic>>(json);

            BrandsChart.Series = new SeriesCollection
            {
                new ColumnSeries
                {
                    Values = new ChartValues<int>(
                        data.Select(x => (int)x.total))
                }
            };

            BrandsChart.AxisX.Add(new Axis
            {
                Labels = data.Select(x => (string)x.brand).ToList(),
                Separator = new Separator { Step = 1 } 
            });
        }

        private void CarsButton_Click(object sender, RoutedEventArgs e)
        {
            ManagerCarsWindow window = new ManagerCarsWindow();
            window.Show();
            Close();
        }

        private void BookingsButton_Click(object sender, RoutedEventArgs e)
        {
            ManagerBookingsWindow window =
                new ManagerBookingsWindow();

            window.Show();
            Close();
        }

        private void ProfileButton_Click(object sender, RoutedEventArgs e)
        {
            ManagerProfileWindow window =
                new ManagerProfileWindow();

            window.Show();
            Close();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }
    }
}
