using NUnit.Framework;

namespace Payload.Tests
{
    public class TestTransaction
    {
        dynamic card_payment;
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
        public void test_get_transactions()
        {
            var payments = pl.Transaction.filter_by(new { status = "processed", type = "payment" }).all();
            Assert.True(payments.Count > 0);
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


    }
}





