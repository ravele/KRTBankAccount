namespace KRTBankAccount.Domain.Entities;

public class Account
{
    public Guid Id { get; private set; }
    public string HolderName { get; private set; }
    public string CPF { get; private set; }
    public bool IsActive { get; private set; }

    public Account(string holderName, string cpf)
    {
        Id = Guid.NewGuid();
        HolderName = holderName;
        CPF = cpf;
        IsActive = true;
    }

    private Account() { }

    internal Account(Guid id, string holderName, string cpf, bool isActive)
    {
        Id = id;
        HolderName = holderName;
        CPF = cpf;
        IsActive = isActive;
    }

    public void UpdateHolderName(string name) => HolderName = name;
    public void UpdateCPF(string cpf) => CPF = cpf;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}