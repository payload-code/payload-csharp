using NUnit.Framework;
using System;

namespace Payload.Tests
{
    public class TestBilling
    {
        dynamic billing_schedule;
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
            this.processing_account = Fixtures.processing_account();
            this.customer_account = Fixtures.customer_account();

            this.billing_schedule = pl.BillingSchedule.create(new
            {
                start_date = "2019-01-01",
                end_date = "2019-12-31",
                recurring_frequency = "monthly",
                type = "subscription",
                customer_id = this.customer_account.id,
                processing_id = this.processing_account.id,
                charges = new pl.BillingCharge(new { type = "option_1", amount = 39.99 })
            });
        }

        [Test]
        public void test_create_billing_schedule()
        {
            Assert.AreEqual(typeof(pl.BillingSchedule), this.billing_schedule.GetType());
            Assert.True(this.billing_schedule.processing_id == this.processing_account.id);
            Assert.True(this.billing_schedule.charges[0].amount == 39.99);
        }

        [Test]
        public void test_update_billing_schedule_frequency()
        {
            Assert.True(this.billing_schedule.processing_id == processing_account.id);
            Assert.True(this.billing_schedule.charges[0].amount == 39.99);

            this.billing_schedule.update(new { recurring_frequency = "quarterly" });

            Assert.True(this.billing_schedule.recurring_frequency == "quarterly");
        }


        [Test]
        public void test_select_billing_charge()
        {
            var billing_charge = pl.BillingSchedule.select("*", "charges").get(this.billing_schedule.id).charges[0];

            Assert.AreEqual(typeof(pl.BillingCharge), billing_charge.GetType());
        }


        [Test]
        public void test_billing_schedule_one()
        {
            Assert.NotNull(pl.BillingSchedule.filter_by(new { type = this.billing_schedule.type }).one());
            Assert.AreEqual(typeof(pl.BillingSchedule), pl.BillingSchedule.filter_by(new { type = billing_schedule.type }).one().GetType());
        }

        [Test]
        public void test_billing_charge_one()
        {
            Assert.NotNull(pl.BillingCharge.filter_by(new { type = this.billing_schedule.charges[0].type }).one());

            Assert.AreEqual(typeof(pl.BillingCharge), pl.BillingCharge.filter_by(new { type = this.billing_schedule.charges[0].type }).one().GetType());
        }
    }
}





