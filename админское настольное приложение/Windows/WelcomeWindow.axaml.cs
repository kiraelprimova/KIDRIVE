using Avalonia.Controls;
using Avalonia.Interactivity;

namespace TestDriveAvalonia.Windows;

public partial class WelcomeWindow : Window
{
    public WelcomeWindow()
    {
        InitializeComponent();
    }

    private void LoginButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new MainWindow();
        window.Show();
        Close();
    }

    private void RegisterButton_Click(object? sender, RoutedEventArgs e)
    {
        var window = new RegistrationWindow();
        window.Show();
        Close();
    }
}
