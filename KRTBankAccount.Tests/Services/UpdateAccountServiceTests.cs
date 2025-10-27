using Bogus;
using Bogus.Extensions.Brazil;
using FluentAssertions;
using KRTBankAccount.Application.DTOs;
using KRTBankAccount.Application.Services;
using KRTBankAccount.Domain.Entities;
using KRTBankAccount.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;

namespace KRTBankAccount.Tests.Services;

public class UpdateAccountServiceTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly AccountService _service;
    private readonly Faker _faker;

    public UpdateAccountServiceTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _service = new AccountService(_repositoryMock.Object, _cacheMock.Object);
        _faker = new Faker("pt_BR");
    }

    [Fact(DisplayName = "Deve atualizar conta existente com sucesso e remover do cache")]
    public async Task UpdateAccountAsync_ShouldUpdate_WhenAccountExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingAccount = new Account("Titular Antigo", "12345678901");
        var dto = new UpdateAccountDto
        {
            HolderName = _faker.Name.FullName(),
            CPF = _faker.Person.Cpf(),
            Status = "ativo"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingAccount);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAccountAsync(id, dto);

        // Assert
        result.Should().BeTrue();

        _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Once);

        _cacheMock.Verify(c => c.RemoveAsync(
            $"account:{id}",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Deve retornar false quando conta não for encontrada")]
    public async Task UpdateAccountAsync_ShouldReturnFalse_WhenAccountNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new UpdateAccountDto
        {
            HolderName = _faker.Name.FullName(),
            CPF = _faker.Person.Cpf(),
            Status = "inativo"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.UpdateAccountAsync(id, dto);

        // Assert
        result.Should().BeFalse();

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Deve atualizar conta mesmo se status estiver em maiúsculas ou misto")]
    public async Task UpdateAccountAsync_ShouldHandleStatusCaseInsensitive()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingAccount = new Account("Titular Original", "98765432100");
        var dto = new UpdateAccountDto
        {
            HolderName = "Titular Atualizado",
            CPF = "12345678901",
            Status = "ATIVO"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingAccount);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .Returns(Task.CompletedTask);

        _cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.UpdateAccountAsync(id, dto);

        // Assert
        result.Should().BeTrue();

        _repositoryMock.Verify(r => r.UpdateAsync(It.Is<Account>(a => a.IsActive)), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync(
            $"account:{id}",
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}