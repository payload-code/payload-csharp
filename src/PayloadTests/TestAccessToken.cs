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
            var client_token = pl.ClientToken.Create();
            Assert.True(client_token.status == 'active');
            Assert.True(client_token.type == 'client');

        }

    }
}
