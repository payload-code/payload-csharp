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
        public void test_create_client_token()
        {
            var client_key = pl.ClientToken.Create();
            Assert.True(client_key.status == 'active');
            Assert.True(client_key.type == 'client');

        }

    }
}
