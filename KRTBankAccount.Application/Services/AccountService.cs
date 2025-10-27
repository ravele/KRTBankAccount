using KRTBankAccount.Application.DTOs;
using KRTBankAccount.Domain.Entities;
using KRTBankAccount.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace KRTBankAccount.Application.Services;

public class AccountService
{
    private readonly IAccountRepository _repository;
    private readonly IDistributedCache _cache;

    public AccountService(IAccountRepository repository, IDistributedCache cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<Guid> CreateAccountAsync(CreateAccountDto dto)
    {
        if (dto == null) throw new ArgumentNullException(nameof(dto));
        if (string.IsNullOrWhiteSpace(dto.HolderName)) throw new ArgumentException("HolderName é obrigatório.");
        if (!IsValidCpf(dto.CPF)) throw new ArgumentException("CPF inválido.");

        var account = new Account(dto.HolderName, dto.CPF);
        await _repository.AddAsync(account);

        return account.Id;
    }

    public async Task<Account?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"account:{id}";

        var cachedAccount = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedAccount))
        {
            var account = JsonConvert.DeserializeObject<Account>(cachedAccount);
            return account;
        }

        var dbAccount = await _repository.GetByIdAsync(id);
        if (dbAccount == null)
            return null;

        var serialized = JsonConvert.SerializeObject(dbAccount);
        var cacheOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        };

        await _cache.SetStringAsync(cacheKey, serialized, cacheOptions);

        return dbAccount;
    }

    public async Task<IEnumerable<Account>> GetAllAsync(string? status = null)
    {
        bool? isActive = status?.ToLower() switch
        {
            "ativo" => true,
            "inativo" => false,
            _ => null
        };

        return await _repository.GetAllAsync(isActive);
    }

    public async Task<bool> UpdateAccountAsync(Guid id, UpdateAccountDto dto)
    {
        var account = await _repository.GetByIdAsync(id);
        if (account == null) return false;

        account.UpdateHolderName(dto.HolderName);
        account.UpdateCPF(dto.CPF);

        if (dto.Status.ToLower() == "ativo")
            account.Activate();
        else
            account.Deactivate();

        await _repository.UpdateAsync(account);

        await _cache.RemoveAsync($"account:{id}");

        return true;
    }

    public async Task<bool> DeleteAccountAsync(Guid id)
    {
        var account = await _repository.GetByIdAsync(id);
        if (account == null)
            return false;

        account.Deactivate();
        await _repository.UpdateAsync(account);
        return true;
    }

    private bool IsValidCpf(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf)) return false;
        var onlyDigits = Regex.Replace(cpf, @"\D", "");
        return onlyDigits.Length == 11;
    }
}