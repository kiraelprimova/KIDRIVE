using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.IO;

namespace TestDriveAvalonia.Classes;

public static class ImageHelper
{
    public static Bitmap? LoadBitmap(string? path)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return LoadPlaceholder();
            }

            // Убираем возможные префиксы
            string clean = path.Replace("/Resources/", "").Replace("Resources/", "").Trim();

            // Пробуем из ресурсов Avalonia (avares)
            var uri = new Uri($"avares://TestDriveAvalonia/Resources/{clean}");
            if (AssetLoader.Exists(uri))
            {
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }

            // Пробуем из папки приложения
            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", clean);
            if (File.Exists(localPath))
            {
                return new Bitmap(localPath);
            }

            return LoadPlaceholder();
        }
        catch
        {
            return LoadPlaceholder();
        }
    }

    private static Bitmap? LoadPlaceholder()
    {
        try
        {
            var uri = new Uri("avares://TestDriveAvalonia/Resources/заглушка.png");
            if (AssetLoader.Exists(uri))
            {
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }

            string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "заглушка.png");
            if (File.Exists(localPath))
            {
                return new Bitmap(localPath);
            }
        }
        catch { }

        return null;
    }
}
