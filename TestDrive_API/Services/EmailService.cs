using System.Net;
using System.Net.Mail;

namespace TestDrive_API.Services
{
    public static class EmailService
    {
        public static void SendEmail(
            string email,
            string pdfPath)
        {
            MailMessage message =
                new MailMessage();

            message.From =
                new MailAddress(
                    "fatkhlislamovad@mail.ru");

            message.To.Add(email);

            message.Subject =
                "Договор на бронирование автомобиля";

            message.Body =
                "Ваш договор находится во вложении.";

            Attachment attachment =
                new Attachment(pdfPath);

            message.Attachments.Add(attachment);

            SmtpClient smtp =
                new SmtpClient(
                    "smtp.mail.ru",
                    587);

            smtp.Credentials =
                new NetworkCredential(
                    "fatkhlislamovad@mail.ru",
                    "eQiWPtmfqffaDTE6VNQ4");

            smtp.EnableSsl = true;

            smtp.Send(message);
        }
    }
}