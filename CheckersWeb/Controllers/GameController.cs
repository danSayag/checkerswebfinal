using CheckersWeb.Data;
using CheckersWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CheckersWeb.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly CheckersDbContext _db;


        //allow us to access the database context to perform operations related to game sessions and user data.
        public GameController(CheckersDbContext db)
        {
            _db = db;
        }


        // This method allows a logged-in user to start a new game session.
        // It creates a new game record in the database with the current user as Player 1 and generates a unique invite code for others to join.
        // After saving the game, it redirects the user to the game view.
        [HttpGet]
        public async Task<IActionResult> Start()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

            var me = await _db.users.FirstOrDefaultAsync(u => u.Username == username);
            if (me == null) return Unauthorized();

            var game = new Game
            {
                Player1Id = me.Id,
                Player2Id = null,
                IsPlayer1Turn = true,
                IsPlayer2Turn = false,
                InviteCode = Guid.NewGuid().ToString("N"),
            };

            _db.Games.Add(game);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { gameId = game.Id });
        }


        // This method displays the invite page for a specific game session.
        // It retrieves the game details using the provided game ID and generates an invite URL that can be shared with others to join the game.
        [HttpGet]
        public async Task<IActionResult> Invite(int gameId)
        {
            var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null) return NotFound();

            ViewBag.GameId = game.Id;
            ViewBag.InviteUrl = $"{Request.Scheme}://{Request.Host}/Game/Join/{game.InviteCode}";
            return View();
        }


        // This method allows users to join an existing game session using an invite code.
        [AllowAnonymous]
        [HttpGet("/Game/Join/{code}")]
        public async Task<IActionResult> Join(string code)
        {
            var game = await _db.Games
                .Include(g => g.Player1)
                .FirstOrDefaultAsync(g => g.InviteCode == code);
            if (game == null) return NotFound("Invite not found.");

            ViewBag.GameId = game.Id;
            ViewBag.Code = code;
            ViewBag.HostName = game.Player1?.Username ?? "Someone";
            return View();
        }


        // This method processes the acceptance of a game invite.
        [HttpPost("/Game/AcceptInvite/{code}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptInvite(string code)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

            var me = await _db.users.FirstOrDefaultAsync(u => u.Username == username);
            if (me == null) return Unauthorized();

            var game = await _db.Games.FirstOrDefaultAsync(g => g.InviteCode == code);
            if (game == null) return NotFound("Invite not found.");

            if (game.Player1Id == me.Id || game.Player2Id == me.Id)
                return RedirectToAction("Index", new { gameId = game.Id });

            if (game.Player2Id != null)
                return BadRequest("This game already has 2 players.");

            game.Player2Id = me.Id;
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", new { gameId = game.Id });
        }


        // This method displays the main game view for a specific game session.
        [HttpGet]
        [Route("Game")]
        [Route("Game/Index")]
        public async Task<IActionResult> Index(int gameId)
        {
            var game = await _db.Games
                .Include(g => g.Player1)
                .Include(g => g.Player2)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null) return NotFound("Game session not found.");

            var username = User.Identity?.Name;
            var me = await _db.users.FirstOrDefaultAsync(u => u.Username == username);

            ViewData["User1"] = game.Player1?.Username ?? "Player 1";
            ViewData["User2"] = game.Player2?.Username ?? "Waiting for opponent...";
            ViewBag.GameId = gameId;
            ViewBag.MyColor = (me?.Id == game.Player1Id) ? "white" : "black";
            ViewBag.WinnerId = game.WinnerId;
            ViewBag.InviteUrl = $"{Request.Scheme}://{Request.Host}/Game/Join/{game.InviteCode}";

            return View(game);
        }


        [HttpGet]
        public async Task<IActionResult> History()
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

            var me = await _db.users.FirstOrDefaultAsync(u => u.Username == username);
            if (me == null) return Unauthorized();

            var gameHistory = await _db.Games
                .Include(g => g.Player1)
                .Include(g => g.Player2)
                .Include(g => g.Moves)
                .Where(g => g.Player1Id == me.Id || g.Player2Id == me.Id)
                .OrderByDescending(g => g.StartedAt)
                .ToListAsync();

            return View(gameHistory);
        }

        // delete game if no one other the the user who created it joined
        [HttpPost]
        public async Task<IActionResult> Delete(int gameId)
        {
            var username = User.Identity?.Name;
            var me = await _db.users.FirstOrDefaultAsync(u => u.Username == username);
            if (me == null) return Unauthorized();

            var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null) return NotFound();

            // Only host can delete, and only if no one joined
            if (game.Player1Id != me.Id) return Forbid();
            if (game.Player2Id != null) return BadRequest("Cannot delete a game that already has an opponent.");

            _db.Games.Remove(game);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        // Shows the move history for a completed game
        [HttpGet]
        public async Task<IActionResult> Details(int gameId)
        {
            var username = User.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) return Unauthorized();

            var me = await _db.users.FirstOrDefaultAsync(u => u.Username == username);
            if (me == null) return Unauthorized();

            var game = await _db.Games
                .Include(g => g.Player1)
                .Include(g => g.Player2)
                .Include(g => g.Winner)
                .Include(g => g.Moves)
                .FirstOrDefaultAsync(g => g.Id == gameId);

            if (game == null) return NotFound();

            // Only players of this game can view it
            if (game.Player1Id != me.Id && game.Player2Id != me.Id)
                return Forbid();

            return View(game);
        }
    }
}