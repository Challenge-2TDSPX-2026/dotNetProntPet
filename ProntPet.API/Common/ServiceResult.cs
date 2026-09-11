namespace ProntPet.Common;

/// <summary>
/// Status possíveis de uma operação executada por um Service.
/// </summary>
public enum ServiceStatus
{
    Ok,
    NotFound,
    ValidationError
}

/// <summary>
/// Encapsula o resultado de uma operação de Service, permitindo que o Controller
/// traduza o desfecho da regra de negócio em uma resposta HTTP sem conhecer a regra em si.
/// </summary>
/// <typeparam name="T">Tipo do dado retornado em caso de sucesso.</typeparam>
public class ServiceResult<T>
{
    public ServiceStatus Status { get; }
    public T? Value { get; }
    public string? Message { get; }

    private ServiceResult(ServiceStatus status, T? value, string? message)
    {
        Status = status;
        Value = value;
        Message = message;
    }

    public static ServiceResult<T> Ok(T value) =>
        new(ServiceStatus.Ok, value, null);

    public static ServiceResult<T> NotFound(string message) =>
        new(ServiceStatus.NotFound, default, message);

    public static ServiceResult<T> ValidationError(string message) =>
        new(ServiceStatus.ValidationError, default, message);
}
