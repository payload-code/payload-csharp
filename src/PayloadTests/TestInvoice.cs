using NUnit.Framework;
using System;

namespace Payload.Tests
{
    public class TestInvoice
    {
        dynamic invoice;
        dynamic processing_account;
        dynamic customer_account;

        [SetUp]
        public void Setup()
        {

            PayloadTestSetup.initAPI();
            this.processing_account = Fixtures.processing_account();
            this.customer_account = Fixtures.customer_account();

            this.invoice = pl.Invoice.create(new
            {
                type = "bill",
                processing_id = processing_account.id,
                due_date = "2019-05-01",
                customer_id = customer_account.id,
                items = new[] { new pl.ChargeItem( new {amount=29.99 }),
             }
            });

        }

        [Test]
        public void test_create_invoice()
        {
            Assert.AreEqual(typeof(pl.Invoice), this.invoice.GetType());
            Assert.True(this.invoice.due_date == "2019-05-01");
            Assert.True(this.invoice.status == "unpaid");
        }

        [Test]
        public void test_pay_invoice()
        {

            var cust = pl.Customer.create(new { email = "test2@gmail.com", name = "Test Account" });

            var card_on_file = pl.Card.create(new
            {
                account_id = cust.id,
                card_number = "4242 4242 4242 4242",
                default_payment_method = true,
                expiry = "12/25",
                card_code = "123",
                billing_address = new {
                    postal_code = "12345"
                }
            });

            var invoice = pl.Invoice.create(new
            {
                due_date = new DateTime(2019, 5, 1),
                items = new[]{
                new pl.ChargeItem(new{amount=100, description="membership"})
            },
                customer_id = cust.id
            });

            Assert.AreEqual(typeof(pl.Invoice), invoice.GetType());
            Assert.AreEqual(cust.id, invoice.customer_id);

            if (invoice.status != "paid")
            {
                var payment = pl.Payment.create(new
                {
                    amount = invoice.amount_due,
                    customer_id = invoice.customer_id,
                    allocations = new[]{
                    new pl.PaymentItem(new{
                        invoice_id=invoice.id
                    })
                }
                });

                var invoice_get = pl.Invoice.get(invoice.id);

                Assert.AreEqual("paid", invoice_get.status);

            }
        }

        [Test]
        public void test_invoice_one()
        {
            Assert.NotNull(pl.Invoice.filter_by(new { type = this.invoice.type }).one());
            Assert.AreEqual(typeof(pl.Invoice), pl.Invoice.filter_by(new{ type = this.invoice.type }).one().GetType());
        }
    }
}

