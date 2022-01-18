
using Apollo.Core.Domain;
using Apollo.Print;
using System;
using System.IO;
using Xunit;

namespace Apollo.BL.Test
{
    public class PrintTest
    {
        [Fact]
        public async System.Threading.Tasks.Task TestPrintAsync()
        {
            IPrint print = new ApolloPrint();
            byte[] res = print.PrintTicket(new Core.Domain.TicketInformation
            {
                MovieTitle = "TestMovieTestMovieTestMovie",
                Cinema = "TestCinema",
                Row = "5",
                Seat = "2",
                StartTime = new DateTime(2020, 8, 15, 20, 15, 0),
                ReservatoinId = 12345
            }); ;
            Assert.NotEmpty(res);
            // await File.WriteAllBytesAsync(@"c:\tmp\test.pdf", res);
        }

        [Fact]
        public async System.Threading.Tasks.Task TestQRCode()
        {
            IPrint print = new ApolloPrint();
            TicketInformation ti = new Core.Domain.TicketInformation
            {
                MovieTitle = "TestMovieTestMovieTestMovie",
                Cinema = "TestCinema",
                Row = "5",
                Seat = "2",
                StartTime = new DateTime(2020, 8, 15, 20, 15, 0),
                ReservatoinId = 12345
            };

            Assert.Equal("TestMovieTestMovieTestMovie|TestCinema|5|2|12345", ti.GetQRCodeString());
            string base64 = print.GetQRCodeBase64(ti.GetQRCodeString());
            Assert.NotNull(base64);
            //await File.WriteAllBytesAsync(@"c:\tmp\test.pdf", res);
        }
    }
}
