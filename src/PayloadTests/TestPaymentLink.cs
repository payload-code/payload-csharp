using NUnit.Framework;

namespace Payload.Tests
{
    public class PaymentLink
    {
        dynamic processing_account;

        [OneTimeSetUp]
        public void ClassInit()
        {
            pl.api_key = "your_secret_key_3bfn0Ilzojfd5M76hFOxT";
        }

        [SetUp]
        public void Setup()
        {
            this.processing_account = Fixtures.processing_account();
        }

        [Test]
        public void test_create_payment_link()
        {
            var payment_link = pl.PaymentLink.create(new
            {
                type = "one_time",
                description = "Payment Request",
                amount = 10.00,
                processing_id = this.processing_account.id
            }
               );

            Assert.True(payment_link.processing_id == this.processing_account.id);
        }


    }
}





