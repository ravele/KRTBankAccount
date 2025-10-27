using KRTBankAccount.Application.DTOs;
using KRTBankAccount.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace KRTBankAccount.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly AccountService _accountService;

    public AccountsController(AccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountDto dto)
    {
        var id = await _accountService.CreateAccountAsync(dto);
        return CreatedAtAction(nameof(Create), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var account = await _accountService.GetByIdAsync(id);

        if (account == null)
            return NotFound(new { message = "Conta não encontrada." });

        return Ok(new
        {
            account.Id,
            account.HolderName,
            account.CPF,
            Status = account.IsActive ? "Ativo" : "Inativo"
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? status = null)
    {
        var accounts = await _accountService.GetAllAsync(status);

        var result = accounts.Select(a => new
        {
            a.Id,
            a.HolderName,
            a.CPF,
            Status = a.IsActive ? "Ativo" : "Inativo"
        });

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountDto dto)
    {
        var updated = await _accountService.UpdateAccountAsync(id, dto);

        if (!updated)
            return NotFound(new { message = "Conta não encontrada." });

        return Ok(new { message = "Conta atualizada com sucesso." });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _accountService.DeleteAccountAsync(id);

        if (!deleted)
            return NotFound(new { message = "Conta não encontrada." });

        return NoContent();
    }
}