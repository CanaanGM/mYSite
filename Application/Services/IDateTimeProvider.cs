namespace Application.Services;

public interface IDateTimeProvider
{
    DateTime UtcNow {get; }
    DateTime LocalNow { get; }
}