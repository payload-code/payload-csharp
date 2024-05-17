using NUnit.Framework;
using NUnit.Framework.Legacy;
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
            dynamic client_token = pl.ClientToken.Create();
            ClassicAssert.AreEqual(client_token.status, "active");
            ClassicAssert.AreEqual(client_token.type, "client");

        }

    }
}
