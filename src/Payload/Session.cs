using Payload.ARM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payload
{
    public partial class Payload
    {
        public partial class Session
        {
            public dynamic Attr = new Attr(null);
            public ARMRequest<pl.AccessToken> AccessToken => new ARMRequest<pl.AccessToken>(this);
            public ARMRequest<pl.ClientToken> ClientToken => new ARMRequest<pl.ClientToken>(this);
            public ARMRequest<pl.Account> Account => new ARMRequest<pl.Account>(this);
            public ARMRequest<pl.Customer> Customer => new ARMRequest<pl.Customer>(this);
            public ARMRequest<pl.ProcessingAccount> ProcessingAccount => new ARMRequest<pl.ProcessingAccount>(this);
            public ARMRequest<pl.Org> Org => new ARMRequest<pl.Org>(this);
            public ARMRequest<pl.User> User => new ARMRequest<pl.User>(this);
            public ARMRequest<pl.Transaction> Transaction => new ARMRequest<pl.Transaction>(this);
            public ARMRequest<pl.Payment> Payment => new ARMRequest<pl.Payment>(this);
            public ARMRequest<pl.Refund> Refund => new ARMRequest<pl.Refund>(this);
            public ARMRequest<pl.Credit> Credit => new ARMRequest<pl.Credit>(this);
            public ARMRequest<pl.Deposit> Deposit => new ARMRequest<pl.Deposit>(this);
            public ARMRequest<pl.Ledger> Ledger => new ARMRequest<pl.Ledger>(this);
            public ARMRequest<pl.PaymentMethod> PaymentMethod => new ARMRequest<pl.PaymentMethod>(this);
            public ARMRequest<pl.Card> Card => new ARMRequest<pl.Card>(this);
            public ARMRequest<pl.BankAccount> BankAccount => new ARMRequest<pl.BankAccount>(this);
            public ARMRequest<pl.BillingSchedule> BillingSchedule => new ARMRequest<pl.BillingSchedule>(this);
            public ARMRequest<pl.BillingCharge> BillingCharge => new ARMRequest<pl.BillingCharge>(this);
            public ARMRequest<pl.Invoice> Invoice => new ARMRequest<pl.Invoice>(this);
            public ARMRequest<pl.LineItem> LineItem => new ARMRequest<pl.LineItem>(this);
            public ARMRequest<pl.ChargeItem> ChargeItem => new ARMRequest<pl.ChargeItem>(this);
            public ARMRequest<pl.PaymentItem> PaymentItem => new ARMRequest<pl.PaymentItem>(this);
            public ARMRequest<pl.Webhook> Webhook => new ARMRequest<pl.Webhook>(this);
            public ARMRequest<pl.PaymentLink> PaymentLink => new ARMRequest<pl.PaymentLink>(this);
            public ARMRequest<pl.OAuthToken> OAuthToken => new ARMRequest<pl.OAuthToken>(this);
            public ARMRequest<pl.PaymentActivation> PaymentActivation => new ARMRequest<pl.PaymentActivation>(this);
        }
    }
}
