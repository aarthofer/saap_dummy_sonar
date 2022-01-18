using Apollo.Pay;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apollo.Pay
{
    public interface IPayment
    {
        Task<PaymentResult> CreateTransactionAsync(decimal amount, CreditCard creditCard, string description);
        bool ValidateCreditCardNumber(CreditCard creditCard);
    }
}
