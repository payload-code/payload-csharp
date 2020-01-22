using NUnit.Framework;

namespace Payload.Tests
{
    public class PaymentLink
    {
        dynamic processing_account;
        dynamic payment_link;

        [OneTimeSetUp]
        public void ClassInit()
        {
            pl.api_key = "your_secret_key_3bfn0Ilzojfd5M76hFOxT";
        }

        [SetUp]
        public void Setup()
        {
            this.processing_account = Fixtures.processing_account();
            this.payment_link = pl.PaymentLink.create(new
            {
                type = "one_time",
                description = "Payment Request",
                amount = 10.00,
                processing_id = this.processing_account.id
            }
               );
        }

        [Test]
        public void test_create_payment_link()
        {
            Assert.True(payment_link.processing_id == this.processing_account.id);
        }

        [Test]
        public void test_payment_link_one()
        {
            Assert.NotNull(pl.PaymentLink.filter_by(new { type = this.payment_link.type }).one());
            Assert.AreEqual(typeof(pl.PaymentLink), pl.PaymentLink.filter_by(new { type = this.payment_link.type }).one().GetType());
        }


    }
}





