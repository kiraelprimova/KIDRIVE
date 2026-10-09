using iTextSharp.text;
using iTextSharp.text.pdf;

namespace TestDrive_API.Services
{
    public static class PdfService
    {
        public static string CreateContract(
            string fullName,
            string phone,
            string email,
            string category,
            string model,
            int year,
            decimal price,
            DateOnly bookingDate,
            TimeOnly bookingTime,
            string payment,
            string status)
        {
            string folder =
                Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "Contracts");

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName =
                $"contract_{Guid.NewGuid()}.pdf";

            string fullPath =
                Path.Combine(folder, fileName);

            Document document =
                new Document(
                    PageSize.A4,
                    50,
                    50,
                    40,
                    40);

            PdfWriter.GetInstance(
                document,
                new FileStream(
                    fullPath,
                    FileMode.Create));

            document.Open();

            string fontPath =
                @"C:\Windows\Fonts\times.ttf";

            BaseFont baseFont =
                BaseFont.CreateFont(
                    fontPath,
                    BaseFont.IDENTITY_H,
                    BaseFont.EMBEDDED);

            Font normal =
                new Font(baseFont, 12);

            Font bold =
                new Font(
                    baseFont,
                    12,
                    Font.BOLD);

            Font title =
                new Font(
                    baseFont,
                    14,
                    Font.BOLD);

            Paragraph p;

            int contractNumber = Directory.GetFiles(folder).Length + 1;

            p = new Paragraph(
                $"Договор № {contractNumber}\n" +
                "на прохождение тест-драйва автомобиля\n" +
                "в автосалоне",
                title);

            p.Alignment = Element.ALIGN_CENTER;

            document.Add(p);

            document.Add(
                new Paragraph("\n"));

            PdfPTable topTable =
                new PdfPTable(2);

            topTable.WidthPercentage = 100;

            topTable.SetWidths(
                new float[] { 1, 1 });

            PdfPCell left =
                new PdfPCell(
                    new Phrase(
                        "г. Уфа",
                        normal));

            left.Border = Rectangle.NO_BORDER;

            PdfPCell right =
                new PdfPCell(
                    new Phrase(
                        $"\"{DateTime.Now.Day}\" {bookingDate:MMMM} {bookingDate.Year} г.",
                        normal));

            right.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            right.Border =
                Rectangle.NO_BORDER;

            topTable.AddCell(left);
            topTable.AddCell(right);

            document.Add(topTable);

            document.Add(
                new Paragraph("\n"));

            p = new Paragraph(
                "Автосалон «TestDrive», именуемый в дальнейшем «Автосалон», " +
                "в лице администратора системы, действующего на основании внутренних регламентов, " +
                $"с одной стороны, и {fullName}, именуемый(ая) в дальнейшем «Пользователь», " +
                "с другой стороны, заключили настоящий Договор о нижеследующем:",
                normal);

            p.FirstLineIndent = 25;

            p.Alignment =
                Element.ALIGN_JUSTIFIED;

            document.Add(p);

            document.Add(
                new Paragraph("\n"));

            document.Add(
                new Paragraph(
                    "1. Предмет Договора",
                    bold));

            p = new Paragraph(
                "1.1. Автосалон предоставляет Пользователю возможность прохождения тест-драйва выбранного автомобиля, оформленного через информационную систему бронирования тест-драйва.",
                normal);

            p.FirstLineIndent = 25;

            p.Alignment =
                Element.ALIGN_JUSTIFIED;

            document.Add(p);

            document.Add(
                new Paragraph(
                    "1.2. Информация о выбранном автомобиле:",
                    normal));

            document.Add(
                new Paragraph(
                    $"• категория автомобиля: {category};",
                    normal));

            document.Add(
                new Paragraph(
                    $"• модель автомобиля: {model};",
                    normal));

            document.Add(
                new Paragraph(
                    $"• год выпуска: {year};",
                    normal));

