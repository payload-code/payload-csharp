using NUnit.Framework;
using NUnit.Framework.Legacy;


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
            this.card_payment_method = pl.Card.Create(new
            {
                account_holder = "John Smith",
                card_number = "4242 4242 4242 4242",
                expiry = "05/40",
                card_code = "123",
                billing_address = new
                {
                    postal_code = "12345"
                }
            });

            this.bank_payment_method = pl.BankAccount.Create(new
            {
                account_number = "123456789",
                routing_number = "036001808",
                account_type = "checking"
            });
        }

        [Test]
        public void test_create_payment_method_card()
        {
            ClassicAssert.AreEqual(typeof(pl.Card), this.card_payment_method.GetType());
            ClassicAssert.AreEqual("xxxxxxxxxxxx4242", this.card_payment_method.card_number);
        }

        [Test]
        public void test_create_payment_method_bank()
        {
            ClassicAssert.AreEqual(typeof(pl.BankAccount), this.bank_payment_method.GetType());
            ClassicAssert.AreEqual("checking", this.bank_payment_method.account_type);

        }

        [Test]
        public void test_invalid_payment_method_type_invalid_attributes()
        {
            Assert.Throws<pl.InvalidAttributes>(
               () => pl.Transaction.Create(new { type = "invalid", card_number = "4242 4242 4242 4242" }));
        }


        [Test]
        public void test_card_payment_method_one()
        {
            ClassicAssert.NotNull(pl.PaymentMethod.FilterBy(new { this.card_payment_method.type }).One());

        }

        [Test]
        public void test_bank_payment_method_one()
        {
            ClassicAssert.NotNull(pl.PaymentMethod.FilterBy(new { this.bank_payment_method.type }).One());
        }
    }
}
