using Application.DTOs;
using Application.Interface;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    // Requires authenticated user by default; specific actions allow anonymous
    [Authorize]
    public class AuthController : Controller
    {
        private readonly IUserInterface _userService;
        private readonly SignInManager<UserEntity> _signInManager;
        private readonly UserManager<UserEntity> _userManager;
        private readonly IWebHostEnvironment _env;

        // Inject dependencies: service for logic, identity managers for roles, hosting for file paths
        public AuthController(
            IUserInterface userService,
            SignInManager<UserEntity> signInManager,
            UserManager<UserEntity> userManager,
            IWebHostEnvironment env)
        {
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
            _env = env;  // used for profile photo path
        }

        // GET: /Auth/Login  - anyone can view login page
        [HttpGet, AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginRequest());
        }

        // POST: /Auth/Login  - process login
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Login(LoginRequest request, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }
            var result = await _userService.LoginAsync(request);
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null && await _userManager.IsInRoleAsync(user, "Agent") && !user.IsApproved)
            {
                return RedirectToAction("AccessDenied", "Auth");
            }
            // Any other failure
            if (!result.Succeeded)
            {
                ViewBag.Error = "Invalid email or password.";
                return View(request);
            }



            // If a safe returnUrl is provided, redirect there
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            // Otherwise send to role-specific landing page
            if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                return RedirectToAction("AllUsers", "Admin");
            }
            if (await _userManager.IsInRoleAsync(user, "Agent"))
            {
                return RedirectToAction("Index", "Payment"); //("AgentDashboard", "Dashboard");
            }
            // NormalUser fallback
            return RedirectToAction("Index", "Store");
        }

        // GET: /Auth/Register  - anyone can view registration
        [HttpGet, AllowAnonymous]
        public IActionResult Register()
        {
            return View(new RegisterRequest());
        }

        // POST: /Auth/Register  - process registration
        [HttpPost, AllowAnonymous]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return View(request);

            // Password match validation
            if (request.Password != request.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                return View(request);
            }

            // Delegate creation to service (handles roles, approval flag, photo)
            var result = await _userService.RegisterAsync(request);
            if (!result.Succeeded)
            {
                foreach (var err in result.Errors)
                    ModelState.AddModelError("", err.Description);
                return View(request);
            }

            // Inform user of next steps
            TempData["Success"] = request.Role == "Agent"
                ? "Registration successful, awaiting admin approval."
                : "Registration successful, you may now log in.";

            return RedirectToAction("Login");
        }

        // POST or GET: /Auth/Logout  - logs out any authenticated user
        [HttpPost, HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _userService.LogoutAsync();
            return RedirectToAction("Login");
        }

        // GET: /Auth/AccessDenied - show access denied page
        [HttpGet]
        public IActionResult AccessDenied(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }
    }
}