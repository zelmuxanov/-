namespace CarRental.Domain.Exceptions;

public class BusinessException : Exception
{
    public string Code { get; }

    public BusinessException(string message) : base(message)
    {
        Code = "BUSINESS_ERROR";
    }

    public BusinessException(string code, string message) : base(message)
    {
        Code = code;
    }
}
