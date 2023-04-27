using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Payload.Tests
{
    public class TestSession
    {
        pl.Session session;
        dynamic bank_payment;
        dynamic processing_account;

        [SetUp]
        public void Setup()
        {
            pl.api_key = null;

            this.session = new pl.Session(Environment.GetEnvironmentVariable("API_KEY"));
            string url = Environment.GetEnvironmentVariable("API_URL");
            if (url != null)
                this.session.api_url = url;
        }

        [Test]
        public void test_create()
        {
            var rand_email = Fixtures.RandomString(10) + "@example.com";

            var account = this.session.create(
               new pl.Customer(new { email = rand_email, name = "Matt Perez" })
            );

            var get_account = this.session.query<pl.Customer>().filter_by(new { email = rand_email }).one();
            Assert.NotNull(get_account);
        }

        [Test]
        public void test_create_multi()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var accounts = this.session.create(
                new[]{
                   new pl.Customer(new{email=rand_email1, name="Matt Perez"}),
                   new pl.Customer(new{email=rand_email2, name="Matt Perez"})
                }
            );

            var get_account_1 = this.session.query<pl.Customer>().filter_by(new { email = rand_email1 }).one();
            var get_account_2 = this.session.query<pl.Customer>().filter_by(new { email = rand_email2 }).one();

            Assert.NotNull(get_account_1);
            Assert.NotNull(get_account_2);
        }

        [Test]
        public void test_get()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var account = this.session.create(
               new pl.Customer(new { email = rand_email1, name = "Matt Perez" })
            );

            var get_account = this.session.query<pl.Customer>().get(account.id);
            Assert.NotNull(get_account);
            Assert.True(account.id == get_account.id);
        }

        [Test]
        public void test_update()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var account = this.session.create(
               new pl.Customer(new { email = rand_email1, name = "Matt Perez" })
            );

            var get_account1 = this.session.query<pl.Customer>().filter_by(new { email = rand_email1 }).one();
            var get_account2 = this.session.query<pl.Customer>().filter_by(new { email = rand_email2 }).one();
            Assert.NotNull(get_account1);
            Assert.Null(get_account2);

            account.update(new
            {
                email = rand_email2
            });

            get_account1 = this.session.query<pl.Customer>().filter_by(new { email = rand_email1 }).one();
            get_account2 = this.session.query<pl.Customer>().filter_by(new { email = rand_email2 }).one();
            Assert.Null(get_account1);
            Assert.NotNull(get_account2);

        }

        [Test]
        public void test_delete()
        {
            var rand_email = Fixtures.RandomString(10) + "@example.com";

            var account = this.session.create(
               new pl.Customer(new { email = rand_email, name = "Matt Perez" })
            );

            var get_account = this.session.query<pl.Customer>().filter_by(new { email = rand_email }).one();
            Assert.NotNull(get_account);

            this.session.delete(account);

            get_account = this.session.query<pl.Customer>().filter_by(new { email = rand_email }).one();
            Assert.Null(get_account);
        }

        [Test]
        public void test_delete_multi()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var accounts = this.session.create(
                new[]{
                   new pl.Customer(new{email=rand_email1, name="Matt Perez"}),
                   new pl.Customer(new{email=rand_email2, name="Matt Perez"})
                }
            );


            var get_account_1 = this.session.query<pl.Customer>().filter_by(new { email = rand_email1 }).one();
            var get_account_2 = this.session.query<pl.Customer>().filter_by(new { email = rand_email2 }).one();

            Assert.NotNull(get_account_1);
            Assert.NotNull(get_account_2);

            this.session.delete(accounts);

            get_account_1 = this.session.query<pl.Customer>().filter_by(new { email = rand_email1 }).one();
            get_account_2 = this.session.query<pl.Customer>().filter_by(new { email = rand_email2 }).one();

            Assert.Null(get_account_1);
            Assert.Null(get_account_2);
        }
    }
}
