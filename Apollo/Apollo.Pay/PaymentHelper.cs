using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Pay
{
    public abstract class PaymentHelper
    {
        public static PaymentResult ConvertPaymentResult(FHPay.PaymentResult paymentResult)
        {
            switch (paymentResult)
            {
                case FHPay.PaymentResult.CardExpired: return PaymentResult.CardExpired;
                case FHPay.PaymentResult.CardReportedLost: return PaymentResult.CardReportedLost;
                case FHPay.PaymentResult.InsufficientFunds: return PaymentResult.InsufficientFunds;
                case FHPay.PaymentResult.InvalidCardValidationCode: return PaymentResult.InvalidCardValidationCode;
                case FHPay.PaymentResult.InvalidName: return PaymentResult.InvalidName;
                case FHPay.PaymentResult.PaymentSuccessful: return PaymentResult.PaymentSuccessful;
                default: throw new ArgumentOutOfRangeException($"Unknown payment result: {paymentResult}");
            }
        }

        public static FHPay.CreditCard ConvertCreditCard(CreditCard creditCard)
        {
            return new FHPay.CreditCard(creditCard.Owner
                , new FHPay.CreditCardNumber(creditCard.Number)
                , new FHPay.ExpirationDate(creditCard.ExpirationDate.Month, creditCard.ExpirationDate.Year)
                , new FHPay.CardValidationCode(creditCard.ValidationCode));
        }
    }
}
