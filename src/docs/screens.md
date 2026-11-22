# E-Bank screen scope

We implement the minimum number of screens required to support the main
end-to-end flows and the Must features.

Features without a dedicated screen are implemented as components, tabs,
dialogs or sections inside an existing screen.

## Customer app

1. **Login / Registration** — registration, sign-in and one-time code.  
   Covers CRM-01 and CBS-06.

2. **Accounts** — account list, open account, account details and
   transaction history.  
   Covers CBS-01 and CBS-03.

3. **Transfer** — new transfer, confirmation, SCA, dual-signature approval
   and payment result.  
   Covers PAY-01, PAY-02, PAY-03, PAY-04 and PAY-06.

4. **Cards & Loans** — cards and loans, including card actions, loan
   application, offer and signing.  
   Covers CARD-01–CARD-05 and LOAN-01–LOAN-05.

## Back office

5. **Review** — KYC review, loan-officer review and AML queue/case details,
   organised by tabs.  
   Covers KYC-05, KYC-08, KYC-09, LOAN-02 and AML-05–AML-14.

6. **Accounting** — general ledger, discrepancies, reports and chart of
   accounts, organised by tabs.  
   Covers ACC-01–ACC-05.

## Components instead of screens

- One-time code dialog
- Transfer confirmation and result
- Dual-signature approval
- Pending approvals
- Manage mandates
- Saved recipients
- Loan calculator
- Notification settings
- Customer 360
- Card application and repayment
- Chart of accounts and reports

## Could features

Implemented only if time allows:

- PAY-05 saved recipients
- LOAN-06 loan calculator
- CRM-07 customer notes
- ACC-06 monthly statement
- NTF-09 notification language choice
