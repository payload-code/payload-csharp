using Payload.ARM;
using System;
using System.Collections.Generic;
using System.Text;

namespace Payload
{
    public partial class pl
    {
        public partial class Session
        {
            public ARMRequest<AccessToken> AccessToken => new ARMRequest<AccessToken>(this);
            public ARMRequest<ClientToken> ClientToken => new ARMRequest<ClientToken>(this);
            public ARMRequest<Account> Account => new ARMRequest<Account>(this);
            public ARMRequest<Customer> Customer => new ARMRequest<Customer>(this);
            public ARMRequest<ProcessingAccount> ProcessingAccount => new ARMRequest<ProcessingAccount>(this);
            public ARMRequest<Org> Org => new ARMRequest<Org>(this);
            public ARMRequest<User> User => new ARMRequest<User>(this);
            public ARMRequest<Transaction> Transaction => new ARMRequest<Transaction>(this);
            public ARMRequest<Payment> Payment => new ARMRequest<Payment>(this);
            public ARMRequest<Refund> Refund => new ARMRequest<Refund>(this);
            public ARMRequest<Credit> Credit => new ARMRequest<Credit>(this);
            public ARMRequest<Deposit> Deposit => new ARMRequest<Deposit>(this);
            public ARMRequest<Ledger> Ledger => new ARMRequest<Ledger>(this);
            public ARMRequest<PaymentMethod> PaymentMethod => new ARMRequest<PaymentMethod>(this);
            public ARMRequest<Card> Card => new ARMRequest<Card>(this);
            public ARMRequest<BankAccount> BankAccount => new ARMRequest<BankAccount>(this);
            public ARMRequest<BillingSchedule> BillingSchedule => new ARMRequest<BillingSchedule>(this);
            public ARMRequest<BillingCharge> BillingCharge => new ARMRequest<BillingCharge>(this);
            public ARMRequest<Invoice> Invoice => new ARMRequest<Invoice>(this);
            public ARMRequest<LineItem> LineItem => new ARMRequest<LineItem>(this);
            public ARMRequest<ChargeItem> ChargeItem => new ARMRequest<ChargeItem>(this);
            public ARMRequest<PaymentItem> PaymentItem => new ARMRequest<PaymentItem>(this);
            public ARMRequest<Webhook> Webhook => new ARMRequest<Webhook>(this);
            public ARMRequest<PaymentLink> PaymentLink => new ARMRequest<PaymentLink>(this);
            public ARMRequest<OAuthToken> OAuthToken => new ARMRequest<OAuthToken>(this);
            public ARMRequest<PaymentActivation> PaymentActivation => new ARMRequest<PaymentActivation>(this);
        }
    }
}
