using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Payload.Tests
{
    public class Fixtures
    {
        public static pl.Customer customer_account()
        {
            var customer = pl.Customer.Create(new { email = "customer@example.com", name = "Customer Account" });
            return customer;

        }


        public static pl.ProcessingAccount processing_account()
        {
            string id = Environment.GetEnvironmentVariable("PROCESSING_ID");
            if (id != null)
                return pl.ProcessingAccount.Get(id);

            var processing_account = pl.ProcessingAccount.Create(new
            {
                name = "Processing Account",
                legal_entity = new
                {
                    legal_name = "Test",
                    type = "INDIVIDUAL_SOLE_PROPRIETORSHIP",
                    ein = "23 423 4234",
                    street_address = "123 Example St",
                    unit_number = "Suite 1",
                    city = "New York",
                    state_province = "NY",
                    state_incorporated = "NY",
                    postal_code = "11238",
                    phone_number = "(111) 222-3333",
                    country = "US",
                    website = "https://payload.com",
                    start_date = "05/01/2015",
                    contact_name = "Test Person",
                    contact_email = "test.person@example.com",
                    contact_title = "VP",
                    owners = new
                    {
                        full_name = "Test Person",
                        email = "test.person@example.com",
                        ssn = "234 23 4234",
                        birth_date = "06/20/1985",
                        title = "CEO",
                        ownership = "100",
                        street_address = "4455 Carver Woods Drive, Suite 200",
                        unit_number = "2408",
                        city = "Cincinnati",
                        state_province = "OH",
                        postal_code = "45242",
                        phone_number = "(111) 222-3333",
                        type = "owner",
                    }
                },
                payment_methods = new pl.PaymentMethod[]{
                    new pl.PaymentMethod(new {
                        type = "bank_account",
                        account_holder = "Test User",
                        bank_account = new {
                            account_number = "123456789",
                            routing_number = "036001808",
                            account_type = "checking" },

                    })
                }
            });


            return processing_account;
        }


        public static pl.Payment card_payment()
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 100);
            var card_payment = pl.Payment.Create(new
            {
                amount = randomNumber,
                payment_method = new { 
                    type = "card",
                    card = new {
                        card_number = "4242 4242 4242 4242", 
                        expiry = "12/28",
                        card_code = "123"
                    }
                }
            });

            return card_payment;
        }



        public static pl.Payment bank_payment()
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 100);
            var bank_payment = pl.Payment.Create(new
            {
                amount = randomNumber,
                payment_method = new pl.PaymentMethod(new {
                    account_holder = "Test User",
                    type = "bank_account",
                    bank_account = new {
                        account_number = "123456789",
                        routing_number = "036001808",
                        account_type = "checking"
                    }
                })
            });

            return bank_payment;
        }

        public static string RandomString(int length)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = new char[length];

            using (var rng = new RNGCryptoServiceProvider())
            {
                byte[] buffer = new byte[length];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(buffer);
                    int num = buffer[i] % pool.Length;
                    chars[i] = pool[num];
                }
            }

            return new string(chars);
        }
    }

}

