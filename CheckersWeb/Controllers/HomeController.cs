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


        // Constructor for HomeController, initializes the logger and database context.
        public HomeController(ILogger<HomeController> logger, UserDbContex context)
        {
            _logger = logger;
            _context = context;
        }

        // Action method for the Index page, retrieves open games and leaderboard data, and passes it to the view.
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

            // Active games for logged-in user
            if (User.Identity!.IsAuthenticated)
            {
                var me = await _context.users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
                if (me != null)
                {
                    var userIds = new List<int>();

                    var myGames = await _context.Games
                        .Where(g => (g.Player1Id == me.Id || g.Player2Id == me.Id) && g.WinnerId == null)
                        .ToListAsync();

                    userIds = myGames
                        .SelectMany(g => new[] { g.Player1Id, g.Player2Id ?? 0 })
                        .Where(id => id != 0)
                        .Distinct()
                        .ToList();

                    var users = await _context.users
                        .Where(u => userIds.Contains(u.Id))
                        .ToDictionaryAsync(u => u.Id, u => u.Username);

                    ViewBag.MyGames = myGames.Select(g =>
                    {
                        bool isP1 = g.Player1Id == me.Id;
                        int opponentId = isP1 ? (g.Player2Id ?? 0) : g.Player1Id;
                        return new
                        {
                            g.Id,
                            Opponent = opponentId != 0 ? users.GetValueOrDefault(opponentId, "Unknown") : "Waiting...",
                            MyTurn = (isP1 && g.IsPlayer1Turn) || (!isP1 && g.IsPlayer2Turn)
                        };
                    }).ToList<dynamic>();
                }
            }

            return View(openGames);
        }

        // Action method for the Login page.
        public IActionResult Login() => View();


        // Action method for the Create User page.
        public IActionResult createUser() => View();


        // Action method for the HelloUser page, retrieves the user's name from TempData and passes it to the view.
        public IActionResult HelloUser()
        {
            ViewData["Name"] = TempData["UserName"] ?? "Guest";
            return View();
        }


        // Action method for handling the login form submission, checks the user's credentials and signs them in if valid.
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


        // Action method for handling the create user form submission, adds the new user to the database if the model is valid.
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


        // Action method for the Error page, returns an error view with the current request ID.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        // Action method for logging out the user, signs them out and redirects to the Index page.
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> QuitGame(int gameId)
        {
            var username = User.Identity?.Name;
            var me = await _context.users.FirstOrDefaultAsync(u => u.Username == username);
            if (me == null) return Unauthorized();

            var game = await _context.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null) return NotFound();

            if (game.Player1Id != me.Id && game.Player2Id != me.Id)
                return Forbid();

            if (game.WinnerId != null)
                return RedirectToAction("Index");

            bool isP1 = game.Player1Id == me.Id;
            game.LoserId = me.Id;
            game.WinnerId = isP1 ? game.Player2Id : game.Player1Id;

            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }



}