using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using OTP.BussinesInterfaces.ModelLogic;
using OTP.Domain;
using OTP.Domain.Models;
using OTP.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Authentication;


namespace OTP.Controllers
{

    public class AccountController : Controller
    {
        private readonly IUserLogic _userLogic;
        public AccountController(IUserLogic userLogic)
        {
            _userLogic = userLogic;
        }

        [AllowAnonymous]
        public IActionResult Login()
        {
           if (!User.Identity.IsAuthenticated)
                return View();
            return RedirectToAction("Home", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(UserLogin user)
        {
            if (!_userLogic.IsValidUser(user.UserName, user.Password))
            {
                ModelState.AddModelError("LoginFail", "Invalid username and password!");
                return View(user);
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name,user.UserName),
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookie"));

            await HttpContext.Authentication.SignInAsync("Cookies", principal,
                new AuthenticationProperties()
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(500),
                    IsPersistent = false
                });

            return View(user);
        }
        
        public IActionResult Forbidden()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.Authentication.SignOutAsync("Cookies");
            return RedirectToAction("Login", "Account");
        }

        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(UserRegister model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                UserName = model.UserName,
                CompleteName = model.CompleteName,
                Password = model.Password,
                Email = model.Email,
            };

            using (var context = new OTPContext())
            {
                context.Users.Add(user);

                context.SaveChanges();
            }
            return RedirectToAction("Login", "Account");
        }
    }
}

