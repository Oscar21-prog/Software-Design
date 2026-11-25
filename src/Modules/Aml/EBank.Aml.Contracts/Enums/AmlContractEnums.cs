using System.Text.Json.Serialization;

namespace EBank.Aml.Contracts.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScreeningDecision
{
    ALLOW,
    HOLD
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScreeningSource
{
    PAYMENT,
    CARD
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ScreeningResult
{
    CLEAR,
    HIT
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WatchlistType
{
    SANCTIONS,
    PEP
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum WatchlistMatchType
{
    NAME_ONLY,
    NAME_AND_BIRTH_DATE,
    NAME_AND_CODE
}