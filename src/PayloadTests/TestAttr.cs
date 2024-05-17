using NUnit.Framework;
using NUnit.Framework.Legacy;
using Payload.ARM;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Payload.Tests
{
    public class AttrTests
    {
        private dynamic subject;

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestToStringProperty()
        {
            // Arrange
            var actual = pl.Attr.payment_method.card_number;

            // Act
            var result = actual.ToString();

            // Assert
            ClassicAssert.AreEqual("payment_method[card_number]", result);
        }

        [Test]
        public void TestToStringMethod()
        {
            // Arrange
            var actual = pl.Attr.amount.sum();

            // Act
            var result = actual.ToString();

            // Assert
            ClassicAssert.AreEqual("sum(amount)", result);
        }

        [Test]
        public void TestToStringNestedObject()
        {
            // Arrange
            var actual = pl.Attr.payment_method.card_number.expiry_date;

            // Act
            var result = actual.ToString();

            // Assert
            ClassicAssert.AreEqual("payment_method[card_number][expiry_date]", result);
        }
    }
}
