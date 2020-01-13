using System;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Net;
using System.Text;
using System.IO;
using System.Reflection;
using System.Dynamic;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Payload;

namespace PayloadTests
{
    class Program
    {
        static void Main(string[] args)
        {

		pl.api_key = "your_secret_key_2zsp9QrpbFVsWola7W1BTs";

		Console.WriteLine ("Payload Test");


		test_general_api_calls();
		test_payment_calls();
		test_invoice_calls();

	}

	public static void objMatch( dynamic input, dynamic output ) {
		PropertyInfo[] properties = input.GetType().GetProperties();
		foreach (PropertyInfo pi in properties) {
			var val = output[pi.Name];
			if (val is IList<dynamic>) {
				for( int i = 0; i < val.Count; i++ ) {
					objMatch( pi.GetValue(input,null)[i], val[i] );
				}
			} else if (pi.GetValue(input,null) == null && val == null ){
				continue;
			} else if ( pi.GetValue(input,null).GetType().Name.Contains("AnonymousType") ) {
				objMatch( pi.GetValue(input,null), val );
			} else if (!pi.GetValue(input,null).Equals(Convert.ChangeType(val,pi.GetValue(input,null).GetType())) ){
				throw new System.Exception("Invalid result: "+pi.Name+": "+val+" != "+pi.GetValue(input,null));
			}
		}

	}

	static public void test_general_api_calls() {
		Console.Write("test_attr...");
		test_attr();
		Console.WriteLine("passed");

		Console.Write("test_create_cust...");
		test_create_cust();
		Console.WriteLine("passed");

		Console.Write("test_create_cust_subscript...");
		test_create_cust_subscript();
		Console.WriteLine("passed");

		Console.Write("test_create_cust_poly...");
		test_create_cust_poly();
		Console.WriteLine("passed");

		Console.Write("test_multi_create...");
		test_multi_create();
		Console.WriteLine("passed");

		Console.Write("test_multi_create_insts...");
		test_multi_create_insts();
		Console.WriteLine("passed");

		Console.Write("test_multi_create_insts2...");
		test_multi_create_insts2();
		Console.WriteLine("passed");

		Console.Write("test_select_cust...");
		test_select_cust();
		Console.WriteLine("passed");

		Console.Write("test_get_cust...");
		test_get_cust();
		Console.WriteLine("passed");

		Console.Write("test_get_accnt...");
		test_get_accnt();
		Console.WriteLine("passed");

		Console.Write("test_bad_get...");
		test_bad_get();
		Console.WriteLine("passed");

		Console.Write("test_update_cust...");
		test_update_cust();
		Console.WriteLine("passed");

		Console.Write("test_update_cust_multi...");
		test_update_cust_multi();
		Console.WriteLine("passed");

		Console.Write("test_update_cust_multi2...");
		test_update_cust_multi2();
		Console.WriteLine("passed");

		Console.Write("test_update_cust_query...");
		test_update_cust_query();
		Console.WriteLine("passed");

		Console.Write("test_del_cust...");
		test_del_cust();
		Console.WriteLine("passed");

		Console.Write("test_del_cust_query...");
		test_del_cust_query();
		Console.WriteLine("passed");

		Console.Write("test_json...");
		test_json();
		Console.WriteLine("passed");

	}

	public static void test_payment_calls() {
		Console.Write("test_card_payment...");
		test_card_payment();
		Console.WriteLine("passed");

		Console.Write("test_card_payment2...");
		test_card_payment2();
		Console.WriteLine("passed");

		Console.Write("test_bank_account_payment...");
		test_bank_account_payment();
		Console.WriteLine("passed");

		Console.Write("test_no_card_on_file...");
		test_no_card_on_file();
		Console.WriteLine("passed");

		Console.Write("test_card_on_file...");
		test_card_on_file();
		Console.WriteLine("passed");

		Console.Write("test_longform_refund...");
		test_longform_refund();
		Console.WriteLine("passed");


		Console.Write("test_shortform_refund...");
		test_shortform_refund();
		Console.WriteLine("passed");


	}

	public static void test_invoice_calls() {
		Console.Write("test_create_invoice...");
		test_create_invoice();
		Console.WriteLine("passed");

		Console.Write("test_pay_invoice...");
		test_pay_invoice();
		Console.WriteLine("passed");

		Console.Write("test_create_billing_schedule...");
		test_create_billing_schedule();
		Console.WriteLine("passed");
	}

