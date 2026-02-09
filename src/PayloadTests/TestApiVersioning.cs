using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Payload.Tests
{
    public class TestApiVersioning
    {
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            public HttpRequestMessage CapturedRequest { get; private set; }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                CapturedRequest = request;
                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"object\":\"customer\",\"id\":\"cust_test123\"}")
                };
                return Task.FromResult(response);
            }
        }

        [Test]
        public void test_no_api_version_header_when_not_set()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key", apiVersion: null);
            session._httpMessageHandler = mockHandler;

            try { session.Customer.Get("cust_test123"); } catch { }

            var capturedRequest = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(capturedRequest);
            ClassicAssert.IsFalse(capturedRequest.Headers.Contains("X-API-Version"));
        }

        [Test]
        public void test_api_version_header_with_other_headers()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key", apiVersion: "v2.0");
            session._httpMessageHandler = mockHandler;

            try { session.Customer.Create(new { email = "test@example.com" }); } catch { }

            var capturedRequest = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(capturedRequest);
            ClassicAssert.IsTrue(capturedRequest.Headers.Contains("X-API-Version"));
            ClassicAssert.IsTrue(capturedRequest.Headers.Contains("Authorization"));
            ClassicAssert.IsTrue(capturedRequest.Headers.Contains("Accept"));
            if (capturedRequest.Content != null)
            {
                ClassicAssert.IsNotNull(capturedRequest.Content.Headers.ContentType);
            }
        }

        [Test]
        [TestCase("v1.0")]
        [TestCase("v2.1")]
        public void test_different_api_version_values(string version)
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key", apiVersion: version);
            session._httpMessageHandler = mockHandler;

            try { session.Customer.Get("cust_test123"); } catch { }

            var capturedRequest = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(capturedRequest);
            ClassicAssert.IsTrue(capturedRequest.Headers.Contains("X-API-Version"));
            ClassicAssert.AreEqual(version, capturedRequest.Headers.GetValues("X-API-Version").First());
        }

        [Test]
        [TestCase("GET")]
        [TestCase("POST")]
        [TestCase("PUT")]
        [TestCase("DELETE")]
        public void test_api_version_on_all_http_methods(string httpMethod)
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key", apiVersion: "v2.0");
            session._httpMessageHandler = mockHandler;

            try
            {
                switch (httpMethod)
                {
                    case "GET":
                        session.Customer.Get("cust_test123");
                        break;
                    case "POST":
                        session.Customer.Create(new { email = "test@example.com" });
                        break;
                    case "PUT":
                        var customer1 = new pl.Customer(new { id = "cust_123", name = "Test" });
                        session.Customer.UpdateAll((customer1, new { email = "new@example.com" }));
                        break;
                    case "DELETE":
                        var customer2 = new pl.Customer(new { id = "cust_123", name = "Test" });
                        session.Customer.Delete(customer2);
                        break;
                }
            }
            catch { }

            var capturedRequest = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(capturedRequest);
            ClassicAssert.AreEqual(httpMethod, capturedRequest.Method.Method);
            ClassicAssert.IsTrue(capturedRequest.Headers.Contains("X-API-Version"));
            ClassicAssert.AreEqual("v2.0", capturedRequest.Headers.GetValues("X-API-Version").First());
        }

        [Test]
        public void test_global_api_version_fallback()
        {
            var mockHandler = new MockHttpMessageHandler();
            pl.ApiVersion = "v2.5";
            pl.DefaultSession._httpMessageHandler = mockHandler;

            try { pl.DefaultSession.Customer.Get("cust_test123"); } catch { }

            var capturedRequest = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(capturedRequest);
            ClassicAssert.IsTrue(capturedRequest.Headers.Contains("X-API-Version"));
            ClassicAssert.AreEqual("v2.5", capturedRequest.Headers.GetValues("X-API-Version").First());

            pl.ApiVersion = null;
            pl.DefaultSession._httpMessageHandler = null;
        }
    }
}


