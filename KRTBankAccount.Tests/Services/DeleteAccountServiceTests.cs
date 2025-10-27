using Bogus;
using Bogus.Extensions.Brazil;
using FluentAssertions;
using KRTBankAccount.Application.Services;
using KRTBankAccount.Domain.Entities;
using KRTBankAccount.Infrastructure.Interfaces;
using Moq;

namespace KRTBankAccount.Tests.Services;

public class DeleteAccountServiceTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly AccountService _service;
    private readonly Faker _faker;

    public DeleteAccountServiceTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        _service = new AccountService(_repositoryMock.Object, Mock.Of<Microsoft.Extensions.Caching.Distributed.IDistributedCache>());
        _faker = new Faker("pt_BR");
    }

    [Fact(DisplayName = "Deve desativar conta existente e retornar true")]
    public async Task DeleteAccountAsync_ShouldDeactivateAccount_WhenAccountExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var existingAccount = new Account(_faker.Name.FullName(), _faker.Person.Cpf());

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(existingAccount);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Account>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteAccountAsync(id);

        // Assert
        result.Should().BeTrue();
        existingAccount.IsActive.Should().BeFalse();

        _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(existingAccount), Times.Once);
    }

    [Fact(DisplayName = "Deve retornar false se conta não existir")]
    public async Task DeleteAccountAsync_ShouldReturnFalse_WhenAccountNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.DeleteAccountAsync(id);

        // Assert
        result.Should().BeFalse();

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Account>()), Times.Never);
    }
}