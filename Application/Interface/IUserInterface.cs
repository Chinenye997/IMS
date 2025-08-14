using Application.DTOs;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interface
{
    public interface IUserInterface
    {
        Task<IdentityResult> RegisterAsync(RegisterRequest registerRequest);
        Task<SignInResult> LoginAsync(LoginRequest loginRequest);
        Task LogoutAsync();
        Task<UserEntity> GetCurrentUserAsync(); // Added for current user retrieval
        Task<List<UserResponse>> GetAll();
        Task<UserResponse> GetUserById(string id);
        Task<UserResponse> UpdateUser(string id, UserRequest request);
        Task<List<UserResponse>> UnApproveAgents();
        Task<string> ApproveAgent(string id);
        Task<bool> DeleteUser(string id);
    }
}