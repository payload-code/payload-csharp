using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Payload.ARM;

namespace Payload
{

    public static partial class Payload
    {
        public partial class Session
        {
            public const string URL = "https://api.payload.co";
            private string _url = URL;
            public string ApiKey { get; set; }
            public string ApiUrl { get { return _url; } set { _url = value; } }

            public Session(string api_key)
            {
                ApiKey = api_key;
            }

            public override bool Equals(object obj)
            {
                return obj is Session session &&
                        ApiKey == session.ApiKey &&
                        ApiUrl == session.ApiUrl;
            }

            public async Task<List<T>> CreateAllAsync<T>(params T[] objects) where T : ARMObjectBase<T>
            {
                return await new ARMRequest<T>(this).CreateAllAsync(objects);
            }

            public async Task<List<T>> CreateAllAsync<T>(List<T> objects) where T : ARMObjectBase<T> => await CreateAllAsync(objects.ToArray());

            public List<T> CreateAll<T>(params T[] objects) where T : ARMObjectBase<T>
            {
                return CreateAllAsync(objects).GetAwaiter().GetResult();
            }

            public List<T> CreateAll<T>(List<T> objects) where T : ARMObjectBase<T> => CreateAllAsync(objects.ToArray()).GetAwaiter().GetResult();

            public async Task<T> CreateAsync<T>(T obj) where T : ARMObjectBase<T>
            {
                return await new ARMRequest<T>(this).CreateAsync(obj);
            }

            public T Create<T>(T obj) where T : ARMObjectBase<T>
            {
                return CreateAsync(obj).GetAwaiter().GetResult();
            }

            public ARMRequest<T> Select<T>(params object[] fields) where T : ARMObjectBase<T>
            {
                return new ARMRequest<T>(this).Select(fields);
            }

            public async Task<List<T>> UpdateAllAsync<T>((T, object)[] updates) where T : ARMObjectBase<T>
            {
                return await new ARMRequest<T>(this).UpdateAllAsync(updates);
            }

            public List<T> UpdateAll<T>((T, object)[] updates) where T : ARMObjectBase<T>
            {
                return UpdateAllAsync<T>(updates).GetAwaiter().GetResult();
            }

            public async Task<List<T>> DeleteAllAsync<T>(params T[] objects) where T : ARMObjectBase<T>
            {
                return await new ARMRequest<T>(this).DeleteAllAsync(objects);
            }

            public async Task<List<T>> DeleteAllAsync<T>(List<T> objects) where T : ARMObjectBase<T> => await DeleteAllAsync(objects.ToArray());

            public List<T> DeleteAll<T>(params T[] objects) where T : ARMObjectBase<T>
            {
                return DeleteAllAsync(objects).GetAwaiter().GetResult();
            }

            public List<T> DeleteAll<T>(List<T> objects) where T : ARMObjectBase<T> => DeleteAllAsync(objects.ToArray()).GetAwaiter().GetResult();

            public async Task<T> DeleteAsync<T>(T obj) where T : ARMObjectBase<T>
            {
                return await new ARMRequest<T>(this).DeleteAsync(obj);
            }

            public T Delete<T>(T objects) where T : ARMObjectBase<T>
            {
                return DeleteAsync(objects).GetAwaiter().GetResult();
            }

