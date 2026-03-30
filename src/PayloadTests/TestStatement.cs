using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Payload.Tests
{
    public class TestStatement
    {
        dynamic processing_account;
        dynamic statement;

        [SetUp]
        public void Setup()
        {
            PayloadTestSetup.initAPI();
            this.processing_account = Fixtures.processing_account();
        }

        [Test]
        public void test_create_statement()
        {
            var now = DateTime.UtcNow;
            var start_date = new DateTime(now.Year, now.Month, 1);
            var end_date = start_date.AddMonths(1).AddDays(-1);

            var created_statement = pl.Statement.Create(new
            {
                processing_id = this.processing_account.id,
                start_date = start_date,
                end_date = end_date
            });

            ClassicAssert.AreEqual(typeof(pl.Statement), created_statement.GetType());
            ClassicAssert.NotNull(created_statement.id);
            ClassicAssert.AreEqual(this.processing_account.id, created_statement.processing_id);
        }

        [Test]
        public void test_get_statement()
        {
            var now = DateTime.UtcNow;
            var start_date = new DateTime(now.Year, now.Month, 1);
            var end_date = start_date.AddMonths(1).AddDays(-1);

            this.statement = pl.Statement.Create(new
            {
                processing_id = this.processing_account.id,
                start_date = start_date,
                end_date = end_date
            });

            var retrieved_statement = pl.Statement.Get(this.statement.id);

            ClassicAssert.True(retrieved_statement.id == this.statement.id);
            ClassicAssert.AreEqual(typeof(pl.Statement), retrieved_statement.GetType());
        }

        [Test]
        public void test_statement_not_found()
        {
            Assert.Throws<pl.NotFound>(
                () => pl.Statement.Get("invalid_id"));
        }

        [Test]
        public void test_filter_statements()
        {
            var now = DateTime.UtcNow;
            var start_date = new DateTime(now.Year, now.Month, 1);
            var end_date = start_date.AddMonths(1).AddDays(-1);

            this.statement = pl.Statement.Create(new
            {
                processing_id = this.processing_account.id,
                start_date = start_date,
                end_date = end_date
            });

            var statements = pl.Statement
                .FilterBy(new { processing_id = this.processing_account.id })
                .All();

            ClassicAssert.True(statements.Count > 0);
            ClassicAssert.AreEqual(typeof(pl.Statement), statements[0].GetType());
        }

        [Test]
        public void test_update_statement_not_permitted()
        {
            var now = DateTime.UtcNow;
            var start_date = new DateTime(now.Year, now.Month, 1);
            var end_date = start_date.AddMonths(1).AddDays(-1);

            this.statement = pl.Statement.Create(new
            {
                processing_id = this.processing_account.id,
                start_date = start_date,
                end_date = end_date
            });

            var new_end_date = start_date.AddMonths(1).AddDays(-2);

            // Updating statements is only allowed by system admins
            Assert.Throws<pl.NotPermitted>(
                () => this.statement.Update(new { end_date = new_end_date }));
        }

        [Test]
        public void test_delete_statement()
        {
            var now = DateTime.UtcNow;
            var start_date = new DateTime(now.Year, now.Month, 1);
            var end_date = start_date.AddMonths(1).AddDays(-1);

            this.statement = pl.Statement.Create(new
            {
                processing_id = this.processing_account.id,
                start_date = start_date,
                end_date = end_date
            });

            var statement_id = this.statement.id;
            this.statement.Delete();

            Assert.Throws<pl.NotFound>(
                () => pl.Statement.Get(statement_id));
        }

        [Test]
        public void test_select_statement_fields()
        {
            var now = DateTime.UtcNow;
            var start_date = new DateTime(now.Year, now.Month, 1);
            var end_date = start_date.AddMonths(1).AddDays(-1);

            this.statement = pl.Statement.Create(new
            {
                processing_id = this.processing_account.id,
                start_date = start_date,
                end_date = end_date
            });

            var selected_statement = pl.Statement.Select("id", "processing_id").Get(this.statement.id);

            ClassicAssert.AreEqual(typeof(pl.Statement), selected_statement.GetType());
            ClassicAssert.True(selected_statement.id == this.statement.id);
            ClassicAssert.NotNull(selected_statement.processing_id);
        }
    }
}
