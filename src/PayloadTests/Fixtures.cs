using System;
using System.Linq;

namespace Payload.Tests
{
    public class Fixtures
    {

        public static dynamic customer_account()
        {
            dynamic customer = pl.Customer.create(new { email = "customer@example.com", name = "Customer Account" });
            return customer;

        }


        public static dynamic processing_account()
        {
            string id = Environment.GetEnvironmentVariable("PROCESSING_ID");
            if (id != null)
                return pl.ProcessingAccount.get(id);

            dynamic processing_account = pl.ProcessingAccount.create(new
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
                    website = "https://payload.co",
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
                payment_methods = new dynamic[]{
                    new pl.PaymentMethod(new {
                        type = "bank_account",
                        bank_account = new {
                            account_number = "123456789",
                            routing_number = "036001808",
                            account_type = "checking" },

                    })
                }
            });


            return processing_account;
        }


        public static dynamic card_payment()
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 100);
            dynamic card_payment = pl.Payment.create(new
            {
                amount = randomNumber,
                payment_method = new pl.Card(new { card_number = "4242 4242 4242 4242", expiry = "12/25" })
            });

            return card_payment;
        }



        public static dynamic bank_payment()
        {
            Random random = new Random();
            int randomNumber = random.Next(1, 100);
            dynamic bank_payment = pl.Payment.create(new
            {
                amount = randomNumber,
                payment_method = new pl.BankAccount(new
                {
                    account_number = "123456789",
                    routing_number = "036001808",
                    account_type = "checking"
                })
            });

            return bank_payment;
        }

        public static string RandomString(int length)
        {
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random(DateTime.Now.Millisecond);
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }
    }

}

