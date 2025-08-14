
using Application.DTOs;
using Application.Interface;
using Domain;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Presentation.Controllers
{
    // Controller for administrative tasks: managing users, roles, and approvals.
    [Authorize(Roles = "Admin,Agent")]  // Both Admin and Agent can view user list/details
    public class AdminController : Controller
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IUserInterface _userService;
        private readonly AppDbContext _context;

        // Constructor injects managers and service
        public AdminController(
            UserManager<UserEntity> userManager,
            RoleManager<IdentityRole> roleManager,
            IUserInterface userService,
            AppDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _userService = userService;
            _context = dbContext;
        }

        // View all users in the system.
        // Agents have read-only; Admin can manage.
        [Authorize(Roles = "Admin,Agent")]
        [HttpGet]
        public async Task<IActionResult> AllUsers()
        {
            var users = await _userService.GetAll();
            return View(users);
        }

        // Get approval page for pending agents.
        // Admin only.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> ApproveAgent()
        {

            var agents = await _userService.UnApproveAgents();
            return View(agents);
        }

        // Approve an Agent account so they can login.
        // Admin only.
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> ApproveAgent(string id)
        {
            var result = await _userService.ApproveAgent(id);

            if (result == null)
            {
                return NotFound();
            }

            TempData["Success"] = result;
            return RedirectToAction("ApproveAgent");
        }

        // View details of a single user. Admin and Agent.
        [Authorize(Roles = "Admin,Agent")]
        [HttpGet]
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null) return NotFound();
            return View(user);
        }

        // Edit user details (non-sensitive). Admin and Agent.
        [Authorize(Roles = "Admin,Agent")]
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [Authorize(Roles = "Admin,Agent")]
        [HttpPost]
        public async Task<IActionResult> Edit(string id, UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            await _userService.UpdateUser(id, request);
            TempData["Success"] = "User updated successfully.";
            return RedirectToAction("AllUsers");
        }

        // Show delete confirmation. Admin only.
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // Handle user deletion. Admin only.
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var success = await _userService.DeleteUser(id);
            if (success)
                TempData["Success"] = "User deleted successfully.";
            return RedirectToAction("AllUsers");
        }
    }
}

