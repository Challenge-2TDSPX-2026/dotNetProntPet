namespace ProntPet.API.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
    Task CreateAsync(T entity, CancellationToken ct = default);
    Task<bool> UpdateAsync(string id, T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
}