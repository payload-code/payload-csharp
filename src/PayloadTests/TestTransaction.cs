using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payload.Tests
{
    public class TestTransaction
    {
        dynamic card_payment;
        dynamic bank_payment;
        dynamic processing_account;

        [SetUp]
        public void Setup()
        {
            PayloadTestSetup.initAPI();
            this.processing_account = Fixtures.processing_account();
        }

        [Test]
        public void test_transaction_ledger_empty()
        {
            this.card_payment = Fixtures.card_payment();

            dynamic transaction = pl.Transaction.Select("*", "ledger").Get(this.card_payment.id);

            Assert.IsEmpty(transaction.ledger);
        }

        [Test]
        public void test_unified_payout_batching()
        {
            pl.Refund.Create(new
            {
                amount = 10,
                processing_id = this.processing_account.id,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242", expiry = "12/25" })
            });

            dynamic transactions = pl.Transaction.Select("*", "ledger")
                .FilterBy(new { type = "refund", processing_id = this.processing_account.id })
                .All();


            Assert.True(transactions.Count == 1);
            Assert.True(transactions[0].processing_id == this.processing_account.id);
        }

        [Test]
        public void test_get_transaction()
        {
            this.card_payment = Fixtures.card_payment();

            var transaction = pl.Transaction.Get(card_payment.id);

            Assert.True(transaction.id == card_payment.id);
        }

        [Test]
        public void test_transactions_one()
        {
            this.card_payment = Fixtures.card_payment();

            Assert.NotNull(pl.Transaction.FilterBy(new { type = this.card_payment.type }).One());
            Assert.AreEqual(typeof(pl.Payment), pl.Payment.FilterBy(new { amount = this.card_payment.amount }).One().GetType());
        }

        [Test]
        public void test_risk_flag()
        {
            this.card_payment = Fixtures.card_payment();

            Assert.True(this.card_payment.risk_flag == "allowed");
        }

        [Test]
        public void test_update_processed()
        {
            this.card_payment = Fixtures.card_payment();

            this.card_payment.Update(new { status = "voided" });
            Assert.True(card_payment.status == "voided");

        }

        [Test]
        public void test_transactions_not_found()
        {
            Assert.Throws<pl.NotFound>(
              () => pl.Transaction.Get("invalid"));
        }


        [Test]
        public void test_payment_filters_1()
        {
            String rand_description = Fixtures.RandomString(10);
            Random random = new Random();
            int randomNumber = random.Next(1, 100);

            dynamic card_payment = pl.Payment.Create(new
            {
                amount = randomNumber,
                description = rand_description,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242", expiry = "12/25" })
            });

            List<pl.Payment> payments = pl.Payment.FilterBy(new object[] {
                pl.Attr.amount.gt(0),
                pl.Attr.amount.lt(200),
                pl.Attr.description.contains(rand_description),
                pl.Attr.created_at.gt(new DateTime(2019, 2, 1))
            }).All();

            Assert.True(payments.Count == 1);
            Assert.True(payments.ElementAt(0).Data.id == card_payment.id);
        }

        [Test]
        public void test_void_card_payment()
        {
            this.card_payment = Fixtures.card_payment();

            ((pl.Payment)this.card_payment).Update(new { status = "voided" });

            Assert.AreEqual("voided", this.card_payment.status);
        }

        [Test]
        public void test_void_bank_payment()
        {
            this.bank_payment = Fixtures.bank_payment();

            ((pl.Payment)this.bank_payment).Update(new { status = "voided" });
            Assert.AreEqual("voided", this.bank_payment.status);

        }

        [Test]
        public void test_refund_card_payment()
        {
            this.card_payment = Fixtures.card_payment();

            dynamic refund = pl.Refund.Create(new
            {
                amount = this.card_payment.amount,
                ledger = new[] {
                    new pl.Ledger(new {
                        assoc_transaction_id=this.card_payment.id
                    })
                }
            });

            Assert.True(refund.type == "refund");
            Assert.True(refund.amount == this.card_payment.amount);
            Assert.True(refund.status_code == "approved");
        }

        [Test]
        public void test_partial_refund_card_payment()
        {
            this.card_payment = Fixtures.card_payment();

            dynamic refund = pl.Refund.Create(new
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
            dynamic refund = pl.Refund.Create(new
            {
                amount = 10.0,
                processing_id = this.processing_account.id,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242", expiry = "12/25" })
            });

            Assert.True(refund.type == "refund");
            Assert.True(refund.amount == 10);
            Assert.True(refund.status_code == "approved");

        }

        [Test]
        public void test_refund_bank_payment()
        {
            this.bank_payment = Fixtures.bank_payment();

            dynamic refund = pl.Refund.Create(new
            {
                amount = this.bank_payment.amount,
                ledger = new[] {
                    new pl.Ledger(new {
                        assoc_transaction_id=this.bank_payment.id
                    })
                }
            });

            Assert.True(refund.type == "refund");
            Assert.True(refund.amount == this.bank_payment.amount);
            Assert.True(refund.status_code == "approved");
        }

        [Test]
        public void test_partial_refund_bank_payment()
        {
            this.bank_payment = Fixtures.bank_payment();

            dynamic refund = pl.Refund.Create(new
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
            dynamic payment = pl.Payment.Select("*", "fee", "conv_fee").Create(new
            {
                amount = 100,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242", expiry = "12/25" })
            });

            Assert.NotNull(payment.fee);
            Assert.NotNull(payment.conv_fee);
        }
    }
}