            public override int GetHashCode()
            {
                int hashCode = -1954838531;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_url);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ApiKey);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(ApiUrl);
                return hashCode;
            }
        }
    }

    public static partial class pl
    {

        public static string ApiKey { get { return DefaultSession.ApiKey; } set { DefaultSession.ApiKey = value; } }
        public static string ApiUrl { get { return DefaultSession.ApiUrl; } set { DefaultSession.ApiUrl = value; } }
        public static dynamic Attr = new Attr(null);
        public static Payload.Session DefaultSession = new Payload.Session(null);

        public static async Task<List<T>> CreateAllAsync<T>(IEnumerable<T> objects) where T : ARMObjectBase<T>
        {
            return await new ARMRequest<T>(DefaultSession).CreateAllAsync(objects);
        }

        public static List<T> CreateAll<T>(IEnumerable<T> objects) where T : ARMObjectBase<T>
        {
            return CreateAllAsync(objects).GetAwaiter().GetResult();
        }

        public static async Task<T> CreateAsync<T>(T obj) where T : ARMObjectBase<T>
        {
            return await new ARMRequest<T>(DefaultSession).CreateAsync(obj);
        }

        public static T Create<T>(T obj) where T : ARMObjectBase<T>
        {
            return CreateAsync(obj).GetAwaiter().GetResult();
        }

        public static async Task<List<T>> UpdateAllAsync<T>((T, object)[] objects) where T : ARMObjectBase<T>
        {
            return await new ARMRequest<T>(DefaultSession).UpdateAllAsync(objects);
        }

        public static List<T> UpdateAll<T>(params (T, object)[] objects) where T : ARMObjectBase<T>
        {
            return UpdateAllAsync(objects).GetAwaiter().GetResult();
        }

        public static async Task<T> DeleteAsync<T>(T obj) where T : ARMObjectBase<T>
        {
            return await new ARMRequest<T>(DefaultSession).DeleteAsync(obj);
        }

        public static T Delete<T>(T obj) where T : ARMObjectBase<T>
        {
            return DeleteAsync(obj).GetAwaiter().GetResult();
        }

        public class DefaultParams
        {
            public object[] Fields { get; set; } = new object[0];
        }

        public class ARMObject : ARMObjectBase<ARMObject>
        {
            public ARMObject(object obj) : base(obj) { }
            public ARMObject() : base() { }
        }

        public class AccessToken : ARMObjectBase<AccessToken>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "access_token"
            };
            public AccessToken(object obj) : base(obj) { }
            public AccessToken() : base() { }
        }

        public class ClientToken : ARMObjectBase<ClientToken>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "access_token",
                Polymorphic = new ARMObject(new { type = "client" })
            };
            public ClientToken(object obj) : base(obj) { }
            public ClientToken() : base() { }
        }

        public class Account : ARMObjectBase<Account>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "account"
            };
            public Account(object obj) : base(obj) { }
            public Account() : base() { }
        }

        public class Customer : ARMObjectBase<Customer>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "customer"
            };
            public Customer(object obj) : base(obj) { }
            public Customer() : base() { }

            public async Task<Payment> ChargeAsync(dynamic obj)
            {
                dynamic data = new ExpandoObject();
                Utils.PopulateExpando(data, obj);
                data.customer_id = this["id"];
                return await Payment.CreateAsync(data);
            }

            public Payment Charge(dynamic obj)
            {
                return ChargeAsync(obj).GetAwaiter().GetResult();
            }
        }

        public class ProcessingAccount : ARMObjectBase<ProcessingAccount>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "processing_account"
            };
            public ProcessingAccount(object obj) : base(obj) { }
            public ProcessingAccount() : base() { }
        }

        public class Org : ARMObjectBase<Org>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "org"
            };
            public Org(object obj) : base(obj) { }
            public Org() : base() { }
        }

        public class User : ARMObjectBase<User>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "user"
            };
            public User(object obj) : base(obj) { }
            public User() : base() { }
        }

        public class Transaction : ARMObjectBase<Transaction>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "transaction"
            };
            public Transaction(object obj) : base(obj) { }
            public Transaction() : base() { }
        }

        public class Payment : ARMObjectBase<Payment>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "transaction",
                Polymorphic = new ARMObject(new { type = "payment" })
            };
            public Payment(object obj) : base(obj) { }
            public Payment() : base() { }

            public async Task<Refund> RefundAsync()
            {
                return await pl.Refund
                    .Select(new[] { "*", Attr.ledger })
                    .CreateAsync(new
                    {
                        amount = this["amount"],
                        ledger = new[] {
                            new Ledger(new{ assoc_transaction_id=this["id"] })
                        }
                    });
            }

            public Refund Refund() => RefundAsync().GetAwaiter().GetResult();
        }

        public class Refund : ARMObjectBase<Refund>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "transaction",
                Polymorphic = new ARMObject(new { type = "refund" })
            };
            public Refund(object obj) : base(obj) { }
            public Refund() : base() { }
        }

        public class Credit : ARMObjectBase<Credit>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "transaction",
                Polymorphic = new ARMObject(new { type = "credit" })
            };
            public Credit(object obj) : base(obj) { }
            public Credit() : base() { }
        }

        public class Deposit : ARMObjectBase<Deposit>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "transaction",
                Polymorphic = new ARMObject(new { type = "deposit" })
            };
            public Deposit(object obj) : base(obj) { }
            public Deposit() : base() { }
        }

        public class Ledger : ARMObjectBase<Ledger>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "transaction_ledger"
            };
            public Ledger(object obj) : base(obj) { }
            public Ledger() : base() { }
        }

        public class PaymentMethod : ARMObjectBase<PaymentMethod>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "payment_method"
            };
            public PaymentMethod(object obj) : base(obj) { }
            public PaymentMethod() : base() { }

            public bool IsCard(out Card card)
            {
                if ((string)this["type"] == "card")
                {
                    card = new Card(this);
                }
                else
                {
                    card = null;
                }

                return card != null;
            }

            public bool IsBankAccount(out BankAccount bankAccount)
            {
                if ((string)this["type"] == "bank_account")
                {
                    bankAccount = new BankAccount(this);
                }
                else
                {
                    bankAccount = null;
                }

                return bankAccount != null;
            }
        }

        public class Card : ARMObjectBase<Card>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "payment_method",
                Polymorphic = new ARMObject(new { type = "card" })
            };
            public Card(object obj) : base(obj) { }
            public Card() : base() { }

            public string card_number
            {
                get
                {
                    return GetObject("card").GetProperty<string>("card_number");
                }
                set
                {
                    if (!HasObject("card"))
                        this["card"] = new Dynamo();

                    GetObject("card")["card_number"] = value;
                }
            }

            public string card_code
            {
                get
                {
                    return GetObject("card").GetProperty<string>("card_code");
                }
                set
                {
                    if (!HasObject("card"))
                        this["card"] = new Dynamo();

                    GetObject("card")["card_code"] = value;
                }
            }

            public string expiry
            {
                get
                {
                    return GetObject("card").GetProperty<string>("expiry");
                }
                set
                {
                    if (!HasObject("card"))
                        this["card"] = new Dynamo();

                    GetObject("card")["expiry"] = value;
                }
            }


        }

        public class BankAccount : ARMObjectBase<BankAccount>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "payment_method",
                Polymorphic = new ARMObject(new { type = "bank_account" })
            };
            public BankAccount(object obj) : base(obj) { }
            public BankAccount() : base() { }

            public string account_number
            {
                get
                {
                    return GetObject("bank_account").GetProperty<string>("account_number");
                }
                set
                {
                    if (!HasObject("bank_account"))
                        this["bank_account"] = new Dynamo();

                    GetObject("bank_account")["account_number"] = value;
                }
            }

            public string routing_number
            {
                get
                {
                    return GetObject("bank_account").GetProperty<string>("routing_number");
                }
                set
                {
                    if (!HasObject("bank_account"))
                        this["bank_account"] = new Dynamo();

                    GetObject("bank_account")["routing_number"] = value;
                }
            }

            public string account_type
            {
                get
                {
                    return GetObject("bank_account").GetProperty<string>("account_type");
                }
                set
                {
                    if (!HasObject("bank_account"))
                        this["bank_account"] = new Dynamo();

                    GetObject("bank_account")["account_type"] = value;
                }
            }
        }

        public class BillingSchedule : ARMObjectBase<BillingSchedule>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "billing_schedule"
            };
            public BillingSchedule(object obj) : base(obj) { }
            public BillingSchedule() : base() { }
        }

        public class BillingCharge : ARMObjectBase<BillingCharge>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "billing_charge"
            };
            public BillingCharge(object obj) : base(obj) { }
            public BillingCharge() : base() { }
        }

        public class Invoice : ARMObjectBase<Invoice>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "invoice"
            };
            public Invoice(object obj) : base(obj) { }
            public Invoice() : base() { }
        }

        public class LineItem : ARMObjectBase<LineItem>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "line_item"
            };
            public LineItem(object obj) : base(obj) { }
            public LineItem() : base() { }
        }

        public class ChargeItem : ARMObjectBase<ChargeItem>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "line_item",
                Polymorphic = new ARMObject(new { entry_type = "charge" })
            };
            public ChargeItem(object obj) : base(obj) { }
            public ChargeItem() : base() { }
        }

        public class PaymentItem : ARMObjectBase<PaymentItem>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "line_item",
                Polymorphic = new ARMObject(new { entry_type = "payment" })
            };
            public PaymentItem(object obj) : base(obj) { }
            public PaymentItem() : base() { }
        }

        public class Webhook : ARMObjectBase<Webhook>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "webhook"
            };
            public Webhook(object obj) : base(obj) { }
            public Webhook() : base() { }
        }

        public class PaymentLink : ARMObjectBase<PaymentLink>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "payment_link"
            };
            public PaymentLink(object obj) : base(obj) { }
            public PaymentLink() : base() { }
        }

        public class OAuthToken : ARMObjectBase<OAuthToken>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "oauth_token"
            };
            public OAuthToken(object obj) : base(obj) { }
            public OAuthToken() : base() { }
        }

        public class PaymentActivation : ARMObjectBase<PaymentActivation>
        {
            public override ARMObjectSpec GetSpec() => new ARMObjectSpec
            {
                Object = "payment_activation"
            };
            public PaymentActivation(object obj) : base(obj) { }
            public PaymentActivation() : base() { }
        }

        public class UnknownResponse : PayloadError
        {
            public UnknownResponse() { }
            public UnknownResponse(string message, JSONObject response) : base(message, response) { }
        }

        public class BadRequest : PayloadError
        {
            public override int GetCode() { return 400; }
            public BadRequest() { }
            public BadRequest(string message, JSONObject response) : base(message, response) { }
        }

        public class InvalidAttributes : PayloadError
        {
            public override int GetCode() { return 400; }
            public InvalidAttributes() { }
            public InvalidAttributes(string message, JSONObject response) : base(message, response) { }
        }

        public class TransactionDeclined : PayloadError
        {
            public override int GetCode() { return 400; }
            public TransactionDeclined() { }
            public TransactionDeclined(string message, JSONObject response) : base(message, response) { }
        }

        public class Unauthorized : PayloadError
        {
            public override int GetCode() { return 401; }
            public Unauthorized() { }
            public Unauthorized(string message, JSONObject response) : base(message, response) { }
        }

        public class NotPermitted : PayloadError
        {
            public override int GetCode() { return 403; }
            public NotPermitted() { }
            public NotPermitted(string message, JSONObject response) : base(message, response) { }
        }

        public class NotFound : PayloadError
        {
            public override int GetCode() { return 404; }
            public NotFound() { }
            public NotFound(string message, JSONObject response) : base(message, response) { }
        }

        public class TooManyRequests : PayloadError
        {
            public override int GetCode() { return 429; }
            public TooManyRequests() { }
            public TooManyRequests(string message, JSONObject response) : base(message, response) { }
        }

        public class InternalServerError : PayloadError
        {
            public override int GetCode() { return 500; }
            public InternalServerError() { }
            public InternalServerError(string message, JSONObject response) : base(message, response) { }
        }

        public class ServiceUnavailable : PayloadError
        {
            public override int GetCode() { return 503; }
            public ServiceUnavailable() { }
            public ServiceUnavailable(string message, JSONObject response) : base(message, response) { }
        }
    }
}
