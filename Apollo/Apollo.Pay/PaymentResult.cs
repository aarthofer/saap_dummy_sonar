using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Pay
{
    public enum PaymentResult
    {
        CardExpired, CardReportedLost, InsufficientFunds, InvalidCardValidationCode, InvalidName, PaymentSuccessful, InvalidPaymentResult
    }
}
