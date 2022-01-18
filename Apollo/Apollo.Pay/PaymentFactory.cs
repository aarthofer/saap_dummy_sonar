using System;
using System.Collections.Generic;
using System.Text;

namespace Apollo.Pay
{
    public class PaymentFactory
    {
        private const string FHPAY_PROVIDER = "Payment.FHPay";
        private const string MOCK_PROVIDER = "Payment.Mock";
        public static IPayment GetApolloPay(string apiKey, string paymentProvider)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new ArgumentException($"'{nameof(apiKey)}' cannot be null or empty", nameof(apiKey));
            }

            switch (paymentProvider)
            {
                case FHPAY_PROVIDER: return new ApolloPay(apiKey);
                case MOCK_PROVIDER: return new MockPaymentProvider();
                default: throw new ArgumentOutOfRangeException("Invalid Provider");
            }
        }
    }
}
