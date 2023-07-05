using NUnit.Framework;
using Payload.ARM;
using System;

namespace Payload.Tests
{
    public class TestARMRequest
    {
        [Test]
        public void test_range()
        {
            var req = new ARMRequest<pl.Payment>().Range(5, 15);

            Assert.AreEqual(req._filters["offset"], 5);
            Assert.AreEqual(req._filters["limit"], 10);
        }
    }
}

