using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WarehouseOps.Domain.Interfaces;
using WarehouseOps.Infrastructure.Persistence;

namespace WarehouseOps.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly WarehouseDbContext _db;
    protected readonly DbSet<T> _set;

    public Repository(WarehouseDbContext db) { _db = db; _set = db.Set<T>(); }

    public virtual async Task<T?> GetByIdAsync(int id) => await _set.FindAsync(id);
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await _set.ToListAsync();
    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _set.Where(predicate).ToListAsync();
    public async Task AddAsync(T entity) => await _set.AddAsync(entity);
    public void Update(T entity) => _set.Update(entity);
    public void Remove(T entity) => _set.Remove(entity);
    public async Task SaveChangesAsync() => await _db.SaveChangesAsync();
}
