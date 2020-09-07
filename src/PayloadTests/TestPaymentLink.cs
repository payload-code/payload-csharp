using NUnit.Framework;
using System;

namespace Payload.Tests
{
    public class PaymentLink
    {
        dynamic processing_account;
        dynamic payment_link;

        [SetUp]
        public void Setup()
        {
            PayloadTestSetup.initAPI();
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
            dynamic lnk = pl.PaymentLink.filter_by(new { type = this.payment_link.type }).one();
            Assert.NotNull(lnk);
            Assert.AreEqual(typeof(pl.PaymentLink), lnk.GetType());
        }


    }
}





