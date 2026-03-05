using CheckersWeb.Data;
using CheckersWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace CheckersWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserDbContex _context;

        public HomeController(ILogger<HomeController> logger, UserDbContex context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var openGames = await _context.Games
                .Include(g => g.Player1)
                .Where(g => g.Player2Id == null && g.WinnerId == null)
                .OrderByDescending(g => g.Id)
                .ToListAsync();

            var leaderboard = await _context.Games
                .Where(g => g.WinnerId != null)
                .Include(g => g.Winner)
                .GroupBy(g => new { g.WinnerId, g.Winner!.Username })
                .Select(g => new { Username = g.Key.Username, Wins = g.Count() })
                .OrderByDescending(g => g.Wins)
                .ToListAsync();

            ViewBag.Leaderboard = leaderboard;

            return View(openGames);
        }

        public IActionResult Login() => View();

        public IActionResult createUser() => View();

        public IActionResult HelloUser()
        {
            ViewData["Name"] = TempData["UserName"] ?? "Guest";
            return View();
        }

        public async Task<IActionResult> loginForm(User model)
        {
            var user = _context.users
                .FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim("FullName", user.Name ?? user.Username)
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult createUserForm(User model)
        {
            if (ModelState.IsValid)
            {
                _context.users.Add(model);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("createUser", model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}