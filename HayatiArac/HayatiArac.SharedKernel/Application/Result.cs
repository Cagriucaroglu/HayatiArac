namespace HayatiArac.SharedKernel.Application;

public record Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public IEnumerable<string>? Errors { get; }

    protected Result(bool isSuccess, string? error = null, IEnumerable<string>? errors = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Errors = errors;
    }

    public static Result Success() => new(true);
    public static Result Failure(string error) => new(false, error);
    public static Result Failure(IEnumerable<string> errors) => new(false, errors: errors);

    public static Result<T> Success<T>(T value) => new(value, true);
    public static Result<T> Failure<T>(string error) => new(default, false, error);
    public static Result<T> Failure<T>(IEnumerable<string> errors) => new(default, false, errors: errors);
}

public record Result<T> : Result
{
    public T? Value { get; }

    internal Result(T? value, bool isSuccess, string? error = null, IEnumerable<string>? errors = null)
        : base(isSuccess, error, errors)
    {
        Value = value;
    }
}
