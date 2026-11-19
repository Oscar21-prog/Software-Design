namespace EBank.BuildingBlocks.Errors;

public sealed class EBankException : Exception
{
    public string Code { get; }
    public int StatusCode { get; }

    public EBankException(string code, string message, int statusCode = 400) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }
}

public sealed record ErrorResponse(ErrorDetail Error);
public sealed record ErrorDetail(string Code, string Message);
