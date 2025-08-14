using Application.DTOs;
using Application.Interface;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    // Service handling user registration, login, logout, and user management.
    public class UserService : IUserInterface
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly string _profilePicsPath;

        // Constructor injects identity managers and HTTP context for accessing the request
        // Uses IHostingEnvironment to set up where profile photos are saved
        public UserService(
            UserManager<UserEntity> userManager,
            SignInManager<UserEntity> signInManager,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment env,
            AppDbContext dbContext)  // build photo save path
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
            _context = dbContext;

            // Prepare folder path under wwwroot/profilePics
            _profilePicsPath = Path.Combine(env.WebRootPath, "profilePics");
            Directory.CreateDirectory(_profilePicsPath);
        }

        // Agents must be approved by Admin before login.
        public async Task<IdentityResult> RegisterAsync(RegisterRequest request)
        {
            // Check if passwords match
            if (request.Password != request.ConfirmPassword)
                return IdentityResult.Failed(new IdentityError { Description = "Passwords do not match." });

            string photoPath = null;
            if (request.ProfilePhoto != null)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(request.ProfilePhoto.FileName);
                var fullPath = Path.Combine(_profilePicsPath, fileName);
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await request.ProfilePhoto.CopyToAsync(stream);
                }
                photoPath = "/profilePics/" + fileName; // Single upload logic
            }

            var user = new UserEntity
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                FullName = $"{request.FirstName} {request.LastName}",
                Gender = request.Gender,
                Address = request.Address,
                PhoneNumber = request.PhoneNumber,
                Email = request.Email,
                UserName = request.Email,
                DateRegistered = DateTime.UtcNow,
                ProfilePhotoUrl = photoPath,
                // Agents default to not approved, NormalUser approved
                IsApproved = request.Role != "Agent"
            };

            // Create user in Identity store with password
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return result;

            // Assign role
            await _userManager.AddToRoleAsync(user, request.Role);
            return result;
        }

        // Attempts to sign in a user; checks approval for agents.
        public async Task<SignInResult> LoginAsync(LoginRequest loginRequest)
        {
            // Step 1: Try to find user by email
            var user = await _userManager.FindByEmailAsync(loginRequest.Email);
            if (user == null)
                return SignInResult.Failed;

            // Step 2: Check role and approval logic
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Contains("Agent") && !user.IsApproved)
            {
                // Optional: Add logging or extra logic if needed
                return SignInResult.NotAllowed;  // Block unapproved agent
            }

            // Step 3: Proceed to login check
            return await _signInManager.PasswordSignInAsync(
                user.Email,
                loginRequest.Password,
                loginRequest.RememberMe,
                lockoutOnFailure: false
            );

        }

        // Signs out the current user.
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // Retrieves the currently logged-in user entity, or null if none.
        public async Task<UserEntity> GetCurrentUserAsync()
        {
            var ctxUser = _httpContextAccessor.HttpContext?.User;
            if (ctxUser?.Identity?.IsAuthenticated == true)
                return await _userManager.GetUserAsync(ctxUser);
            return null;
        }

        // Gets all users as DTOs.
        public async Task<List<UserResponse>> GetAll()
        {
            var users = await _userManager.Users.ToListAsync();
            var userResponses = new List<UserResponse>();
            foreach (var user in users)
            {
                var roles = user != null ? await _userManager.GetRolesAsync(user) : new List<string>();
                userResponses.Add(new UserResponse
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    FullName = user.FullName,
                    Gender = user.Gender,
                    Address = user.Address,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    UserName = user.UserName,
                    DateRegistered = user.DateRegistered,
                    ProfilePhotoUrl = user.ProfilePhotoUrl,
                    IsApproved = user.IsApproved,
                    Role = roles.FirstOrDefault() ?? "NormalUser"
                });
            }
            return userResponses;
        }

        // Gets a single user by ID.
        public async Task<UserResponse> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Gender = user.Gender,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserName = user.UserName,
                DateRegistered = user.DateRegistered,
                ProfilePhotoUrl = user.ProfilePhotoUrl,
                IsApproved = user.IsApproved,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "NormalUser"
            };
        }

        // Updates non-sensitive user fields.
        public async Task<UserResponse> UpdateUser(string id, UserRequest request)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;
            // Only update allowed fields
            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.FullName = request.FullName;
            user.Gender = request.Gender;
            user.Address = request.Address;
            user.PhoneNumber = request.PhoneNumber;
            await _userManager.UpdateAsync(user);
            return new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Gender = user.Gender,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email,
                UserName = user.UserName,
                DateRegistered = user.DateRegistered,
                ProfilePhotoUrl = user.PhotoUrl,
                IsApproved = user.IsApproved,
                Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault() ?? "NormalUser"
            };
        }

        // Deletes a user by ID.
        public async Task<bool> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;
            var response = await _userManager.DeleteAsync(user);
            return response.Succeeded;
        }

        public async Task<List<UserResponse>> UnApproveAgents()
        {

            var result = await (from agent in _context.Users
                          join userRole in _context.UserRoles on agent.Id equals userRole.UserId
                          join role in _context.Roles on userRole.RoleId equals role.Id
                          where !agent.IsApproved && role.Name == "Agent"
                          select new UserResponse
                          {
                              Id = agent.Id,
                              FullName = agent.FullName,
                              Email = agent.Email,
                              DateRegistered = agent.DateRegistered,
                          }).OrderByDescending(x => x.DateRegistered).ToListAsync();

            return result;
        }

        public async Task<string> ApproveAgent(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            user.IsApproved = true;  // Mark as approved
            await _userManager.UpdateAsync(user);

            return "Agent approved successfully.";
        }
    }
}
