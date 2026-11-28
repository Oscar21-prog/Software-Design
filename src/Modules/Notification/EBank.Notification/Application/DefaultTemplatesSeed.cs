using EBank.Notification.Domain;

namespace EBank.Notification.Application;

/// <summary>
/// Spec §12.8.4 (EN) & §12.8.5 (LT): All 17 starting templates in English and Lithuanian.
/// </summary>
public static class DefaultTemplatesSeed
{
    public static IReadOnlyList<NotificationTemplate> GetAll() =>
    [
        // --- ENGLISH (§12.8.4) ---
        T("TRANSACTION_COMPLETED", "INCOMING", NotificationCategory.TRANSACTIONS, NotificationLanguage.EN, false,
            "Payment received", "You received {amount} {currency} from {counterpartyName}."),
        T("TRANSACTION_COMPLETED", "OUTGOING", NotificationCategory.TRANSACTIONS, NotificationLanguage.EN, false,
            "Payment sent", "{amount} {currency} was sent to {counterpartyName}."),
        T("CARD_TRANSACTION_AUTHORIZED", null, NotificationCategory.CARDS, NotificationLanguage.EN, false,
            "Card payment", "{amount} {currency} at {merchantName}, card ending {cardLast4}."),
        T("TRANSACTION_REJECTED", null, NotificationCategory.SECURITY, NotificationLanguage.EN, true,
            "Payment not executed", "Your payment could not be executed. Please contact the bank."),
        T("CARD_BLOCKED", null, NotificationCategory.SECURITY, NotificationLanguage.EN, true,
            "Card blocked", "Your card ending {cardLast4} has been blocked."),
        T("OTP_REQUESTED", null, NotificationCategory.SECURITY, NotificationLanguage.EN, true,
            "Confirmation code", "Your code is {code}. Never share it with anyone."),
        T("NEW_DEVICE_LOGIN", null, NotificationCategory.SECURITY, NotificationLanguage.EN, true,
            "New login", "New login from {deviceName} in {city}. If this wasn't you, block access in the app."),
        T("LOAN_INSTALLMENT_DUE", null, NotificationCategory.LOANS, NotificationLanguage.EN, false,
            "Installment due", "{amount} EUR is due on {dueDate}."),
        T("LOAN_INSTALLMENT_MISSED", null, NotificationCategory.LOANS, NotificationLanguage.EN, false,
            "Installment overdue", "Your installment of {amount} EUR is overdue."),
        T("KYC_APPROVED", null, NotificationCategory.SECURITY, NotificationLanguage.EN, true,
            "Account verified", "Your identity has been verified. You can now use all services."),
        T("KYC_INFO_REQUESTED", null, NotificationCategory.SECURITY, NotificationLanguage.EN, true,
            "More information needed", "Please upload the requested documents in the app."),
        T("TRANSACTION_FAILED", null, NotificationCategory.TRANSACTIONS, NotificationLanguage.EN, true,
            "Payment failed", "Your payment of {amount} {currency} to {beneficiaryName} was not executed."),
        T("PAYMENT_PENDING_APPROVAL", null, NotificationCategory.TRANSACTIONS, NotificationLanguage.EN, true,
            "Approval needed", "A payment of {amount} {currency} to {beneficiaryName} is waiting for your approval."),
        T("PAYMENT_REJECTED", null, NotificationCategory.TRANSACTIONS, NotificationLanguage.EN, true,
            "Payment not approved", "Your payment of {amount} {currency} to {beneficiaryName} was rejected or has expired."),
        T("ACCOUNT_OPENED", null, NotificationCategory.TRANSACTIONS, NotificationLanguage.EN, false,
            "Account opened", "Your new {currency} account {iban} is ready."),
        T("ACCOUNT_STATUS_CHANGED", null, NotificationCategory.SECURITY, NotificationLanguage.EN, true,
            "Account status changed", "Your account {iban} is now {newStatus}."),
        T("MANDATE_CHANGED", null, NotificationCategory.SECURITY, NotificationLanguage.EN, true,
            "Account access changed", "Your access to account {iban} was changed."),

        // --- LITHUANIAN (§12.8.5) ---
        T("TRANSACTION_COMPLETED", "INCOMING", NotificationCategory.TRANSACTIONS, NotificationLanguage.LT, false,
            "Gautas mokėjimas", "Gavote {amount} {currency} iš {counterpartyName}."),
        T("TRANSACTION_COMPLETED", "OUTGOING", NotificationCategory.TRANSACTIONS, NotificationLanguage.LT, false,
            "Mokėjimas išsiųstas", "Išsiuntėte {amount} {currency} gavėjui {counterpartyName}."),
        T("CARD_TRANSACTION_AUTHORIZED", null, NotificationCategory.CARDS, NotificationLanguage.LT, false,
            "Mokėjimas kortele", "Atsiskaityta kortele, kurios numeris baigiasi {cardLast4}: {amount} {currency}, {merchantName}."),
        T("TRANSACTION_REJECTED", null, NotificationCategory.SECURITY, NotificationLanguage.LT, true,
            "Mokėjimas neįvykdytas", "Jūsų mokėjimo įvykdyti nepavyko. Susisiekite su banku."),
        T("CARD_BLOCKED", null, NotificationCategory.SECURITY, NotificationLanguage.LT, true,
            "Kortelė užblokuota", "Jūsų kortelė, kurios numeris baigiasi {cardLast4}, užblokuota."),
        T("OTP_REQUESTED", null, NotificationCategory.SECURITY, NotificationLanguage.LT, true,
            "Patvirtinimo kodas", "Jūsų kodas: {code}. Niekam jo nesakykite."),
        T("NEW_DEVICE_LOGIN", null, NotificationCategory.SECURITY, NotificationLanguage.LT, true,
            "Naujas prisijungimas", "Prisijungta iš {deviceName} ({city}). Jei tai ne jūs, užblokuokite prieigą programėlėje."),
        T("LOAN_INSTALLMENT_DUE", null, NotificationCategory.LOANS, NotificationLanguage.LT, false,
            "Artėja įmokos terminas", "Įmoką {amount} EUR reikia sumokėti iki {dueDate}."),
        T("LOAN_INSTALLMENT_MISSED", null, NotificationCategory.LOANS, NotificationLanguage.LT, false,
            "Pradelsta įmoka", "Jūsų {amount} EUR įmoka pradelsta."),
        T("KYC_APPROVED", null, NotificationCategory.SECURITY, NotificationLanguage.LT, true,
            "Tapatybė patvirtinta", "Jūsų tapatybė patvirtinta. Dabar galite naudotis visomis paslaugomis."),
        T("KYC_INFO_REQUESTED", null, NotificationCategory.SECURITY, NotificationLanguage.LT, true,
            "Reikia papildomos informacijos", "Įkelkite prašomus dokumentus programėlėje."),
        T("TRANSACTION_FAILED", null, NotificationCategory.TRANSACTIONS, NotificationLanguage.LT, true,
            "Mokėjimas nepavyko", "Jūsų mokėjimas {amount} {currency} gavėjui {beneficiaryName} neįvykdytas."),
        T("PAYMENT_PENDING_APPROVAL", null, NotificationCategory.TRANSACTIONS, NotificationLanguage.LT, true,
            "Laukia jūsų patvirtinimo", "Mokėjimas {amount} {currency} gavėjui {beneficiaryName} laukia jūsų patvirtinimo."),
        T("PAYMENT_REJECTED", null, NotificationCategory.TRANSACTIONS, NotificationLanguage.LT, true,
            "Mokėjimas nepatvirtintas", "Jūsų mokėjimas {amount} {currency} gavėjui {beneficiaryName} atmestas arba nebegalioja."),
        T("ACCOUNT_OPENED", null, NotificationCategory.TRANSACTIONS, NotificationLanguage.LT, false,
            "Sąskaita atidaryta", "Jūsų nauja {currency} sąskaita {iban} paruošta."),
        T("ACCOUNT_STATUS_CHANGED", null, NotificationCategory.SECURITY, NotificationLanguage.LT, true,
            "Pasikeitė sąskaitos būsena", "Jūsų sąskaitos {iban} būsena: {newStatus}."),
        T("MANDATE_CHANGED", null, NotificationCategory.SECURITY, NotificationLanguage.LT, true,
            "Pasikeitė prieiga prie sąskaitos", "Pakeista jūsų prieiga prie sąskaitos {iban}.")
    ];

    private static NotificationTemplate T(
        string eventType, string? variant, NotificationCategory cat, NotificationLanguage lang, bool mandatory, string title, string body) =>
        new()
        {
            EventType = eventType,
            Variant = variant,
            Category = cat,
            Language = lang,
            Mandatory = mandatory,
            Title = title,
            Body = body
        };
}
