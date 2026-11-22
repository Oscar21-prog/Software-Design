namespace EBank.Kyc.Contracts.Enums;

public enum CaseType
{
    ONBOARDING,
    PERIODIC_REVIEW,
    CHANGE_REVIEW
}

public enum CaseStatus
{
    DRAFT,
    SUBMITTED,
    CHECKS_IN_PROGRESS,
    PENDING_CUSTOMER,
    MANUAL_REVIEW,
    AWAITING_SECOND_APPROVAL,
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

public enum SubjectType
{
    INDIVIDUAL,
    LEGAL_ENTITY
}

public enum PartyRole
{
    SUBJECT,
    REPRESENTATIVE,
    UBO
}

public enum DocType
{
    PASSPORT,
    ID_CARD,
    RESIDENCE_PERMIT,
    SELFIE,
    PROOF_OF_ADDRESS,
    REGISTRY_EXTRACT,
    UBO_DECLARATION
}

public enum DocStatus
{
    UPLOADED,
    VERIFIED,
    REJECTED,
    EXPIRED
}

public enum CheckType
{
    IDENTITY_DOC,
    LIVENESS,
    SANCTIONS,
    PEP,
    REGISTRY,
    UBO_MATCH
}

public enum CheckResult
{
    PASS,
    FAIL,
    HIT,
    ERROR
}

public enum Decision
{
    APPROVE,
    REJECT,
    REQUEST_INFO
}

public enum RejectReason
{
    SANCTIONS_MATCH,
    FRAUD_SUSPECTED,
    DOCUMENTS_INVALID,
    OTHER
}

public enum RuleFactor
{
    COUNTRY,
    SUBJECT_TYPE,
    STRUCTURE,
    PEP,
    CHECK_RESULT
}

