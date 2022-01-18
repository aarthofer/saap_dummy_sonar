using Apollo.Core.Domain;
using Apollo.Print;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Drawing.Printing;
using System.IO;
using System.Printing;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Terminal.Services
{
    public class PrintService
    {
        private const string PAPER_NAME = "A4";
        private const string PRINTER_NAME = "EPSON13D6BD (Epson Stylus SX440)";
        public IServiceProvider ServiceProvider { get; }

        public PrintService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }


        public async Task PrintTicket(TicketInformation ticket)
        {
            var printer = ServiceProvider.GetService<IPrint>();
            byte[] ticketBytes = printer.PrintTicket(ticket);
            DirectoryInfo di = new DirectoryInfo("tickets");
            if (!di.Exists) { di.Create(); }

            using (FileStream fs = new FileStream("tickets/" + Guid.NewGuid() + ".pdf", FileMode.CreateNew, FileAccess.ReadWrite))
            {
                fs.Write(ticketBytes, 0, ticketBytes.Length);
            }
            //var printer = ServiceProvider.GetService<IPrint>();

            //byte[] ticketBytes = printer.PrintTicket(ticket);

            //var printQueue = LocalPrintServer.GetDefaultPrintQueue();

            //using (var job = printQueue.AddJob())
            //using (var stream = job.JobStream)
            //{
            //    stream.Write(ticketBytes, 0, ticketBytes.Length);
            //}
        }
    }
}
