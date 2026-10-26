using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Threading.Tasks;

namespace TestDriveAvalonia.Windows;

public partial class MessageDialogWindow : Window
{
    public bool DialogResultValue { get; private set; } = false;

    public MessageDialogWindow()
    {
        InitializeComponent();
    }

    public MessageDialogWindow(string message, string title = "Сообщение", bool isConfirm = false) : this()
    {
        Title = title;
        MessageText.Text = message;
        if (isConfirm)
        {
            CancelButton.IsVisible = true;
            OkButton.Content = "Да";
        }
    }

    private void OkButton_Click(object? sender, RoutedEventArgs e)
    {
        DialogResultValue = true;
        Close();
    }

    private void CancelButton_Click(object? sender, RoutedEventArgs e)
    {
        DialogResultValue = false;
        Close();
    }

    public static async Task<bool> Show(Window? owner, string message, string title = "Сообщение", bool isConfirm = false)
    {
        var dlg = new MessageDialogWindow(message, title, isConfirm);
        if (owner != null)
        {
            await dlg.ShowDialog(owner);
        }
        else
        {
            dlg.Show();
        }
        return dlg.DialogResultValue;
    }
}
