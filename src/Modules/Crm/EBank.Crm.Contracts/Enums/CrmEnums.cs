namespace EBank.Crm.Contracts.Enums;

public enum CustomerType
{
    INDIVIDUAL,
    LEGAL_ENTITY
}

public enum CustomerStatus
{
    PROSPECT,
    ONBOARDING,
    ACTIVE,
    RESTRICTED,
    BLOCKED,
    CLOSED
}

public enum UserStatus
{
    PENDING,
    ACTIVE,
    BLOCKED
}

public enum Language
{
    LT,
    EN
}

public enum AddressType
{
    RESIDENCE,
    REGISTERED,
    MAILING
}

public enum RepRole
{
    DIRECTOR,
    ACCOUNTANT,
    PROCURATOR
}

public enum ConsentPurpose
{
    DATA_PROCESSING,
    MARKETING,
    E_STATEMENTS
}

public enum ChangeType
{
    STATUS,
    PROFILE,
    CONTACT,
    CONSENT,
    STRUCTURE
}

public enum KycStatus
{
    NONE,
    IN_PROGRESS,
    APPROVED,
    REJECTED,
    EXPIRED
}

public enum RiskLevel
{
    LOW,
    MEDIUM,
    HIGH
}

public enum StatusReason
{
    KYC_EXPIRED,
    KYC_APPROVED,
    AUTHORITY_REQUEST,
    SUSPECTED_FRAUD,
    CUSTOMER_REQUEST,
    DECEASED,
    OTHER
}

public enum SensitiveFieldGroup
{
    NAME,
    PERSONAL_CODE,
    DATE_OF_BIRTH,
    NATIONALITY,
    ADDRESS,
    STRUCTURE
}

