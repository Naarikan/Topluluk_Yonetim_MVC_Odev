using Topluluk_Yonetim.MVC.Data.Repositories.Interfaces;
using Topluluk_Yonetim.MVC.Exceptions;
using Topluluk_Yonetim.MVC.Services.Interfaces;

namespace Topluluk_Yonetim.MVC.Services.Implementations;

public class Service<T> : IService<T> where T : class
{
    protected readonly IRepository<T> _repository;

    public Service(IRepository<T> repository)
    {
        _repository = repository;
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ValidationException("Geçersiz kimlik değeri.");

        return await _repository.GetByIdAsync(id);
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        if (entity == null)
            throw new ValidationException("Oluşturulacak kayıt bilgileri boş olamaz.");

        await _repository.AddAsync(entity);
        await _repository.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        if (entity == null)
            throw new ValidationException("Güncellenecek kayıt bilgileri boş olamaz.");

        _repository.Update(entity);
        var result = await _repository.SaveChangesAsync();
        return result > 0;
    }

    public virtual async Task<bool> DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ValidationException("Geçersiz kimlik değeri.");

        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
            throw new NotFoundException("Silinecek kayıt bulunamadı.");

        _repository.Remove(entity);
        var result = await _repository.SaveChangesAsync();
        return result > 0;
    }

    public virtual async Task<bool> ExistsAsync(Guid id)
    {
        if (id == Guid.Empty)
            throw new ValidationException("Geçersiz kimlik değeri.");

        var entity = await _repository.GetByIdAsync(id);
        return entity != null;
    }
}

