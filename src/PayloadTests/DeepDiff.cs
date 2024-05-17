using Newtonsoft.Json.Linq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Payload;
using Payload.ARM;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Payload.Tests
{
    public class DeepDiff
    {
        public static void Diff(object obj1, object obj2)
        {
            CompareObjects(null, obj1, obj2);
        }

        private static void CompareObjects(string key, object obj1, object obj2, bool isList = false)
        {
            void CompareWithEquals()
            {
                if (!Equals(obj1, obj2))
                    throw new AssertionException($"{(isList ? "List item" : "Property")} '{key}' mismatch. Expected: {obj1}, Actual: {obj2}");
            }

            try
            {
                var expando1 = new ExpandoObject();
                Utils.PopulateExpando(expando1, obj1);

                var expando2 = new ExpandoObject();
                Utils.PopulateExpando(expando2, obj2);

                var dict1 = (IDictionary<string, object>)expando1;
                var dict2 = (IDictionary<string, object>)expando2;

                foreach (var pair in dict1)
                {
                    if (!dict2.TryGetValue(pair.Key, out var value))
                    {
                        throw new AssertionException($"Property '{pair.Key}' not found in second object.");
                    }

                    if (pair.Value is IEnumerable enumerable1
                        && !(pair.Value is string)
                        && !(pair.Value is ExpandoObject)
                        && value is IEnumerable enumerable2
                        && !(value is string)
                        && !(value is ExpandoObject))
                    {
                        CompareEnumerables(key, enumerable1, enumerable2);
                    }
                    else
                    {
                        CompareObjects(pair.Key, pair.Value, value);
                    }
                }

                foreach (var k in dict2.Keys)
                {
                    if (!dict1.ContainsKey(k))
                    {
                        throw new AssertionException($"Property '{k}' exists in the second object but not in the first.");
                    }
                }
            }
            catch (InvalidCastException)
            {
                CompareWithEquals();
            }
            catch (TargetParameterCountException)
            {
                CompareWithEquals();
            }
            catch (ArgumentException)
            {
                CompareWithEquals();
            }
        }

        private static void CompareEnumerables(string key, IEnumerable enumerable1, IEnumerable enumerable2)
        {
            var enumerator1 = enumerable1.GetEnumerator();
            var enumerator2 = enumerable2.GetEnumerator();

            int idx = 0;
            while (enumerator1.MoveNext() && enumerator2.MoveNext())
            {
                CompareObjects($"{key ?? ""}[{idx}]", enumerator1.Current, enumerator2.Current, isList: true);
                idx++;
            }

            if (enumerator1.MoveNext() || enumerator2.MoveNext())
            {
                throw new AssertionException($"Enumerables have different lengths. Expected: {enumerable1.Cast<object>().Count()}, Actual: {enumerable2.Cast<object>().Count()}");
            }
        }
    }

    public class TestDeepDiff
    {
        [Test]
        public void Test_ObjectsAreEqual()
        {
            dynamic obj1 = new ExpandoObject();
            obj1.Name = "John";
            obj1.Age = 25;

            dynamic obj2 = new ExpandoObject();
            obj2.Name = "John";
            obj2.Age = 25;

            Assert.DoesNotThrow(() => DeepDiff.Diff(obj1, obj2));
        }

        [Test]
        public void Test_PropertyMismatch_ThrowsException()
        {
            dynamic obj1 = new ExpandoObject();
            obj1.Name = "John";
            obj1.Age = 25;

            dynamic obj2 = new ExpandoObject();
            obj2.Name = "John";
            obj2.Age = 30;

            var ex = Assert.Throws<AssertionException>(() => DeepDiff.Diff(obj1, obj2));
            ClassicAssert.AreEqual("Property 'Age' mismatch. Expected: 25, Actual: 30", ex.Message);
        }

        [Test]
        public void Test_PropertyExistsInOneObjectOnly_ThrowsException()
        {
            dynamic obj1 = new ExpandoObject();
            obj1.Name = "John";

            dynamic obj2 = new ExpandoObject();
            obj2.Name = "John";
            obj2.Age = 30;

            var ex = Assert.Throws<AssertionException>(() => DeepDiff.Diff(obj1, obj2));
            ClassicAssert.AreEqual("Property 'Age' exists in the second object but not in the first.", ex.Message);
        }

        [Test]
        public void Test_NestedExpandoObject_Mismatch_ThrowsException()
        {
            dynamic obj1 = new ExpandoObject();
            obj1.Name = "John";
            dynamic nested1 = new ExpandoObject();
            nested1.City = "New York";
            obj1.Details = nested1;

            dynamic obj2 = new ExpandoObject();
            obj2.Name = "John";
            dynamic nested2 = new ExpandoObject();
            nested2.City = "Los Angeles";
            obj2.Details = nested2;

            var ex = Assert.Throws<AssertionException>(() => DeepDiff.Diff(obj1, obj2));
            ClassicAssert.AreEqual("Property 'City' mismatch. Expected: New York, Actual: Los Angeles", ex.Message);
        }

        [Test]
        public void Test_NestedIEnumerable_Mismatch_ThrowsException()
        {
            dynamic obj1 = new ExpandoObject();
            obj1.Name = "John";
            obj1.Numbers = new List<int> { 1, 2, 3 };

            dynamic obj2 = new ExpandoObject();
            obj2.Name = "John";
            obj2.Numbers = new List<int> { 1, 2, 4 };

            var ex = Assert.Throws<AssertionException>(() => DeepDiff.Diff(obj1, obj2));
            ClassicAssert.AreEqual("List item '[2]' mismatch. Expected: 3, Actual: 4", ex.Message);
        }

        [Test]
        public void Test_NestedExpandoObjectInList_Mismatch_ThrowsException()
        {
            dynamic obj1 = new ExpandoObject();
            obj1.Name = "John";
            dynamic nestedItem1 = new ExpandoObject();
            nestedItem1.City = "New York";
            obj1.DetailsList = new List<ExpandoObject> { nestedItem1 };

            dynamic obj2 = new ExpandoObject();
            obj2.Name = "John";
            dynamic nestedItem2 = new ExpandoObject();
            nestedItem2.City = "Los Angeles";
            obj2.DetailsList = new List<ExpandoObject> { nestedItem2 };

            var ex = Assert.Throws<AssertionException>(() => DeepDiff.Diff(obj1, obj2));
            ClassicAssert.AreEqual("Property 'City' mismatch. Expected: New York, Actual: Los Angeles", ex.Message);
        }

        [Test]
        public void Test_DeepProperty_Mismatch_ThrowsException()
        {
            var ex = Assert.Throws<AssertionException>(() => DeepDiff.Diff(new
            {
                @object = "list",
                values = new[]
                        {
                            new
                            {
                                id = 1,
                                name = "Billy Bob",
                                email = "billy.bob@payload.com",
                            },
                            new
                            {
                                id = 2,
                                name = "Billy Bob",
                                email = "billy.bob@payload.com",
                            },
                            new
                            {
                                id = 3,
                                name = "Billy Bob",
                                email = "billy.bob@payload.com",
                            },
                        }
            }, new
            {
                @object = "list",
                values = new[]
                        {
                            new
                            {
                                id = 1,
                                name = "Billy Bob",
                                email = "billy.bob@payload.com",
                            },
                            new
                            {
                                id = 2,
                                name = "Billy Joe",
                                email = "billy.bob@payload.com",
                            },
                            new
                            {
                                id = 3,
                                name = "Billy Bob",
                                email = "billy.bob@payload.com",
                            },
                        }
            }));
            ClassicAssert.AreEqual("Property 'name' mismatch. Expected: Billy Bob, Actual: Billy Joe", ex.Message);
        }
    }
}
