using NSubstitute;
using Shop.Shared.Exceptions;
using Shop.Users.Application.DTOs.Users;
using Shop.Users.Application.Interfaces;
using Shop.Users.Application.Services;
using Shop.Users.Domain.Models;

namespace Shop.Users.Tests.Services;

public class UserServiceTests
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IProductServiceClient _productServiceClient = Substitute.For<IProductServiceClient>();
    
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_userRepository, _productServiceClient);
    }

    [Fact]
    public async Task UpdateUsernameAsync_UserNotFound_ThrowsUserNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.UpdateUsernameAsync(Guid.NewGuid(), new UpdateUsernameDto { Name = "NewName" })
        );
    }

    [Fact]
    public async Task UpdateUsernameAsync_ValidUser_ChangesName()
    {
        var user = new User { Name = "OldName"};
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(user);
        
        await _sut.UpdateUsernameAsync(Guid.NewGuid(), new UpdateUsernameDto { Name = "NewName" });
        
        await _userRepository.Received(1).SaveChangesAsync();
        Assert.Equal("NewName", user.Name);
    }

    [Fact]
    public async Task DeleteSelf_EmptyGuid_ThrowsAuthorisationException()
    {
        await Assert.ThrowsAsync<AuthorisationException>(() =>
            _sut.DeleteSelf(null)
        );
    }

    [Fact]
    public async Task DeleteSelf_ValidUser_DeletesUserFromDatabase()
    {
        var user = new User {Id = Guid.NewGuid(), Email = "test@test.com"};

        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(user);

        await _sut.DeleteSelf(user.Id.ToString());
        
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task DeleteAsync_UserNotFound_ThrowsUserNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?) null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.DeleteAsync(Arg.Any<Guid>())
        );
    }
    
    [Fact]
    public async Task DeleteAsync_ValidUser_DeletesUserFromDatabase()
    {
        var user = new User {Id = Guid.NewGuid(), Email = "test@test.com"};

        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(user);

        await _sut.DeleteAsync(user.Id);
        
        await _userRepository.Received(1).SaveChangesAsync();
        await _productServiceClient.Received(1).DeleteUserProducts(user.Id);
    }

    [Fact]
    public async Task DeactivateAsync_UserNotFound_ThrowsUserNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?) null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.DeactivateAsync(Arg.Any<Guid>())
        );
    }
    
    [Fact]
    public async Task DeactivateAsync_ValidUser_DeactivatesUserInDatabase()
    {
        var user = new User {Id = Guid.NewGuid(), Email = "test@test.com"};

        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(user);

        await _sut.DeactivateAsync(user.Id);
        
        Assert.False(user.IsActive);
        
        await _userRepository.Received(1).SaveChangesAsync();
        await _productServiceClient.Received(1).DeactivateUserProducts(user.Id);
    }
    
    [Fact]
    public async Task ActivateAsync_UserNotFound_ThrowsUserNotFoundException()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?) null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _sut.ActivateAsync(Arg.Any<Guid>())
        );
    }
    
    [Fact]
    public async Task ActivateAsync_ValidUser_ActivatesUserInDatabase()
    {
        var user = new User {Id = Guid.NewGuid(),Email = "test@test.com"};

        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(user);

        await _sut.ActivateAsync(user.Id);
        
        Assert.True(user.IsActive);
        
        await _userRepository.Received(1).SaveChangesAsync();
        await _productServiceClient.Received(1).ReactivateUserProducts(user.Id);
    }
    
    [Fact]
    public async Task GetByIdAsync_UserNotFound_ReturnsNull()
    {
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns((User?)null);
        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_UserFound_ReturnsMappedDto()
    {
        var user = new User { Id = Guid.NewGuid(), Name = "Test", Email = "test@test.com" };
        _userRepository.GetByIdAsync(Arg.Any<Guid>()).Returns(user);
    
        var result = await _sut.GetByIdAsync(user.Id);
    
        Assert.NotNull(result);
        Assert.Equal(user.Email, result.Email);
        Assert.Equal(user.Name, result.Name);
    }
}