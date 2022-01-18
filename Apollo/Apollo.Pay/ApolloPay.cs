using System;
using System.Threading.Tasks;
using FHPay;

namespace Apollo.Pay
{
    public class ApolloPay : IPayment
    {
        private PaymentApi api;
        internal ApolloPay(string apiKey)
        {
            this.api = new PaymentApi(apiKey);
        }

        public async Task<PaymentResult> CreateTransactionAsync(decimal amount, CreditCard creditCard, string description)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

            if (creditCard is null)
            {
                throw new ArgumentNullException(nameof(creditCard));
            }

            try
            {
                return PaymentHelper.ConvertPaymentResult(await api.CreateTransactionAsync(amount, PaymentHelper.ConvertCreditCard(creditCard), description));
            }
            catch (NetworkConnectionException)
            {
                return PaymentResult.InvalidPaymentResult;
            }
        }

        public bool ValidateCreditCardNumber(CreditCard creditCard)
        {
            try
            {
                // Converter throws exception if Card-Number is not valid
                FHPay.CreditCard card = PaymentHelper.ConvertCreditCard(creditCard);
                return card != null ? true : false;
            }
            catch
            {
                return false;
            }
        }
    }
}
