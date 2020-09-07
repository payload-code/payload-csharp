using System;
using System.Dynamic;
using System.Linq;
using Payload.ARM;

namespace Payload {

	public static class pl {
		public const string URL = "https://api.payload.co";
		private static string _url = URL;
		public static string api_key { get; set; }
		public static string api_url { get { return _url; } set { _url = value; } }
		public static dynamic attr = new Attr(null);

		public static dynamic create(dynamic objects) {
			return new ARMRequest().create(objects);
		}

		public static dynamic update( dynamic objects ) {
			return new ARMRequest().update(objects);
		}

		public static dynamic delete( dynamic objects ) {
			return new ARMRequest().delete(objects);
		}

		public class Account : ARMObject<Account>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="account" };
			}
		}

		public class Customer : ARMObject<Customer>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="customer" };
			}
			public Customer(dynamic obj){Populate(obj);}
			public Customer(){Populate(new{});}

			public Payment charge( dynamic obj ) {
				dynamic data = new ExpandoObject();
				Utils.PopulateExpando( data, obj );
				data.customer_id = this["id"];
				return Payment.create(data);
			}
		}

		public class ProcessingAccount : ARMObject<ProcessingAccount>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="processing_account" };
			}
			public ProcessingAccount(dynamic obj){Populate(obj);}
			public ProcessingAccount(){Populate(new{});}
		}

		public class Org : ARMObject<Org>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="org", endpoint = "/accounts/orgs" };
			}
			public Org(dynamic obj){Populate(obj);}
			public Org(){Populate(new{});}
		}

		public class User : ARMObject<User>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="user" };
			}
			public User(dynamic obj){Populate(obj);}
			public User(){Populate(new{});}
		}

		public class Transaction : ARMObject<Transaction>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="transaction" };
			}
			public Transaction(dynamic obj){Populate(obj);}
			public Transaction(){Populate(new{});}
		}

		public class Payment : ARMObject<Payment>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="transaction", polymorphic=new { type="payment" } };
			}
			public Payment(dynamic obj){Populate(obj);}
			public Payment(){Populate(new{});}

			public Refund refund() {
				return pl.Refund.select("*", pl.attr.ledger ).create(new {
					amount=this["amount"],
					ledger=new[]{
						new pl.Ledger(new{ assoc_transaction_id=this["id"] })
					}
				});
			}
		}

		public class Refund : ARMObject<Refund>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="transaction", polymorphic=new { type="refund" } };
			}
			public Refund(dynamic obj){Populate(obj);}
			public Refund(){Populate(new{});}
		}

		public class Credit : ARMObject<Credit>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="transaction", polymorphic=new { type="credit" } };
			}
			public Credit(dynamic obj){Populate(obj);}
			public Credit(){Populate(new{});}
		}

		public class Deposit : ARMObject<Deposit>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="transaction", polymorphic=new { type="deposit" } };
			}
			public Deposit(dynamic obj){Populate(obj);}
			public Deposit(){Populate(new{});}
		}

		public class Ledger : ARMObject<Ledger>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="transaction_ledger" };
			}
			public Ledger(dynamic obj){Populate(obj);}
			public Ledger(){Populate(new{});}
		}

		public class PaymentMethod : ARMObject<PaymentMethod>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="payment_method" };
			}
			public PaymentMethod(dynamic obj){Populate(obj);}
			public PaymentMethod(){Populate(new{});}
		}

		public class Card : ARMObject<Card>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="payment_method", polymorphic=new { type="card" } };
			}
			public Card(dynamic obj){Populate(obj);}
			public Card(){Populate(new{});}

			public string card_number {
				get {
					return (string)((Dynamo)this["card"])
						.Properties["card_number"];
				}
				set {
					if (!this.Properties.ContainsKey("card"))
						this["card"] = new Dynamo();

					((Dynamo)this["card"])
						.Properties["card_number"] = value;
				}
			}

			public string card_code {
				get {
					return (string)((Dynamo)this["card"])
						.Properties["card_code"];
				}
				set {
					if (!this.Properties.ContainsKey("card"))
						this["card"] = new Dynamo();

					((Dynamo)this["card"])
						.Properties["card_code"] = value;
				}
			}

			public string expiry {
				get {
					return (string)((Dynamo)this["card"])
						.Properties["expiry"];
				}
				set {
					if (!this.Properties.ContainsKey("card"))
						this["card"] = new Dynamo();

					((Dynamo)this["card"])
						.Properties["expiry"] = value;
				}
			}


		}

		public class BankAccount : ARMObject<BankAccount>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="payment_method", polymorphic=new { type="bank_account" } };
			}
			public BankAccount(dynamic obj){Populate(obj);}
			public BankAccount(){Populate(new{});}

			public string account_number {
				get {
					return (string)((Dynamo)this["bank_account"])
						.Properties["account_number"];
				}
				set {
					if (!this.Properties.ContainsKey("bank_account"))
						this["bank_account"] = new Dynamo();

					((Dynamo)this["bank_account"])
						.Properties["account_number"] = value;
				}
			}

			public string routing_number {
				get {
					return (string)((Dynamo)this["bank_account"])
						.Properties["routing_number"];
				}
				set {
					if (!this.Properties.ContainsKey("bank_account"))
						this["bank_account"] = new Dynamo();

					((Dynamo)this["bank_account"])
						.Properties["routing_number"] = value;
				}
			}

			public string account_type {
				get {
					return (string)((Dynamo)this["bank_account"])
						.Properties["account_type"];
				}
				set {
					if (!this.Properties.ContainsKey("bank_account"))
						this["bank_account"] = new Dynamo();

					((Dynamo)this["bank_account"])
						.Properties["account_type"] = value;
				}
			}
		}

		public class BillingSchedule : ARMObject<BillingSchedule>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="billing_schedule" };
			}
			public BillingSchedule(dynamic obj){Populate(obj);}
			public BillingSchedule(){Populate(new{});}
		}

		public class BillingCharge : ARMObject<BillingCharge>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="billing_charge" };
			}
			public BillingCharge(dynamic obj){Populate(obj);}
			public BillingCharge(){Populate(new{});}
		}

		public class Invoice : ARMObject<Invoice>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="invoice" };
			}
			public Invoice(dynamic obj){Populate(obj);}
			public Invoice(){Populate(new{});}
		}

		public class LineItem : ARMObject<LineItem>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="line_item" };
			}
			public LineItem(dynamic obj){Populate(obj);}
			public LineItem(){Populate(new{});}
		}

		public class ChargeItem : ARMObject<ChargeItem>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="line_item", polymorphic=new { entry_type="charge" } };
			}
			public ChargeItem(dynamic obj){Populate(obj);}
			public ChargeItem(){Populate(new{});}
		}

		public class PaymentItem : ARMObject<PaymentItem>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="line_item", polymorphic=new { entry_type="payment" } };
			}
			public PaymentItem(dynamic obj){Populate(obj);}
			public PaymentItem(){Populate(new{});}
		}

		public class Webhook : ARMObject<Webhook>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="webhook" };
			}
			public Webhook(dynamic obj){Populate(obj);}
			public Webhook(){Populate(new{});}
		}

		public class PaymentLink : ARMObject<PaymentLink>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="payment_link" };
			}
			public PaymentLink(dynamic obj){Populate(obj);}
			public PaymentLink(){Populate(new{});}
		}

		public class OAuthToken : ARMObject<OAuthToken>, IARMObject {
			public override dynamic GetSpec() {
				return new { sobject="oauth_token", endpoint = "/oauth/token" };
			}
			public OAuthToken(dynamic obj){Populate(obj);}
			public OAuthToken(){Populate(new{});}
		}

		public class UnknownResponse : PayloadError {
			public UnknownResponse(){}
			public UnknownResponse(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class BadRequest : PayloadError {
			public override int GetCode() { return 400; }
			public BadRequest(){}
			public BadRequest(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class InvalidAttributes : PayloadError {
			public override int GetCode() { return 400; }
			public InvalidAttributes(){}
			public InvalidAttributes(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class TransactionDeclined : PayloadError {
			public override int GetCode() { return 400; }
			public TransactionDeclined(){}
			public TransactionDeclined(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class Unauthorized : PayloadError {
			public override int GetCode() { return 401; }
			public Unauthorized(){}
			public Unauthorized(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class NotPermitted : PayloadError {
			public override int GetCode() { return 403; }
			public NotPermitted(){}
			public NotPermitted(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class NotFound : PayloadError {
			public override int GetCode() { return 404; }
			public NotFound(){}
			public NotFound(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class TooManyRequests : PayloadError {
			public override int GetCode() { return 429; }
			public TooManyRequests(){}
			public TooManyRequests(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class InternalServerError : PayloadError {
			public override int GetCode() { return 500; }
			public InternalServerError(){}
			public InternalServerError(string message, ARMObject<object> response) : base(message, response) {}
		}

		public class ServiceUnavailable : PayloadError {
			public override int GetCode() { return 503; }
			public ServiceUnavailable(){}
			public ServiceUnavailable(string message, ARMObject<object> response) : base(message, response) {}
		}
	}
}
