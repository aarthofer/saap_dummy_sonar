using Apollo.Pay;
using FHPay;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Apollo.BL.Test
{
    public class ApolloPayTest
    {
        [Fact]
        public void TestPaymentFactory_FHPay()
        {
            Assert.IsType<ApolloPay>(PaymentFactory.GetApolloPay("5B77199A-C9D1-48AB-B93A-6B2750BB33E4", "Payment.FHPay"));
            Assert.Throws<ArgumentException>(() => PaymentFactory.GetApolloPay("", "Payment.FHPay"));
            Assert.Throws<InvalidApiKeyException>(() => PaymentFactory.GetApolloPay("invalid key", "Payment.FHPay"));
        }

        [Fact]
        public void TestPaymentFactory_MockPay()
        {
            Assert.IsType<MockPaymentProvider>(PaymentFactory.GetApolloPay("paymentkey", "Payment.Mock"));
            Assert.Throws<ArgumentException>(() => PaymentFactory.GetApolloPay("", "Payment.Mock"));
        }

        [Fact]
        public void TestPaymentFactory_Invalid()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => PaymentFactory.GetApolloPay("paymentkey", "Payment.NotAvailable"));
            Assert.Throws<ArgumentException>(() => PaymentFactory.GetApolloPay("", "Payment.NotAvailable"));
        }

        [Theory]
        [InlineData("Payment.Mock")]
        [InlineData("Payment.FHPay")]
        public void TestPaymentValidCreditCard(string paymentprovider)
        {
            IPayment payment = PaymentFactory.GetApolloPay("5B77199A-C9D1-48AB-B93A-6B2750BB33E4", paymentprovider);
            Assert.True(payment.ValidateCreditCardNumber(new Pay.CreditCard()
            {
                ExpirationDate = new Pay.ExpirationDate(12, 2099),
                Number = "4111111111111111",
                Owner = "UnitTest",
                ValidationCode = "123"
            }));

            Assert.False(payment.ValidateCreditCardNumber(new Pay.CreditCard()
            {
                ExpirationDate = new Pay.ExpirationDate(12, 2099),
                Number = "123456789",
                Owner = "UnitTest",
                ValidationCode = "123"
            }));

            Assert.False(payment.ValidateCreditCardNumber(new Pay.CreditCard()
            {
                ExpirationDate = new Pay.ExpirationDate(12, 23),
                Number = "4111111111111111",
                Owner = "UnitTest",
                ValidationCode = "123"
            }));

            Assert.False(payment.ValidateCreditCardNumber(new Pay.CreditCard()
            {
                ExpirationDate = new Pay.ExpirationDate(12, 23),
                Number = "4111111111111111",
                Owner = "UnitTest",
                ValidationCode = "123456"
            }));
        }

        [Fact]
        public void TestPaymentConvertCreditCard()
        {
            Pay.CreditCard aC = new Pay.CreditCard()
            {
                ExpirationDate = new Pay.ExpirationDate(12, 2099),
                Number = "4111111111111111",
                Owner = "UnitTest",
                ValidationCode = "123"
            };

            FHPay.CreditCard fC = PaymentHelper.ConvertCreditCard(aC);
            Assert.Equal(aC.ExpirationDate.Month, fC.ExpirationDate.Month);
            Assert.Equal(aC.ExpirationDate.Year, fC.ExpirationDate.Year);
            Assert.Equal(aC.Number, fC.Number.Value);
            Assert.Equal(aC.Owner, fC.Owner);
            Assert.Equal(aC.ValidationCode, fC.ValidationCode.Value);
        }

        [Theory]
        [InlineData(Pay.PaymentResult.CardExpired, FHPay.PaymentResult.CardExpired)]
        [InlineData(Pay.PaymentResult.CardReportedLost, FHPay.PaymentResult.CardReportedLost)]
        [InlineData(Pay.PaymentResult.InsufficientFunds, FHPay.PaymentResult.InsufficientFunds)]
        [InlineData(Pay.PaymentResult.InvalidCardValidationCode, FHPay.PaymentResult.InvalidCardValidationCode)]
        [InlineData(Pay.PaymentResult.InvalidName, FHPay.PaymentResult.InvalidName)]
        [InlineData(Pay.PaymentResult.PaymentSuccessful, FHPay.PaymentResult.PaymentSuccessful)]
        public void TestPaymentCOnvertPaymentResult(Pay.PaymentResult apollo, FHPay.PaymentResult fhpay)
        {
            Assert.Equal(apollo, PaymentHelper.ConvertPaymentResult(fhpay));
        }
    }
}
