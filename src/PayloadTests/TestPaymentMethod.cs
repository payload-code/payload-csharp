using NUnit.Framework;


namespace Payload.Tests
{
    public class TestPaymentMethod
    {

        dynamic bank_payment_method;
        dynamic card_payment_method;


        [SetUp]
        public void Setup()
        {
            PayloadTestSetup.initAPI();
            this.card_payment_method = pl.Card.create(new {
                account_holder="John Smith",
                card_number="4242 4242 4242 4242",
                expiry="05/22",
                card_code="123"
            });

            this.bank_payment_method = pl.BankAccount.create(new
            {
                account_number = "123456789",
                routing_number = "036001808",
                account_type = "checking"
            });
        }

        [Test]
        public void test_create_payment_method_card()
        {
            Assert.AreEqual(typeof(pl.Card), this.card_payment_method.GetType());
            Assert.AreEqual("xxxxxxxxxxxx4242", this.card_payment_method.card_number);
        }

        [Test]
        public void test_create_payment_method_bank()
        {
            Assert.AreEqual(typeof(pl.BankAccount), this.bank_payment_method.GetType());
            Assert.AreEqual("checking", this.bank_payment_method.account_type);

        }

        [Test]
        public void test_invalid_payment_method_type_invalid_attributes()
        {
            Assert.Throws<pl.InvalidAttributes>(
               () => pl.Transaction.create(new { type = "invalid", card_number = "4242 4242 4242 4242" }));
        }


        [Test]
        public void test_card_payment_method_one()
        {
            Assert.NotNull(pl.PaymentMethod.filter_by(new { type = this.card_payment_method.type }).one());
            Assert.AreEqual(typeof(pl.Card), pl.PaymentMethod.filter_by(new { type = this.card_payment_method.type }).one().GetType());

        }

        [Test]
        public void test_bank_payment_method_one()
        {
            Assert.NotNull(pl.PaymentMethod.filter_by(new { type = this.bank_payment_method.type }).one());
            Assert.AreEqual(typeof(pl.BankAccount), pl.PaymentMethod.filter_by(new { type = this.bank_payment_method.type }).one().GetType());

        }
    }
}
