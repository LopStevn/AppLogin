using Microsoft.AspNetCore.Mvc;

using AppLogin.Data;
using AppLogin.Models;
using Microsoft.EntityFrameworkCore;
using AppLogin.ViewModels;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;


namespace AppLogin.Controllers
{
    public class AccessController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        public AccessController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Home", "Index");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SignIn(UserViewModel model)
        {
            if(model.Password != model.ConfirmPassword)
            {
                ViewData["Message"] = "They passwords are different. Please use the same password in 'Confirm Password'";
                return View();
            }

            User pUser = new User()
            {
                Name = model.Name,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
            };

            await _dbContext.Users.AddAsync(pUser);
            await _dbContext.SaveChangesAsync();

            if (pUser.Id != 0)
                return RedirectToAction("Login", "Access");

            ViewData["Message"] = "The user could not be created :(";

            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Home", "Index");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            User found_user = await _dbContext.Users.
                Where(u => u.Email == model.Email && u.Password == model.Password)
                .FirstOrDefaultAsync();

            if(found_user == null)
            {
                ViewData["Message"] = "No coincidences were found";
                return View();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, $"{found_user.Name} {found_user.LastName}")
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );

            return RedirectToAction("Index", "Home");
        }
    }
}
