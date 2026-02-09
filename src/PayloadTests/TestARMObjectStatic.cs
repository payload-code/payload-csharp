using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Payload.Tests
{
    public class TestARMObjectStatic
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
                    Content = new StringContent("{\"object\":\"list\",\"values\":[]}")
                };
                return Task.FromResult(response);
            }
        }

        [Test]
        public void test_all_sends_get_to_correct_endpoint()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key");
            session._httpMessageHandler = mockHandler;

            try { session.Customer.All(); } catch { }

            var req = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(req);
            ClassicAssert.AreEqual("GET", req.Method.Method);
            ClassicAssert.IsTrue(req.RequestUri.AbsolutePath.EndsWith("/customers"));
        }

        [Test]
        public void test_all_on_polymorphic_type_includes_filter()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key");
            session._httpMessageHandler = mockHandler;

            try { session.Payment.All(); } catch { }

            var req = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(req);
            StringAssert.Contains("type=payment", req.RequestUri.Query);
        }

        [Test]
        public void test_group_by_includes_query_param()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key");
            session._httpMessageHandler = mockHandler;

            try { session.Customer.GroupBy("status").All(); } catch { }

            var req = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(req);
            StringAssert.Contains("group_by", req.RequestUri.Query);
            StringAssert.Contains("status", req.RequestUri.Query);
        }

        [Test]
        public void test_order_by_includes_query_param()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key");
            session._httpMessageHandler = mockHandler;

            try { session.Customer.OrderBy(new[] { "desc(created_at)" }).All(); } catch { }

            var req = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(req);
            StringAssert.Contains("order_by", req.RequestUri.Query);
            StringAssert.Contains("desc(created_at)", req.RequestUri.Query);
        }

        [Test]
        public void test_group_by_on_polymorphic_type_includes_filter()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key");
            session._httpMessageHandler = mockHandler;

            try { session.Payment.GroupBy("status").All(); } catch { }

            var req = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(req);
            StringAssert.Contains("type=payment", req.RequestUri.Query);
            StringAssert.Contains("group_by", req.RequestUri.Query);
        }

        [Test]
        public void test_order_by_on_polymorphic_type_includes_filter()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key");
            session._httpMessageHandler = mockHandler;

            try { session.Payment.OrderBy(new[] { "desc(created_at)" }).All(); } catch { }

            var req = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(req);
            StringAssert.Contains("type=payment", req.RequestUri.Query);
            StringAssert.Contains("order_by", req.RequestUri.Query);
        }

        [Test]
        public void test_select_on_polymorphic_type_includes_filter()
        {
            var mockHandler = new MockHttpMessageHandler();
            var session = new Payload.Session("test_key");
            session._httpMessageHandler = mockHandler;

            try { session.Payment.Select("amount").All(); } catch { }

            var req = mockHandler.CapturedRequest;
            ClassicAssert.IsNotNull(req);
            StringAssert.Contains("type=payment", req.RequestUri.Query);
            StringAssert.Contains("fields", req.RequestUri.Query);
        }
    }
}