            document.Add(
                new Paragraph(
                    $"• стоимость тест-драйва за час: {price} ₽;",
                    normal));

            document.Add(
                new Paragraph(
                    $"• дата тест-драйва: {bookingDate:dd.MM.yyyy};",
                    normal));

            document.Add(
                new Paragraph(
                    $"• время тест-драйва: {bookingTime};",
                    normal));

            document.Add(
                new Paragraph(
                    $"• способ оплаты: {payment};",
                    normal));

            //document.Add(
            //    new Paragraph(
            //        $"• статус бронирования: {status};",
            //        normal));

            document.Add(
                new Paragraph(
                    $"• контактный номер пользователя: {phone};",
                    normal));

            document.Add(
                new Paragraph(
                    $"• адрес электронной почты пользователя: {email}.",
                    normal));

            document.Add(
                new Paragraph("\n"));

            document.Add(
                new Paragraph(
                    "1.3. Автомобиль предоставляется Пользователю исключительно для прохождения тест-драйва в согласованное время.",
                    normal));

            document.Add(
                new Paragraph(
                    "1.4. Автосалон гарантирует исправное техническое состояние автомобиля на момент начала тест-драйва.",
                    normal));

            document.Add(
                new Paragraph(
                    "1.5. Пользователь подтверждает достоверность данных, указанных при регистрации и бронировании тест-драйва.",
                    normal));

            document.Add(
                new Paragraph(
                    "1.6. Продолжительность тест-драйва составляет 4 часа.",
                    normal));

            document.Add(
                new Paragraph(
                    "1.7. Тест-драйв проводится по маршруту, согласованному с представителем Автосалона.",
                    normal));

            document.Add(
                new Paragraph(
                    "1.8. Пользователь гарантирует наличие водительского удостоверения соответствующей категории.",
                    normal));

            document.Add(
                new Paragraph("\n"));

            document.Add(
                new Paragraph(
                    "2. Права и обязанности Сторон",
                    bold));

            document.Add(
                new Paragraph(
                    "2.1. Автосалон обязан:",
                    normal));

            document.Add(
                new Paragraph(
                    "2.1.1. Предоставить Пользователю автомобиль в исправном состоянии в назначенное время.",
                    normal));

            document.Add(
                new Paragraph(
                    "2.1.2. Предоставить информацию о правилах прохождения тест-драйва.",
                    normal));

            document.Add(
                new Paragraph(
                    "2.1.3. Обеспечить сопровождение тест-драйва представителем Автосалона при необходимости.",
                    normal));

            document.Add(
                new Paragraph("\n"));

            document.Add(
                new Paragraph(
                    "2.2. Пользователь обязан:",
                    normal));

            document.Add(
                new Paragraph(
                    "2.2.1. Соблюдать правила эксплуатации автомобиля во время тест-драйва.",
                    normal));

            document.Add(
                new Paragraph(
                    "2.2.2. Прибыть в назначенное время тест-драйва и иметь при себе водительское удостоверение.",
                    normal));

            document.Add(
                new Paragraph("\n\n"));

            PdfPTable signTable =
                new PdfPTable(2);

            signTable.WidthPercentage = 100;

            signTable.SetWidths(
                new float[] { 1, 1 });

            PdfPCell sign1 =
                new PdfPCell(
                    new Phrase(
                        "Автосалон:\n\n_____________________",
                        normal));

            sign1.Border =
                Rectangle.NO_BORDER;

            PdfPCell sign2 =
                new PdfPCell(
                    new Phrase(
                        $"Пользователь:\n\n{fullName}\n\n_____________________",
                        normal));

            sign2.Border =
                Rectangle.NO_BORDER;

            sign2.HorizontalAlignment =
                Element.ALIGN_RIGHT;

            signTable.AddCell(sign1);
            signTable.AddCell(sign2);

            document.Add(signTable);

            document.Close();

            return fullPath;
        }
    }
}