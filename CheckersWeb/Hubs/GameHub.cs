using CheckersWeb.Data;
using CheckersWeb.Models;
using CheckersWeb.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace CheckersWeb.Hubs
{
    [Authorize]
    public class GameHub : Hub
    {
        private readonly UserDbContex _db;

        public GameHub(UserDbContex db)
        {
            _db = db;
        }

        public override Task OnConnectedAsync()
        {
            var username = Context.User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
                ConnectedUsers.Map[Context.ConnectionId] = username;
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            ConnectedUsers.Map.TryRemove(Context.ConnectionId, out _);
            return base.OnDisconnectedAsync(exception);
        }

        private static string GroupName(int gameId) => $"game-{gameId}";

        private static Piece?[][] CreateInitialBoard()
        {
            var board = new Piece?[8][];
            for (int r = 0; r < 8; r++)
            {
                board[r] = new Piece?[8];
                for (int c = 0; c < 8; c++)
                {
                    if ((r + c) % 2 == 0)
                    {
                        if (r < 3)
                            board[r][c] = new Piece { Color = "black", IsKing = false };
                        else if (r > 4)
                            board[r][c] = new Piece { Color = "white", IsKing = false };
                    }
                }
            }
            return board;
        }

        public async Task JoinGame(int gameId)
        {
            var username = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) throw new HubException("Not logged in.");

            var me = await _db.users.FirstOrDefaultAsync(u => u.Username == username);
            if (me == null) throw new HubException("User not found.");

            var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null) throw new HubException("Game not found.");

            if (game.Player1Id != me.Id && game.Player2Id != me.Id)
                throw new HubException("You are not a player in this game.");

            if (string.IsNullOrWhiteSpace(game.BoardJson))
            {
                game.BoardJson = JsonSerializer.Serialize(CreateInitialBoard());
                await _db.SaveChangesAsync();
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(gameId));
            await Clients.Group(GroupName(gameId)).SendAsync("PlayerJoined", me.Username);

            var board = JsonSerializer.Deserialize<Piece?[][]>(game.BoardJson);
            var currentTurn = game.IsPlayer1Turn ? "white" : "black";
            var camelOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var boardForClient = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(board, camelOptions));
            await Clients.Caller.SendAsync("LoadBoard", boardForClient, currentTurn);
        }

        public async Task MakeMove(int gameId, int fromR, int fromC, int toR, int toC)
        {
            var username = Context.User?.Identity?.Name;
            if (string.IsNullOrWhiteSpace(username)) throw new HubException("Not logged in.");

            var me = await _db.users.FirstOrDefaultAsync(u => u.Username == username);
            if (me == null) throw new HubException("User not found.");

            var game = await _db.Games.FirstOrDefaultAsync(g => g.Id == gameId);
            if (game == null) throw new HubException("Game not found.");

            bool isP1 = game.Player1Id == me.Id;
            bool isP2 = game.Player2Id == me.Id;
            if (!isP1 && !isP2) throw new HubException("Not your game.");

            bool myTurn = (isP1 && game.IsPlayer1Turn) || (isP2 && game.IsPlayer2Turn);
            if (!myTurn) throw new HubException("Not your turn.");

            if (string.IsNullOrWhiteSpace(game.BoardJson)) throw new HubException("Board not initialized.");

            var board = JsonSerializer.Deserialize<Piece?[][]>(game.BoardJson)
                ?? throw new HubException("Failed to deserialize board.");

            if (fromR < 0 || fromR > 7 || fromC < 0 || fromC > 7 || toR < 0 || toR > 7 || toC < 0 || toC > 7)
                throw new HubException("Out of bounds.");

            var piece = board[fromR][fromC] ?? throw new HubException("No piece at source.");

            string myColor = isP1 ? "white" : "black";
            if (piece.Color != myColor) throw new HubException("Not your piece.");
            if (board[toR][toC] != null) throw new HubException("Destination not empty.");

            int dRow = toR - fromR;
            int dCol = toC - fromC;
            int absRow = Math.Abs(dRow);
            int absCol = Math.Abs(dCol);

            if (absRow != absCol) throw new HubException("Must move diagonally.");

            if (piece.IsKing)
            {
                int stepR = dRow > 0 ? 1 : -1;
                int stepC = dCol > 0 ? 1 : -1;
                int currR = fromR + stepR, currC = fromC + stepC;
                Piece? captured = null;
                int capturedR = -1, capturedC = -1;

                while (currR != toR)
                {
                    var sq = board[currR][currC];
                    if (sq != null)
                    {
                        if (sq.Color == piece.Color) throw new HubException("Cannot jump over own piece.");
                        if (captured != null) throw new HubException("Cannot capture more than one piece per jump.");
                        captured = sq; capturedR = currR; capturedC = currC;
                    }
                    currR += stepR; currC += stepC;
                }

                if (captured != null) board[capturedR][capturedC] = null;
            }
            else
            {
                int forwardDir = myColor == "white" ? -1 : 1;
                if (absRow == 1)
                {
                    if (dRow != forwardDir) throw new HubException("Normal pieces can only move forward.");
                }
                else if (absRow == 2)
                {
                    int midR = (fromR + toR) / 2, midC = (fromC + toC) / 2;
                    var jumped = board[midR][midC] ?? throw new HubException("No piece to jump over.");
                    if (jumped.Color == piece.Color) throw new HubException("Cannot capture own piece.");
                    board[midR][midC] = null;
                }
                else throw new HubException("Normal pieces can only move 1 or 2 squares.");
            }

            board[toR][toC] = piece;
            board[fromR][fromC] = null;

            if (!piece.IsKing &&
                ((piece.Color == "white" && toR == 0) || (piece.Color == "black" && toR == 7)))
            {
                piece.IsKing = true;
            }

            game.IsPlayer1Turn = !game.IsPlayer1Turn;
            game.IsPlayer2Turn = !game.IsPlayer2Turn;
            game.BoardJson = JsonSerializer.Serialize(board);

            string opponentColor = myColor == "white" ? "black" : "white";
            int opponentPieces = board.SelectMany(row => row).Count(p => p?.Color == opponentColor);
            bool gameOver = opponentPieces == 0;
            if (gameOver)
            {
                game.WinnerId = me.Id;
                game.LoserId = isP1 ? game.Player2Id : game.Player1Id;
            }

            await _db.SaveChangesAsync();

            var camelOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var boardForClient = JsonSerializer.Deserialize<object>(JsonSerializer.Serialize(board, camelOptions));

            await Clients.Group(GroupName(gameId)).SendAsync("MoveMade", new
            {
                gameId,
                fromR,
                fromC,
                toR,
                toC,
                by = me.Username,
                board = boardForClient,
                nextTurn = game.IsPlayer1Turn ? "white" : "black",
                winner = gameOver ? me.Username : (string?)null
            });
        }
    }
}