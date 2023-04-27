using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Net;

namespace Payload.Tests
{
    public class TestPaymentLink
    {
        dynamic processing_account;
        dynamic payment_link;

        [SetUp]
        public void Setup()
        {
            PayloadTestSetup.initAPI();
            this.processing_account = Fixtures.processing_account();
            this.payment_link = pl.PaymentLink.create(new
            {
                type = "one_time",
                description = "Payment Request",
                amount = 10.00,
                processing_id = this.processing_account.id
            }
               );
        }

        [Test]
        public void test_create_payment_link()
        {
            Assert.True(payment_link.processing_id == this.processing_account.id);
        }

        [Test]
        public void test_payment_link_one()
        {
            dynamic lnk = pl.PaymentLink.filter_by(new { type = this.payment_link.type }).one();
            Assert.NotNull(lnk);
            Assert.AreEqual(typeof(pl.PaymentLink), lnk.GetType());
        }


        [Test]
        public void test_create_payment_link_with_attachement()
        {

            string path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".pdf";
            using (FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
            {
                Byte[] info = new UTF8Encoding(true)
                    .GetBytes("This is some text in the file.");
                // Add some information to the file.
                fs.Write(info, 0, info.Length);
            }

            using (FileStream fs = File.Open(path, FileMode.Open))
            {
                var lnk = pl.PaymentLink.create(new
                {
                    type = "one_time",
                    description = "Payment Request",
                    amount = 10.00,
                    processing_id = this.processing_account.id,
                    additional_datafields = new[]{
                        new {
                            section = "Test",
                            fields = new []{
                                new {
                                    title="Test"
                                }
                            }
                        }
                    },
                    checkout_options = new
                    {
                        billing_address = false
                    },
                    attachments = new[] { new { file = fs } }
                });
                Assert.True(lnk.processing_id == this.processing_account.id);
                Assert.True(lnk.attachments.Count == 1);
                Assert.False(lnk.checkout_options.billing_address);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(lnk.attachments[0].url);

                string content = string.Empty;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    content = reader.ReadToEnd();
                }

                Assert.True(content.Equals("This is some text in the file."));
            }
        }

    }
}





