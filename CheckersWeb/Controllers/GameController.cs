using CheckersWeb.Data;
using CheckersWeb.Models;
using CheckersWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CheckersWeb.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly UserDbContex _db;

        public GameController(UserDbContex db)
        {
            _db = db;
        }

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

        [HttpGet]
        public async Task<IActionResult> Invite(int gameId)
        {
            var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null) return NotFound();

            ViewBag.GameId = game.Id;
            ViewBag.InviteUrl = $"{Request.Scheme}://{Request.Host}/Game/Join/{game.InviteCode}";
            return View();
        }

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

        [HttpGet]
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
    }
}