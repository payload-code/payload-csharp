using NUnit.Framework;
using System;

namespace Payload.Tests
{
    public class TestAccount
    {
        dynamic customer_account;
        dynamic processing_account;

        [OneTimeSetUp]
        public void ClassInit()
        {
            pl.api_key = "your_secret_key_3bfn0Ilzojfd5M76hFOxT";
        }

        [SetUp]
        public void Setup()
        {
            this.customer_account = Fixtures.customer_account();
            this.processing_account = Fixtures.processing_account();
        }


        [Test]
        public void test_create_customer_account()
        {
            Assert.AreEqual(typeof(pl.Customer), this.customer_account.GetType());
            Assert.AreEqual("Customer Account", this.customer_account.name);
            Assert.NotNull(this.customer_account.id);
        }


        public void test_create_processing_account()
        {
            Assert.AreEqual(typeof(pl.ProcessingAccount), this.processing_account.GetType());
            Assert.NotNull(this.processing_account.id);
        }

        [Test]
        public void test_customer_account_one()
        {
            Assert.NotNull(pl.Account.filter_by(new { email = customer_account.email }).one());
            Assert.AreEqual(typeof(pl.Customer), pl.Account.filter_by(new { email = customer_account.email }).one().GetType());
        }

        [Test]
        public void test_processing_account_one()
        {
            Assert.NotNull(pl.Account.filter_by(new { email = processing_account.email }).one());
            Assert.AreEqual(typeof(pl.ProcessingAccount), pl.Account.filter_by(new { email = processing_account.email }).one().GetType());
        }

        [Test]
        public void test_delete()
        {
            this.customer_account.delete();
            Assert.Throws<pl.NotFound>(
               () => pl.Account.get(this.customer_account.id));
        }


        [Test]
        public void test_create_mult_accounts()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var accounts = pl.create(
                new[]{
               new pl.Customer(new{email=rand_email1, name="Matt Perez"}),
               new pl.Customer(new{email=rand_email2, name="Andrea Kearney"})
                }
            );

            var get_account_1 = pl.Account.filter_by(new { email = rand_email1 }).all()[0];
            var get_account_2 = pl.Account.filter_by(new { email = rand_email2 }).all()[0];

            Assert.NotNull(get_account_1);
            Assert.NotNull(get_account_2);

        }
        [Test]
        public void test_get_processing_account()
        {
            Assert.AreEqual(typeof(pl.ProcessingAccount), this.processing_account.GetType());
            Assert.NotNull(pl.ProcessingAccount.get(this.processing_account.id));
            Assert.AreEqual("pending", this.processing_account.processing.status);
        }


        [Test]
        public void test_paging_and_ordering_results()
        {
            var accounts = pl.create(new
                []
                    {
                    new pl.Customer(new {email="account1@example.com", name="Randy Robson"}),
                    new pl.Customer(new {email="account2@example.com", name="Brandy Bobson"}),
                    new pl.Customer(new {email="account3@example.com", name="Mandy Johnson"}),
                    }
            );

            var customers = pl.Customer.filter_by(new { order_by = "created_at", limit = 3, offset = 1 }).all();

            Assert.True(customers.Count == 3);
            Assert.True(Convert.ToDateTime(customers[0].created_at) < Convert.ToDateTime(customers[1].created_at));
            Assert.True(Convert.ToDateTime(customers[1].created_at) < Convert.ToDateTime(customers[2].created_at));


        }



        [Test]
        public void test_update_customer_account()
        {
            this.customer_account.update(new { email = "test2@example.com" });

            Assert.True(this.customer_account.email == "test2@example.com");
        }


        public void test_update_mult_acc()
        {
            var customer_account_1 = pl.Customer.create(new
            {
                name = "Brandy",
                email = "brandy@example.com"
            });

            var customer_account_2 = pl.Customer.create(new
            {
                name = "Sandy",
                email = "sandy@example.com"
            });

            pl.update(new object[]{
                new object[]{ customer_account_1, new { email="matt.perez@newwork.com" } },
                new object[]{ customer_account_2, new { email="andrea.kearney@newwork.com" } }
            });

            Assert.True(customer_account_1.email == "brandy@example.com");
            Assert.True(customer_account_2.email == "sandy@example.com");
        }

        [Test]
        public void test_get_customer_account()
        {
            Assert.AreEqual(typeof(pl.Customer), this.customer_account.GetType());
            Assert.NotNull(pl.Customer.get(this.customer_account.id));
        }



        [Test]
        public void test_select_cust_attr()
        {
            var select_customer = pl.Customer.select("id").get(this.customer_account.id);

            Assert.AreEqual(typeof(pl.Customer), this.customer_account.GetType());
            Assert.True(select_customer.id == this.customer_account.id);
        }

    }
}

