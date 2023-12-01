using NUnit.Framework;
using System;

namespace Payload.Tests
{
    public class TestAccount
    {
        dynamic customer_account;
        dynamic processing_account;

        [SetUp]
        public void Setup()
        {
            PayloadTestSetup.initAPI();
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
            Assert.NotNull(pl.Customer.FilterBy(new { email = customer_account.email }).One());
            Assert.AreEqual(typeof(pl.Customer), pl.Customer.FilterBy(new { email = customer_account.email }).One().GetType());
        }

        [Test]
        public void test_processing_account_one()
        {
            Assert.NotNull(pl.ProcessingAccount.FilterBy(new { name = processing_account.name }).One());
            Assert.AreEqual(typeof(pl.ProcessingAccount), pl.ProcessingAccount.FilterBy(new { name = processing_account.name }).One().GetType());
        }

        [Test]
        public void test_delete()
        {
            this.customer_account.Delete();
            Assert.Throws<pl.NotFound>(
               () => pl.Account.Get(this.customer_account.id));
        }


        [Test]
        public void test_create_mult_accounts()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var accounts = pl.CreateAll(
                new[]{
               new pl.Customer(new{email=rand_email1, name="Matt Perez"}),
               new pl.Customer(new{email=rand_email2, name="Andrea Kearney"})
                }
            );

            var get_account_1 = pl.Account.FilterBy(new { email = rand_email1 }).All()[0];
            var get_account_2 = pl.Account.FilterBy(new { email = rand_email2 }).All()[0];

            Assert.NotNull(get_account_1);
            Assert.NotNull(get_account_2);

        }
        [Test]
        public void test_get_processing_account()
        {
            Assert.AreEqual(typeof(pl.ProcessingAccount), this.processing_account.GetType());
            Assert.NotNull(pl.ProcessingAccount.Get(this.processing_account.id));
        }


        [Test]
        public void test_paging_and_ordering_results()
        {
            dynamic accounts = pl.CreateAll(new[] {
                    new pl.Customer(new {email="account1@example.com", name="Randy Robson"}),
                    new pl.Customer(new {email="account2@example.com", name="Brandy Bobson"}),
                    new pl.Customer(new {email="account3@example.com", name="Mandy Johnson"})
            });

            dynamic customers = pl.Customer.FilterBy(new { order_by = "created_at", limit = 3, offset = 1 }).All();

            Assert.True(customers.Count == 3);
            Assert.True(Convert.ToDateTime(customers[0].created_at) <= Convert.ToDateTime(customers[1].created_at));
            Assert.True(Convert.ToDateTime(customers[1].created_at) <= Convert.ToDateTime(customers[2].created_at));


        }



        [Test]
        public void test_update_customer_account()
        {
            this.customer_account.Update(new { email = "test2@example.com" });

            Assert.True(this.customer_account.email == "test2@example.com");
        }


        public void test_update_mult_acc()
        {
            var customer_account_1 = pl.Customer.Create(new
            {
                name = "Brandy",
                email = "brandy@example.com"
            });

            var customer_account_2 = pl.Customer.Create(new
            {
                name = "Sandy",
                email = "sandy@example.com"
            });

            pl.UpdateAll(
                (customer_account_1, new { email="matt.perez@newwork.com" }),
                (customer_account_2, new { email="andrea.kearney@newwork.com" })
            );

            Assert.True(customer_account_1.Data.email == "brandy@example.com");
            Assert.True(customer_account_2.Data.email == "sandy@example.com");
        }

        [Test]
        public void test_get_customer_account()
        {
            Assert.AreEqual(typeof(pl.Customer), this.customer_account.GetType());
            Assert.NotNull(pl.Customer.Get(this.customer_account.id));
        }



        [Test]
        public void test_select_cust_attr()
        {
            var select_customer = pl.Customer.Select("id").Get(this.customer_account.id);

            Assert.AreEqual(typeof(pl.Customer), this.customer_account.GetType());
            Assert.True(select_customer.id == this.customer_account.id);
        }

    }
}

