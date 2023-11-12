// Ignore Spelling: Accessor Username

namespace DataAccess.Shared;


public enum OperationStatus
{
    Success,
    Created,
    Updated,
    Deleted,
    NotFound,
    Error
}

public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string? ErrorMessage { get; }
    public OperationStatus Operation { get; }

    private Result(bool isSuccess, T value, string errorMessage, OperationStatus operation)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        Operation = operation;
    }

    public static Result<T> Success(T value, OperationStatus operation = OperationStatus.Success)
    {
        return new Result<T>(true, value, null!, operation);
    }

    public static Result<T> Failure(string errorMessage, OperationStatus operation = OperationStatus.Error)
    {
        return new Result<T>(false, default!, errorMessage, operation);
    }
}