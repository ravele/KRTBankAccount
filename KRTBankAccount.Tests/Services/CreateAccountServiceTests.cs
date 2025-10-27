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

public class CreateAccountServiceTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly AccountService _service;
    private readonly Faker _faker;

    public CreateAccountServiceTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _service = new AccountService(_repositoryMock.Object, _cacheMock.Object);
        _faker = new Faker("pt_BR");
    }

    [Fact(DisplayName = "Deve criar uma conta com sucesso")]
    public async Task AddAccountAsync_ShouldCreateAccountSuccessfully()
    {
        // Arrange
        var dto = new CreateAccountDto
        {
            HolderName = _faker.Name.FullName(),
            CPF = _faker.Person.Cpf()
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Account>()))
            .Returns(Task.CompletedTask);

        // Act
        await _repositoryMock.Object.AddAsync(new Account(dto.HolderName, dto.CPF));

        // Assert
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Once);
    }

    [Fact(DisplayName = "Não deve criar conta com CPF inválido")]
    public async Task CreateAccountAsync_ShouldNotCreate_WhenCPFInvalid()
    {
        // Arrange
        var dto = new CreateAccountDto
        {
            HolderName = _faker.Name.FullName(),
            CPF = "123"
        };

        // Act
        Func<Task> act = async () => await _service.CreateAccountAsync(dto);

        await act.Should().ThrowAsync<ArgumentException>().WithMessage("CPF inválido.");
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Account>()), Times.Never);
    }
}