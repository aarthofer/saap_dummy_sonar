using Apollo.Core.Domain;
using System;

namespace Apollo.Print
{
    public interface IPrint
    {
        byte[] PrintTicket(TicketInformation ticketInformation);
        public string GetQRCodeBase64(string qrCodeString);
    }
}
