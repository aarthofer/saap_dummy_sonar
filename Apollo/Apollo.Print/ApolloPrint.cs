using Apollo.Core.Domain;
using iText.Html2pdf;
using iText.Html2pdf.Attach;
using iText.Html2pdf.Attach.Impl;
using iText.Html2pdf.Attach.Impl.Tags;
using iText.Html2pdf.Html;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.StyledXmlParser.Css.Media;
using iText.StyledXmlParser.Node;
using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace Apollo.Print
{
    public class ApolloPrint : IPrint
    {
        private readonly string TEMPLATE_HTML_FILE = "pdftemplate/tickettemplate.html";

        // https://archive.codeplex.com/?p=htmlrenderer
        public byte[] PrintTicket(TicketInformation ticketInformation)
        {
            byte[] res = null;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (MemoryStream ms = new MemoryStream())
            {
                PdfDocument doc = new PdfDocument(new PdfWriter(ms));
                doc.SetDefaultPageSize(PageSize.A6.Rotate());

                var props = new ConverterProperties().SetBaseUri(Directory.GetCurrentDirectory());
                HtmlConverter.ConvertToPdf(GetHtmlTemplate(ticketInformation), doc, props);
                res = ms.ToArray();
            }
            return res;
        }

        private string GetHtmlTemplate(TicketInformation ticketInformation)
        {
            if (!new FileInfo(TEMPLATE_HTML_FILE).Exists)
            {
                throw new ApplicationException("PDF Template-File not found");
            }

            string template = File.ReadAllText(TEMPLATE_HTML_FILE);
            template = template.Replace("{{movietitle}}", ticketInformation.MovieTitle)
            .Replace("{{ciname}}", ticketInformation.Cinema)
            .Replace("{{row}}", ticketInformation.Row)
            .Replace("{{seat}}", ticketInformation.Seat)
            .Replace("{{date}}", ticketInformation.StartTime.ToString(System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortDatePattern))
            .Replace("{{begin}}", ticketInformation.StartTime.ToString(System.Threading.Thread.CurrentThread.CurrentCulture.DateTimeFormat.ShortTimePattern))
            .Replace("{{qrcode64}}", GetQRCodeBase64(ticketInformation.GetQRCodeString()));
            return template;
        }

        // https://www.c-sharpcorner.com/article/generate-qr-code-in-net-core-using-bitmap/
        public string GetQRCodeBase64(string qrCodeString)
        {
            QRCodeGenerator _qrCode = new QRCodeGenerator();
            QRCodeData _qrCodeData = _qrCode.CreateQrCode(qrCodeString, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(_qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);
            using (MemoryStream stream = new MemoryStream())
            {
                qrCodeImage.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return Convert.ToBase64String(stream.ToArray());
            }
        }
    }
}
