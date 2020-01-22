using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Payload.Tests
{
    public class TestTransaction
    {
        dynamic card_payment;
        dynamic bank_payment;
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
            this.card_payment = Fixtures.card_payment();
            this.bank_payment = Fixtures.bank_payment();
        }

        [Test]
        public void test_transaction_ledger_empty()
        {
            var transaction = pl.Transaction.select("*", "ledger").get(this.card_payment.id);

            Assert.IsEmpty(transaction.ledger);
        }

        [Test]
        public void test_unified_payout_batching()
        {
            pl.Refund.create(new
            {
                amount = 10,
                processing_id = this.processing_account.id,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242" })
            }

                       );

            var transactions = pl.Transaction.select("*", "ledger")
                .filter_by(new { type = "refund", processing_id = this.processing_account.id })
                .all();


            Assert.True(transactions.Count == 1);
            Assert.True(transactions[0].processing_id == this.processing_account.id);
        }

        [Test]
        public void test_get_transaction()
        {
            var transaction = pl.Transaction.get(card_payment.id);

            Assert.True(transaction.id == card_payment.id);
        }

        [Test]
        public void test_transactions_one()
        {
            Assert.NotNull(pl.Transaction.filter_by(new { type = this.card_payment.type }).one());
            Assert.AreEqual(typeof(pl.Payment), pl.Payment.filter_by(new { amount = this.card_payment.amount }).one().GetType());
        }

        [Test]
        public void test_risk_flag()
        {
            Assert.True(this.card_payment.risk_flag == "allowed");
        }

        [Test]
        public void test_update_processed()
        {
            this.card_payment.update(new { status = "voided" });
            Assert.True(card_payment.status == "voided");

        }

        [Test]
        public void test_transactions_not_found()
        {
            Assert.Throws<pl.NotFound>(
              () => pl.Transaction.get("invalid"));
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
                processing_id = this.processing_account.id,
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
    }
}
