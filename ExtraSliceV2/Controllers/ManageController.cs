using ExtraSliceV2.Extensions;
using ExtraSliceV2.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MVCApiExtraSlice.Services;
using System.Security.Claims;

namespace ExtraSliceV2.Controllers
{
    public class ManageController : Controller
    {
        private ServiceRestaurante service;
        public ManageController(ServiceRestaurante service)
        {
            this.service = service;
        }

        public IActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(string email, string password)
        {
            string token = await this.service.GetTokenAsync(email, password);
            if (token == null)
            {
                ViewData["MENSAJE"] = "Usuario/Password incorrectos";
                return View();
            }else{

                HttpContext.Session.SetString("TOKEN", token);
                ClaimsIdentity identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                identity.AddClaim(new Claim(ClaimTypes.Name, email));
               // identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, password));
                ClaimsPrincipal usePrincipal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, usePrincipal, new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30)
                });
                string controller = TempData["controller"].ToString();
                string action = TempData["action"].ToString();
                return RedirectToAction(action, controller);

            }
            
        }

        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Remove("TOKEN");
            HttpContext.Session.Remove("IdProductos");
            return RedirectToAction("Index", "Carta");
        }
    }
}
