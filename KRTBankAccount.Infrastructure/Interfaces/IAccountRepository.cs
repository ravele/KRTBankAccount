using KRTBankAccount.Domain.Entities;

namespace KRTBankAccount.Infrastructure.Interfaces;

public interface IAccountRepository
{
    Task AddAsync(Account account);
    Task<Account?> GetByIdAsync(Guid id);
    Task<IEnumerable<Account>> GetAllAsync(bool? isActive = null);
    Task UpdateAsync(Account account);
    Task<bool> DeleteAsync(Guid id);
}