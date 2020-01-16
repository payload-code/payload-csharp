using NUnit.Framework;
using System;
using System.Linq;
using System.Collections.Generic;


namespace Payload.Tests
{
    public class TestPaymentMethod
    {

        dynamic bank_payment;
        dynamic card_payment;
        dynamic proccessing_account;

        [OneTimeSetUp]
        public void ClassInit()
        {
            pl.api_key = "your_secret_key_3bfn0Ilzojfd5M76hFOxT";
        }

        [SetUp]
        public void Setup()
        {
            this.bank_payment = Fixtures.bank_payment();
            this.card_payment = Fixtures.card_payment();
            this.proccessing_account = Fixtures.processing_account();
        }



        [Test]
        public void test_create_payment_card()
        {
            Assert.AreEqual(typeof(pl.Payment), this.card_payment.GetType());
            Assert.AreEqual("processed", this.card_payment.status);
        }

        [Test]
        public void test_create_payment_bank()
        {
            Assert.AreEqual(typeof(pl.Payment), this.bank_payment.GetType());
            Assert.AreEqual("processed", this.card_payment.status);

        }

        [Test]
        public void test_payment_filters_1()
        {
            String rand_description = Fixtures.RandomString(10);

            var card_payment = pl.Payment.create(new
            {
                amount = 100.0,
                description = rand_description,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242" })
            });

            List<dynamic> payments = pl.Payment.filter_by(
                pl.attr.amount.gt(99),
                pl.attr.amount.lt(200),
                pl.attr.description.contains(rand_description),
                pl.attr.created_at.gt(new DateTime(2019, 2, 1))
            ).all();

            Assert.True(payments.Count == 1);
            Assert.True(payments.ElementAt(0).id == card_payment.id);

        }

        [Test]
        public void test_void_card_payment()
        {
            this.card_payment.update(new { status = "voided" });

            Assert.AreEqual("voided", this.card_payment.status);
        }

        [Test]
        public void test_void_bank_payment()
        {

            this.bank_payment.update(new { status = "voided" });
            Assert.AreEqual("voided", this.bank_payment.status);

        }

        [Test]
        public void test_refund_card_payment()
        {
            var refund = pl.Refund.create(new
            {
                amount = 100.0,
                ledger = new[] {
                    new pl.Ledger(new {
                        assoc_transaction_id=this.card_payment.id
                    })
                }
            });

            Assert.True(refund.type == "refund");
            Assert.True(refund.amount == 100);
            Assert.True(refund.status_code == "approved");
        }

        [Test]
        public void test_partial_refund_card_payment()
        {
            var refund = pl.Refund.create(new
            {
                amount = 10.0,
                ledger = new[] {
                    new pl.Ledger(new {
                        assoc_transaction_id=this.card_payment.id
                    })
                }
            });

            Assert.True(refund.type == "refund");
            Assert.True(refund.amount == 10);
            Assert.True(refund.status_code == "approved");
        }

        [Test]
        public void test_blind_refund_card_payment()
        {
            var refund = pl.Refund.create(new
            {
                amount = 10.0,
                processing_id = this.proccessing_account.id,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242" })
            });

            Assert.True(refund.type == "refund");
            Assert.True(refund.amount == 10);
            Assert.True(refund.status_code == "approved");

        }

        [Test]
        public void test_refund_bank_payment()
        {
            var refund = pl.Refund.create(new
            {
                amount = 100.0,
                ledger = new[] {
                    new pl.Ledger(new {
                        assoc_transaction_id=this.bank_payment.id
                    })
                }
            });

            Assert.True(refund.type == "refund");
            Assert.True(refund.amount == 100);
            Assert.True(refund.status_code == "approved");
        }

        [Test]
        public void test_partial_refund_bank_payment()
        {
            var refund = pl.Refund.create(new
            {
                amount = 10.0,
                ledger = new[] {
                    new pl.Ledger(new {
                        assoc_transaction_id=this.bank_payment.id
                    })
                }
            });

            Assert.True(refund.type == "refund");
            Assert.True(refund.amount == 10);
            Assert.True(refund.status_code == "approved");
        }


        [Test]
        public void test_convenience_fee()
        {
            var payment = pl.Payment.select("*", "conv_fee").create(new
            {
                amount = 100,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242" })
            });

            Assert.NotNull(payment.fee);
            Assert.NotNull(payment.conv_fee);
        }

        [Test]
        public void test_invalid_payment_method_type_invalid_attributes()
        {
            Assert.Throws<pl.InvalidAttributes>(
               () => pl.Transaction.create(new { type = "invalid", card_number = "4242 4242 4242 4242" }));
        }


    }
}



