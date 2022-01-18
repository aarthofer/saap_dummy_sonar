using FHPay;
using System;
using System.Threading.Tasks;

namespace Apollo.Pay
{
    public class MockPaymentProvider : IPayment
    {
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

            if(new DateTime(creditCard.ExpirationDate.Year, creditCard.ExpirationDate.Month, 1) < DateTime.Now)
            {
                return await Task.FromResult(PaymentResult.CardExpired);
            }

            if (amount > 10000)
            {
                return await Task.FromResult(PaymentResult.InsufficientFunds);
            }

            if (string.IsNullOrEmpty(creditCard.Owner))
            {
                return await Task.FromResult(PaymentResult.InvalidName);
            }

            if(string.IsNullOrEmpty(creditCard.ValidationCode) || creditCard.ValidationCode.Length != 3)
            {
                return await Task.FromResult(PaymentResult.InvalidCardValidationCode);
            }

            return await Task.FromResult(PaymentResult.PaymentSuccessful);
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