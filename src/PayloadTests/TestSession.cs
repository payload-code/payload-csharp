using NUnit.Framework;
using Payload.ARM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace Payload.Tests
{
    public class TestSession
    {
        Payload.Session session;

        [SetUp]
        public void Setup()
        {
            pl.ApiKey = null;

            this.session = new Payload.Session(Environment.GetEnvironmentVariable("API_KEY"));
            string url = Environment.GetEnvironmentVariable("API_URL");
            if (url != null)
                this.session.ApiUrl = url;
        }

        [Test]
        public void test_armobject_references()
        {
            var globalObjects = new HashSet<string>();

            foreach (Type type in Assembly.GetAssembly(typeof(pl)).GetTypes())
            {
                if (type.IsClass && !type.IsAbstract && type.BaseType != null)
                {
                    if (type.BaseType.IsGenericType && type.BaseType.GetGenericTypeDefinition() == typeof(ARMObjectBase<>))
                    {
                        if (type.Name != "ARMObject")
                            globalObjects.Add(type.Name);
                    }
                }
            }

            var sessionObjects = new HashSet<string>();

            foreach (PropertyInfo property in typeof(Payload.Session).GetProperties())
            {
                if (property.PropertyType.IsGenericType &&
                    property.PropertyType.GetGenericTypeDefinition() == typeof(ARMRequest<>))
                {
                    Type genericType = property.PropertyType.GetGenericArguments()[0];
                    string genericTypeName = genericType.Name;

                    // Check if the generic type name matches the property name
                    if (genericTypeName == property.Name)
                    {
                        sessionObjects.Add(property.Name);
                    }
                }
            }

            foreach (var globalObj in globalObjects)
            {
                if (!sessionObjects.Contains(globalObj))
                    throw new Exception($"Payload.Session is missing the resource property '{globalObj}'");
            }
        }

        [Test]
        public void test_client_token()
        {
            dynamic client_token = this.session.ClientToken.Create();
            Assert.AreEqual(client_token.status, "active");
            Assert.AreEqual(client_token.type, "client");

            client_token = this.session.ClientToken.Create(new { });
            Assert.AreEqual(client_token.status, "active");
            Assert.AreEqual(client_token.type, "client");
        }

        [Test]
        public void test_create()
        {
            var rand_email = Fixtures.RandomString(10) + "@example.com";

            var account = this.session.Create(
               new pl.Customer(new { email = rand_email, name = "Matt Perez" })
            );

            var get_account = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email }).One();
            Assert.NotNull(get_account);
        }

        [Test]
        public void test_create_multi()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var accounts = this.session.CreateAll(new[] {
                new pl.Customer(new{email=rand_email1, name="Matt Perez"}),
                new pl.Customer(new{email=rand_email2, name="Matt Perez"})
            });

            var get_account_1 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).One();
            var get_account_2 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).One();

            Assert.NotNull(get_account_1);
            Assert.NotNull(get_account_2);
        }

        [Test]
        public void test_get()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            dynamic account = this.session.Create(
               new pl.Customer(new { email = rand_email1, name = "Matt Perez" })
            );

            dynamic get_account = this.session.Select<pl.Customer>().Get(account.id);
            Assert.NotNull(get_account);
            Assert.True(account.id == get_account.id);
        }

        [Test]
        public void test_update()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var account = this.session.Create(
               new pl.Customer(new { email = rand_email1, name = "Matt Perez" })
            );

            var get_account1 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).One();
            var get_account2 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).One();
            Assert.NotNull(get_account1);
            Assert.Null(get_account2);

            account.Update(new
            {
                email = rand_email2
            });

            get_account1 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).One();
            get_account2 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).One();
            Assert.Null(get_account1);
            Assert.NotNull(get_account2);

        }

        [Test]
        public void test_delete()
        {
            var rand_email = Fixtures.RandomString(10) + "@example.com";

            var account = this.session.Create(
               new pl.Customer(new { email = rand_email, name = "Matt Perez" })
            );

            var get_account = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email }).One();
            Assert.NotNull(get_account);

            this.session.Delete(account);

            get_account = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email }).One();
            Assert.Null(get_account);
        }

        [Test]
        public void test_delete_multi()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var accounts = this.session.CreateAll(
                new[]{
                   new pl.Customer(new{email=rand_email1, name="Matt Perez"}),
                   new pl.Customer(new{email=rand_email2, name="Matt Perez"})
                }
            );


            var get_account_1 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).One();
            var get_account_2 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).One();

            Assert.NotNull(get_account_1);
            Assert.NotNull(get_account_2);

            this.session.DeleteAll(accounts);

            get_account_1 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).One();
            get_account_2 = this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).One();

            Assert.Null(get_account_1);
            Assert.Null(get_account_2);
        }

        [Test]
        public async Task test_create_async()
        {
            var rand_email = Fixtures.RandomString(10) + "@example.com";

            var account = await this.session.CreateAsync(
               new pl.Customer(new { email = rand_email, name = "Matt Perez" })
            );

            var get_account = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email }).OneAsync();
            Assert.NotNull(get_account);
        }

        [Test]
        public async Task test_create_multi_async()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var accounts = await this.session.CreateAllAsync(
                new pl.Customer(new { email = rand_email1, name = "Matt Perez" }),
                new pl.Customer(new { email = rand_email2, name = "Matt Perez" })
            );

            var get_account_1 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).OneAsync();
            var get_account_2 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).OneAsync();

            Assert.NotNull(get_account_1);
            Assert.NotNull(get_account_2);
        }

        [Test]
        public async Task test_get_async()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var account = await this.session.CreateAsync(
              new pl.Customer(new { email = rand_email1, name = "Matt Perez" })
           );

            var get_account = await this.session.Select<pl.Customer>().GetAsync(account.Data.id);
            Assert.NotNull(get_account);
            Assert.True(account.Data.id == get_account.id);
        }

        [Test]
        public async Task test_update_async()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var account = await this.session.CreateAsync(
               new pl.Customer(new { email = rand_email1, name = "Matt Perez" })
            );

            var get_account1 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).OneAsync();
            var get_account2 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).OneAsync();
            Assert.NotNull(get_account1);
            Assert.Null(get_account2);

            await account.UpdateAsync(new
            {
                email = rand_email2
            });

            get_account1 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).OneAsync();
            get_account2 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).OneAsync();
            Assert.Null(get_account1);
            Assert.NotNull(get_account2);

        }

        [Test]
        public async Task test_delete_async()
        {
            var rand_email = Fixtures.RandomString(10) + "@example.com";

            var account = await this.session.CreateAsync(
               new pl.Customer(new { email = rand_email, name = "Matt Perez" })
            );

            var get_account = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email }).OneAsync();
            Assert.NotNull(get_account);

            await this.session.DeleteAsync(account);

            get_account = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email }).OneAsync();
            Assert.Null(get_account);
        }

        [Test]
        public async Task test_delete_multi_async()
        {
            var rand_email1 = Fixtures.RandomString(10) + "@example.com";
            var rand_email2 = Fixtures.RandomString(10) + "@example.com";

            var accounts = await this.session.CreateAllAsync(
                new[]{
                   new pl.Customer(new{email=rand_email1, name="Matt Perez"}),
                   new pl.Customer(new{email=rand_email2, name="Matt Perez"})
                }
            );


            var get_account_1 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).OneAsync();
            var get_account_2 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).OneAsync();

            Assert.NotNull(get_account_1);
            Assert.NotNull(get_account_2);

            await this.session.DeleteAllAsync(accounts);

            get_account_1 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email1 }).OneAsync();
            get_account_2 = await this.session.Select<pl.Customer>().FilterBy(new { email = rand_email2 }).OneAsync();

            Assert.Null(get_account_1);
            Assert.Null(get_account_2);
        }
    }
}
