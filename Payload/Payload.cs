using System;
using Payload.ARM;

namespace Payload {

	public static class pl {
		public const string URL = "https://api.payload.co";
		private static string _url = URL;
		public static string api_key { get; set; }
		public static string api_url { get { return _url; } set { _url = value; } }
		public static dynamic attr = new Attr("");

		public class Account : ARMObject<Account>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="account" };
			}
		}

		public class Customer : ARMObject<Customer>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="account", polymorphic=new { type="customer" } };
			}
			public Customer(dynamic obj){Populate(obj);}
			public Customer(){}
		}

		public class ProcessingAccount : ARMObject<ProcessingAccount>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="account", polymorphic=new { type="processing" } };
			}
			public ProcessingAccount(dynamic obj){Populate(obj);}
			public ProcessingAccount(){}
		}

		public class Org : ARMObject<Org>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="org", endpoint = "/accounts/orgs" };
			}
			public Org(dynamic obj){Populate(obj);}
			public Org(){}
		}

		public class User : ARMObject<User>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="user" };
			}
			public User(dynamic obj){Populate(obj);}
			public User(){}
		}

		public class Transaction : ARMObject<Transaction>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="transaction" };
			}
			public Transaction(dynamic obj){Populate(obj);}
			public Transaction(){}
		}

		public class Payment : ARMObject<Payment>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="transaction", polymorphic=new { type="payment" } };
			}
			public Payment(dynamic obj){Populate(obj);}
			public Payment(){}
		}

		public class Refund : ARMObject<Refund>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="transaction", polymorphic=new { type="refund" } };
			}
			public Refund(dynamic obj){Populate(obj);}
			public Refund(){}
		}

		public class Ledger : ARMObject<Ledger>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="transaction_ledger" };
			}
			public Ledger(dynamic obj){Populate(obj);}
			public Ledger(){}
		}

		public class PaymentMethod : ARMObject<PaymentMethod>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="payment_method" };
			}
			public PaymentMethod(dynamic obj){Populate(obj);}
			public PaymentMethod(){}
		}

		public class Card : ARMObject<Card>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="payment_method", polymorphic=new { type="card" } };
			}
			public Card(dynamic obj){Populate(obj);}
			public Card(){}
		}

		public class BankAccount : ARMObject<BankAccount>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="payment_method", polymorphic=new { type="bank_account" } };
			}
			public BankAccount(dynamic obj){Populate(obj);}
			public BankAccount(){}
		}

		public class BillingSchedule : ARMObject<BillingSchedule>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="billing_schedule" };
			}
			public BillingSchedule(dynamic obj){Populate(obj);}
			public BillingSchedule(){}
		}

		public class BillingCharge : ARMObject<BillingCharge>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="billing_charge" };
			}
			public BillingCharge(dynamic obj){Populate(obj);}
			public BillingCharge(){}
		}

		public class Invoice : ARMObject<Invoice>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="invoice" };
			}
			public Invoice(dynamic obj){Populate(obj);}
			public Invoice(){}
		}

		public class LineItem : ARMObject<LineItem>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="line_item" };
			}
			public LineItem(dynamic obj){Populate(obj);}
			public LineItem(){}
		}

		public class ChargeItem : ARMObject<ChargeItem>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="line_item", polymorphic=new { type="charge" } };
			}
			public ChargeItem(dynamic obj){Populate(obj);}
			public ChargeItem(){}
		}

		public class PaymentItem : ARMObject<PaymentItem>, IARMObject {
			public new dynamic GetSpec() {
				return new { sobject="line_item", polymorphic=new { type="payment" } };
			}
			public PaymentItem(dynamic obj){Populate(obj);}
			public PaymentItem(){}
		}
	}
}
