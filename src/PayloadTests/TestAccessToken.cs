using NUnit.Framework;
using System;
using System.IO;
using System.Text;
using System.Net;

namespace Payload.Tests
{
    public class TestAccessToken
    {

        [SetUp]
        public void Setup()
        {
            PayloadTestSetup.initAPI();
        }

        [Test]
        public void test_create_client_key()
        {
            var client_key = pl.ClientKey.Create();
            Assert.True(client_key.status == 'active');
            Assert.True(client_key.type == 'client');

        }

    }
}