	public static void test_create_invoice() {
		var vals = new {
			input=new {
				email="test2@gmail.com",
				name="Test Account"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var cust = pl.Customer.create(vals.input);

		List<pl.ChargeItem> items = new List<pl.ChargeItem>();

		items.Add(new pl.ChargeItem(new {amount=100, description="membership"}));


		var invoice = pl.Invoice.create(new {
			due_date=new DateTime(2019,5,1),
			items=items.ToArray(),
			/*new []{
				new pl.ChargeItem(new{amount=100, description="membership"}),
				new pl.ChargeItem(new{amount=100, description="membership"})
			},*/
			customer_id=cust.id
		});

		if (invoice.GetType() != typeof(pl.Invoice))
			throw new System.Exception("Invalid type");

		objMatch( new{
			customer_id=cust.id,
			items=new[]{
				new {
					amount=100.0,
					description="membership"
				}
			}
		}, invoice );
		Console.Write("+");
	}

	public static void test_pay_invoice() {
		var vals = new {
			input=new {
				email="test2@gmail.com",
				name="Test Account"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var cust = pl.Customer.create(vals.input);

		var card_on_file = pl.Card.create(new{
				account_id=cust.id,
				card_number="4242 4242 4242 4242",
				auto_billing_enabled=true
			});

		var invoice = pl.Invoice.create(new {
			due_date=new DateTime(2019,5,1),
			items=new []{
				new pl.ChargeItem(new{amount=100, description="membership"})
			},
			customer_id=cust.id
		});


		if (invoice.GetType() != typeof(pl.Invoice))
			throw new System.Exception("Invalid type");

		objMatch( new{
			customer_id=cust.id,
			items=new[]{
				new {
					amount=100.0,
					description="membership"
				}
			}
		}, invoice );
		Console.Write("+");

		if ( invoice.status != "paid" ) {
			var payment = pl.Payment.create(new {
				amount=invoice.amount_due,
				customer_id=invoice.customer_id,
				allocations=new[]{
					new pl.PaymentItem(new{
						invoice_id=invoice.id
					})
				}
			});

			if (payment.GetType() != typeof(pl.Payment))
				throw new System.Exception("Invalid type");

			objMatch( new{
				customer_id=cust.id,
				payment_method_id=card_on_file.id,
				processing_id=invoice.processing_id
			}, payment );
			Console.Write("+");
		}
	}

	public static void test_create_billing_schedule() {
		var vals = new {
			input=new {
				email="test2@gmail.com",
				name="Test Account"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var cust = pl.Customer.create(vals.input);

		var billing_schedule = pl.BillingSchedule.create(new {
			start_date="2019-01-01",
			end_date="2019-12-31",
			recurring_frequency="monthly",
			type="subscription",
			charges=new[]{
				new pl.BillingCharge(new{
					type="option_1",
					amount=39.99
				})
			},
			customer_id=cust.id
		});

		if (billing_schedule.GetType() != typeof(pl.BillingSchedule))
			throw new System.Exception("Invalid type");

		objMatch( new{
			customer_id=cust.id,
			charges=new[]{
				new {
					amount=39.99,
					type="option_1"
				}
			}
		}, billing_schedule );
		Console.Write("+");
	}

	public static void test_card_payment() {
		var payment = pl.Payment.create(new {
			amount=100.0,
			payment_method=new pl.Card(new{
				card=new {card_number="4242 4242 4242 4242"}
			})
		});

		var vals = new{
			amount=100.0,
			payment_method=new{
				card=new{
					card_number="xxxxxxxxxxxx4242"
				}
			}
		};

		if (payment.GetType() != typeof(pl.Payment))
			throw new System.Exception("Invalid type");

		objMatch( vals, payment );
		Console.Write("+");
	}

	public static void test_card_payment2() {
		var payment = pl.Payment.create(new {
			amount=100.0,
			payment_method=new pl.Card(new{
				card_number="4242 4242 4242 4242"
			})
		});

		var vals = new{
			amount=100.0,
			payment_method=new{
				card_number="xxxxxxxxxxxx4242"
			}
		};

		if (payment.GetType() != typeof(pl.Payment))
			throw new System.Exception("Invalid type");

		objMatch( vals, payment );
		Console.Write("+");
	}

	public static void test_bank_account_payment() {
		var payment = pl.Payment.create(new {
			amount=100.0,
			payment_method=new pl.BankAccount(new{
				account_number="1234567890",
				routing_number="021000121",
				account_type="checking"
			})
		});

		var vals = new{
			amount=100.0,
			payment_method=new{
				account_number="xxxxxx7890",
				routing_number="xxxxx0121",
				account_type="checking"
			}
		};

		if (payment.GetType() != typeof(pl.Payment))
			throw new System.Exception("Invalid type");

		objMatch( vals, payment );
		Console.Write("+");
	}

	public static void test_no_card_on_file() {
		var vals = new {
			input=new {
				email="test2@gmail.com",
				name="Test Account"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var cust = pl.Customer.create(vals.input);

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals.output, cust );
		Console.Write("+");

		bool exc = false;
		try {
			cust.charge(new{ amount=100 });
		} catch(pl.InvalidAttributes e) {
			Console.WriteLine(e.json());
			Console.WriteLine(e.details["payment_method_id"]);

			var found = false;
			foreach (JProperty property in e.details) {
				if ( property.Name.Equals("payment_method_id") )
					found = true;
				else
					throw new System.Exception("Invalid error");
			}
			if (!found)
					throw new System.Exception("Invalid error");

			exc = true;
		}

		if ( !exc )
			throw new Exception("No exception raised!");
	}

	public static void test_card_on_file() {
		var vals = new {
			input=new {
				email="test2@gmail.com",
				name="Test Account"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var cust = pl.Customer.create(vals.input);

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals.output, cust );
		Console.Write("+");

		var card_on_file = pl.Card.create(new{
				account_id=cust.id,
				card_number="4242 4242 4242 4242",
				auto_billing_enabled=true
			});

		var payment = cust.charge(new{ amount=100 });

		var output=new {
				amount=100.0,
				customer_id=cust.id,
				payment_method_id=card_on_file.id,
			};

		if (payment.GetType() != typeof(pl.Payment))
			throw new System.Exception("Invalid type");

		objMatch( output, payment );
		Console.Write("+");
	}

	public static void test_longform_refund() {
		var payment = pl.Payment.create(new {
			amount=100.0,
			payment_method=new pl.Card(new{
				card_number="4242 4242 4242 4242"
			})
		});

		var refund = pl.Refund.select("*", pl.attr.ledger ).create(new {
			amount=payment.amount,
			ledger=new[]{
				new pl.Ledger(new{ assoc_transaction_id=payment.id })
			}
		});

		if (refund.GetType() != typeof(pl.Refund))
			throw new System.Exception("Invalid type");

		var vals = new{
			amount=100.0,
			customer_id=payment.customer_id,
			payment_method_id=payment.payment_method_id,
			ledger=new[]{
				new {assoc_transaction_id=payment.id}
			}
		};
		objMatch( vals, refund );
		Console.Write("+");
	}

	public static void test_shortform_refund() {
		var payment = pl.Payment.create(new {
			amount=100.0,
			payment_method=new pl.Card(new{
				card_number="4242 4242 4242 4242"
			})
		});

		var refund = payment.refund();

		if (refund.GetType() != typeof(pl.Refund))
			throw new System.Exception("Invalid type");

		var vals = new{
			amount=100.0,
			customer_id=payment.customer_id,
			payment_method_id=payment.payment_method_id,
			ledger=new[]{
				new {assoc_transaction_id=payment.id}
			}
		};

		objMatch( vals, refund );
		Console.Write("+");
	}

	public static void test_attr() {

		if(!pl.attr.created_date.ToString().Equals("created_date"))
			throw new System.Exception("Invalid attr");
		if(!pl.attr.created_date.date().ToString().Equals("date(created_date)"))
			throw new System.Exception("Invalid attr");
		if(!pl.attr.invoice.created_date.ToString().Equals("invoice[created_date]"))
			throw new System.Exception("Invalid attr");
		if(!pl.attr.invoice.created_date.date().ToString().Equals("date(invoice[created_date])"))
			throw new System.Exception("Invalid attr");
	}

	public static void test_create_cust() {
		var vals = new {
			email="test@gmail.com",
			name="Test Account",
			type="customer"
		};

		var cust = pl.Account.create(vals);

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals, cust );
		Console.Write("+");
	}

	public static void test_create_cust_subscript() {
		var vals = new {
			email="test@gmail.com",
			name="Test Account",
			type="customer"
		};

		var cust = new pl.Customer();
		cust["email"] = "test@gmail.com";
		cust["name"] = "Test Account";

		var output = pl.create(new []{cust});

		if (output[0].GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals, output[0] );
		Console.Write("+");
	}

	public static void test_create_cust_poly() {
		var vals = new {
			input=new {
				email="test2@gmail.com",
				name="Test Account"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var cust = pl.Customer.create(vals.input);

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals.output, cust );
		Console.Write("+");
	}

	public static void test_multi_create() {
		var vals = new {
			input=new []{
				new {
					email="test3@gmail.com",
					name="Test Account"
				},
				new {
					email="test4@gmail.com",
					name="Test Account"
				}
			},
			output=new []{
				new {
					email="test3@gmail.com",
					name="Test Account",
					type="customer"
				},
				new {
					email="test4@gmail.com",
					name="Test Account",
					type="customer"
				}
			}
		};

		var custs = pl.Customer.create(vals.input);

		int i = 0;
		foreach( var cust in custs ) {
			if (cust.GetType() != typeof(pl.Customer))
				throw new System.Exception("Invalid type");
			objMatch( vals.output[i++], cust );
			Console.Write("+");
		}

	}

	public static void test_multi_create_insts() {
		var vals = new {
			input=new []{
				new pl.Customer(new {
					email="test3@gmail.com",
					name="Test Account"
				}),
				new pl.Customer(new {
					email="test4@gmail.com",
					name="Test Account"
				})
			},
			output=new []{
				new {
					email="test3@gmail.com",
					name="Test Account",
					type="customer"
				},
				new {
					email="test4@gmail.com",
					name="Test Account",
					type="customer"
				}
			}
		};

		var custs = pl.create(vals.input);

		int i = 0;
		foreach( var cust in custs ) {
			if (cust.GetType() != typeof(pl.Customer))
				throw new System.Exception("Invalid type");
			objMatch( vals.output[i++], cust );
			Console.Write("+");
		}

	}

	public static void test_multi_create_insts2() {
		var vals = new {
			input=new []{
				new pl.Customer(new{
					email="test3@gmail.com",
					name="Test Account"
				}),
				new pl.Customer(new{
					email="test4@gmail.com",
					name="Test Account"
				})
			},
			output=new []{
				new {
					email="test3@gmail.com",
					name="Test Account",
					type="customer"
				},
				new {
					email="test4@gmail.com",
					name="Test Account",
					type="customer"
				}
			}
		};

		var custs = pl.Customer.create(vals.input);

		int i = 0;
		foreach( var cust in custs ) {
			if (cust.GetType() != typeof(pl.Customer))
				throw new System.Exception("Invalid type");
			objMatch( vals.output[i++], cust );
			Console.Write("+");
		}

	}

	public static void test_select_cust() {
		var vals = new {
			input=new {
				email="test2@gmail.com"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var custs = pl.Customer.filter_by(vals.input).all();

		var cust = custs[custs.Count - 1];

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals.output, cust );
		Console.Write("+");
	}

	public static void test_get_cust() {
		var vals = new {
			input=new {
				email="test2@gmail.com"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};
		var custs = pl.Customer.filter_by(vals.input).all();
		var cust = pl.Customer.get(custs[custs.Count - 1].id);

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals.output, cust );
		Console.Write("+");
	}


	public static void test_get_accnt() {
		var vals = new {
			input=new {
				email="test2@gmail.com"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};
		var custs = pl.Customer.filter_by(vals.input).all();
		var cust = pl.Account.get(custs[custs.Count - 1].id);

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals.output, cust );
		Console.Write("+");
	}

	public static void test_bad_get() {
		try {
			pl.Customer.get(null);
			throw new Exception("Get should have thrown error");
		} catch ( ArgumentNullException e ) {
			//this is the error we expected
		}
	}

	public static void test_update_cust() {
		var vals = new {
			input=new {
				email="test2@gmail.com",
				name="Test Account"
			},
			output=new {
				email="test3@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var cust = pl.Customer.create(vals.input);

		cust.update(new { email=vals.output.email });

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals.output, cust );
		Console.Write("+");

	}

	public static void test_update_cust_multi() {
		var vals = new {
			input=new []{
				new {
					email="test3@gmail.com",
					name="Test Account"
				},
				new {
					email="test4@gmail.com",
					name="Test Account"
				}
			},
			output=new []{
				new {
					email="test5@gmail.com",
					name="Test Account",
					type="customer"
				},
				new {
					email="test6@gmail.com",
					name="Test Account",
					type="customer"
				}
			}
		};

		var custs = pl.Customer.create(vals.input);

		int i = 0;
		var updates = new List<dynamic>();
		foreach( var cust in custs )
			updates.Add(new object[]{ cust, new { email=vals.output[i++].email } });

		pl.Customer.update_all(updates);

		i = 0;
		foreach( var cust in custs ) {
			if (cust.GetType() != typeof(pl.Customer))
				throw new System.Exception("Invalid type");
			objMatch( vals.output[i++], cust );
			Console.Write("+");
		}
	}

	public static void test_update_cust_multi2() {
		var vals = new {
			input=new []{
				new {
					email="test3@gmail.com",
					name="Test Account"
				},
				new {
					email="test4@gmail.com",
					name="Test Account"
				}
			},
			output=new []{
				new {
					email="test5@gmail.com",
					name="Test Account",
					type="customer"
				},
				new {
					email="test6@gmail.com",
					name="Test Account",
					type="customer"
				}
			}
		};

		var custs = pl.Customer.create(vals.input);

		int i = 0;
		var updates = new List<dynamic>();
		foreach( var cust in custs )
			updates.Add(new object[]{ cust, new { email=vals.output[i++].email } });

		pl.update(updates);

		i = 0;
		foreach( var cust in custs ) {
			if (cust.GetType() != typeof(pl.Customer))
				throw new System.Exception("Invalid type");
			objMatch( vals.output[i++], cust );
			Console.Write("+");
		}
	}

	public static void test_update_cust_query() {
		var vals = new {
			input=new []{
				new {
					email="test55@gmail.com",
					name="Test Account"
				},
				new {
					email="test55@gmail.com",
					name="Test Account"
				}
			},
			output=new []{
				new {
					email="test56@gmail.com",
					name="Test Account",
					type="customer"
				},
				new {
					email="test56@gmail.com",
					name="Test Account",
					type="customer"
				}
			}
		};

		var custs = pl.Customer.create(vals.input);

		custs = pl.Customer.filter_by(new { email="test55@gmail.com" })
			.update(new { email="test56@gmail.com" });

		int i = 0;
		foreach( var cust in custs ) {
			if (cust.GetType() != typeof(pl.Customer))
				throw new System.Exception("Invalid type");
			objMatch( vals.output[i++], cust );
			Console.Write("+");
		}
	}

	public static void test_del_cust() {
		var vals = new {
			input=new {
				email="test2@gmail.com",
				name="Test Account"
			},
			output=new {
				email="test2@gmail.com",
				name="Test Account",
				type="customer"
			}
		};

		var cust = pl.Customer.create(vals.input);
		Console.WriteLine(cust.json());
		cust.delete();

		if (cust.GetType() != typeof(pl.Customer))
			throw new System.Exception("Invalid type");

		objMatch( vals.output, cust );
		Console.Write("+");

		try {
			pl.Customer.get(cust.id);
			//throw new Exception("Get should have thrown error");
		} catch ( pl.NotFound e ) {
			//this is the error we expected
			Console.Write("!");
		}

	}

	public static void test_del_cust_query() {
		var vals = new {
			input=new []{
				new {
					email="test3@gmail.com",
					name="Test Account"
				},
				new {
					email="test4@gmail.com",
					name="Test Account"
				}
			},
			output=new []{
				new {
					email="test3@gmail.com",
					name="Test Account",
					type="customer"
				},
				new {
					email="test4@gmail.com",
					name="Test Account",
					type="customer"
				}
			}
		};

		var custs = pl.Customer.create(vals.input);

		int i = 0;
		foreach( var cust in custs ) {
			if (cust.GetType() != typeof(pl.Customer))
				throw new System.Exception("Invalid type");
			objMatch( vals.output[i++], cust );
			Console.Write("+");
		}

		pl.Customer.delete_all(custs);

		string id_query = String.Join("|", (from o in (List<dynamic>)custs select o.id).ToArray());

		custs = pl.Customer.filter_by( pl.attr.id.eq(id_query) ).all();

		if ( custs.Count > 0 )
			throw new System.Exception("Invalid delete");

	}

	public static void test_json() {
		Console.WriteLine("TODO");
	}


    }
}
