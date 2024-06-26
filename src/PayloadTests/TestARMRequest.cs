﻿using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Payload.ARM;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;

namespace Payload.Tests
{
    public class TestARMRequest
    {
        [Test]
        public void test_defaultparams()
        {
            pl.Customer.DefaultParams.Fields = new[] { "*", pl.Attr.name };
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers?fields[0]=*&fields[1]=name", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.All();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
            pl.Customer.DefaultParams.Fields = new object[0];
        }

        [Test]
        public void test_range()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers?offset=5&limit=10", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.Range(5, 15).All();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_select()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers?fields[0]=processing%5Bname%5D&fields[1]=sum(amount)", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.Select(new[]
            {
                pl.Attr.processing.name,
                pl.Attr.amount.sum(),
            }).All();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_filterby()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers?name=Jimmy+John&payment_method[uses]=%3E%3D1&payment_method[card_number][expiry_date]=%3E2015-01-01&payment_method[card_number][expiry_date]=%3C2015-12-31&processing_id=acct_0a9s87df0987adsf", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.FilterBy(new[]
            {
                pl.Attr.name.eq("Jimmy John"),
                pl.Attr.payment_method.uses.ge(1),
                pl.Attr.payment_method.card_number.expiry_date.gt("2015-01-01"),
                pl.Attr.payment_method.card_number.expiry_date.lt("2015-12-31"),
                new
                {
                    processing_id = "acct_0a9s87df0987adsf"
                }
            }).All();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_groupby()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers?group_by[0]=name&group_by[1]=created_at", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.GroupBy(new[]
            {
                pl.Attr.name,
                pl.Attr.created_at
            }).All();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_orderby()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers?order_by[0]=asc(name)&order_by[1]=desc(created_at)&order_by[2]=attrs%5Btest%5D", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.OrderBy(new[]
            {
                pl.Attr.name.asc(),
                pl.Attr.created_at.desc(),
                pl.Attr.attrs.test
            }).All();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_one()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers?limit=1", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.One();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_all()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.All();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_get()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.GET, method);
                    ClassicAssert.AreEqual("/customers/test_id", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.Get("test_id");

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_create()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), It.IsAny<ExpandoObject>()))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.POST, method);
                    ClassicAssert.AreEqual("/customers", route);
                    DeepDiff.Diff(new { name = "Billy Bob", email = "billy.bob@payload.com" }, body);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.Create(new
            {
                name = "Billy Bob",
                email = "billy.bob@payload.com",
            });

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), It.IsAny<ExpandoObject>()), Times.Once);
        }

        [Test]
        public void test_create_all()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), It.IsAny<ExpandoObject>()))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.POST, method);
                    ClassicAssert.AreEqual("/customers", route);
                    DeepDiff.Diff(new
                    {
                        @object = "list",
                        values = new[]
                        {
                            new
                            {
                                name = "Billy Bob",
                                email = "billy.bob@payload.com",
                            },
                            new
                            {
                                name = "Billy Bob",
                                email = "billy.bob@payload.com",
                            },
                            new
                            {
                                name = "Billy Bob",
                                email = "billy.bob@payload.com",
                            },
                        }
                    }, body);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object.CreateAll(new[]
            {
                new
                {
                    name = "Billy Bob",
                    email = "billy.bob@payload.com",
                },
                new
                {
                    name = "Billy Bob",
                    email = "billy.bob@payload.com",
                },
                new
                {
                    name = "Billy Bob",
                    email = "billy.bob@payload.com",
                },
            });

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), It.IsAny<ExpandoObject>()), Times.Once);
        }

        [Test]
        public void test_update_all()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), It.IsAny<ExpandoObject>()))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.PUT, method);
                    ClassicAssert.AreEqual("/customers", route);
                    DeepDiff.Diff(new
                    {
                        @object = "list",
                        values = new[]
                        {
                            new
                            {
                                id = "acct_s9d87f9s8d7f9",
                                email = "matt.perez@newwork.com"
                            },
                            new
                            {
                                id = "acct_987gs09d87f9d",
                                email = "andrea.kearney@newwork.com"
                            }
                        }
                    }, body);
                })
                .ReturnsAsync(mockRespJson);

            var customer_account_1 = new pl.Customer(new
            {
                id = "acct_s9d87f9s8d7f9",
                name = "Brandy",
                email = "brandy@example.com"
            });

            var customer_account_2 = new pl.Customer(new
            {
                id = "acct_987gs09d87f9d",
                name = "Sandy",
                email = "sandy@example.com"
            });

            var req = mock.Object.UpdateAll(
                (customer_account_1, new { email = "matt.perez@newwork.com" }),
                (customer_account_2, new { email = "andrea.kearney@newwork.com" })
            );

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), It.IsAny<ExpandoObject>()), Times.Once);
        }

        [Test]
        public void test_query_update()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), It.IsAny<ExpandoObject>()))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.PUT, method);
                    ClassicAssert.AreEqual("/customers?processing_id=acct_9s8d7f98s7dfsf&mode=query", route);
                    DeepDiff.Diff(new { email = "matt.perez@newwork.com" }, body);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object
                .FilterBy(new { processing_id = "acct_9s8d7f98s7dfsf" })
                .QueryUpdate(new { email = "matt.perez@newwork.com" });

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), It.IsAny<ExpandoObject>()), Times.Once);
        }

        [Test]
        public void test_delete()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.DELETE, method);
                    ClassicAssert.AreEqual("/customers/acct_s9d87f9s8d7f9", route);
                })
                .ReturnsAsync(mockRespJson);

            var cust = new pl.Customer(new
            {
                id = "acct_s9d87f9s8d7f9",
                name = "Brandy",
                email = "brandy@example.com"
            });

            var req = mock.Object.Delete(cust);

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_delete_all()
        {

            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.DELETE, method);
                    ClassicAssert.AreEqual("/customers?mode=query&id=acct_s9d87f9s8d7f9%7Cacct_987gs09d87f9d", route);
                })
                .ReturnsAsync(mockRespJson);

            var customer_account_1 = new pl.Customer(new
            {
                id = "acct_s9d87f9s8d7f9",
                name = "Brandy",
                email = "brandy@example.com"
            });

            var customer_account_2 = new pl.Customer(new
            {
                id = "acct_987gs09d87f9d",
                name = "Sandy",
                email = "sandy@example.com"
            });

            var req = mock.Object.DeleteAll(customer_account_1, customer_account_2);

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }

        [Test]
        public void test_query_delete()
        {
            var mock = new Mock<ARMRequest<pl.Customer>> { CallBase = true };
            var mockRespJson = new JSONObject();
            mockRespJson["values"] = new JArray();

            mock.Setup(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null))
                .Callback<RequestMethods, string, ExpandoObject>((method, route, body) =>
                {
                    ClassicAssert.AreEqual(RequestMethods.DELETE, method);
                    ClassicAssert.AreEqual("/customers?processing_id=acct_9s8d7f98s7dfsf&mode=query", route);
                })
                .ReturnsAsync(mockRespJson);

            var req = mock.Object
                .FilterBy(new { processing_id = "acct_9s8d7f98s7dfsf" })
                .QueryDelete();

            mock.Verify(m => m.ExecuteRequestAsync(It.IsAny<RequestMethods>(), It.IsAny<string>(), null), Times.Once);
        }
    }
}

