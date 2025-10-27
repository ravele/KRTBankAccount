using Bogus;
using Bogus.Extensions.Brazil;
using FluentAssertions;
using KRTBankAccount.Application.Services;
using KRTBankAccount.Domain.Entities;
using KRTBankAccount.Infrastructure.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Newtonsoft.Json;

namespace KRTBankAccount.Tests.Services;

public class GetByIdAccountServiceTests
{
    private readonly Mock<IAccountRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly AccountService _service;
    private readonly Faker _faker;

    public GetByIdAccountServiceTests()
    {
        _repositoryMock = new Mock<IAccountRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _service = new AccountService(_repositoryMock.Object, _cacheMock.Object);
        _faker = new Faker("pt_BR");
    }

    [Fact(DisplayName = "Deve retornar conta do cache se já existir")]
    public async Task GetByIdAsync_ShouldReturnFromCache_WhenCacheExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fakeAccount = new Account(_faker.Name.FullName(), _faker.Person.Cpf());
        var serialized = JsonConvert.SerializeObject(fakeAccount);
        var bytes = System.Text.Encoding.UTF8.GetBytes(serialized);

        _cacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.CPF.Should().Be(fakeAccount.CPF);

        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact(DisplayName = "Deve buscar no banco e salvar no cache quando não existir no cache")]
    public async Task GetByIdAsync_ShouldQueryDatabase_WhenCacheMiss()
    {
        // Arrange
        var id = Guid.NewGuid();
        var fakeAccount = new Account(_faker.Name.FullName(), _faker.Person.Cpf());

        _cacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(fakeAccount);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.CPF.Should().Be(fakeAccount.CPF);

        _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact(DisplayName = "Deve retornar null se conta não existir no banco nem no cache")]
    public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        _cacheMock
            .Setup(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _service.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();

        _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()),
            Times.Never);
    }
}