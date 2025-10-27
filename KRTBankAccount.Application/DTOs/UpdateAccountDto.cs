namespace KRTBankAccount.Application.DTOs;

public class UpdateAccountDto
{
    public string HolderName { get; set; }
    public string CPF { get; set; }
    public string Status { get; set; }
}