using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CheckersWeb.Models
{
    public class Game
    {
        [Key]
        public int Id { get; set; }

        public string InviteCode { get; set; } = Guid.NewGuid().ToString("N");

        // --- Player 1 (Host) ---
        public int Player1Id { get; set; }

        [ForeignKey("Player1Id")]
        public User Player1 { get; set; }

        // --- Player 2 (Guest) ---
        public int? Player2Id { get; set; }

        [ForeignKey("Player2Id")]
        public User? Player2 { get; set; } // Made nullable with '?'

        // --- Results ---
        public int? WinnerId { get; set; }
        [ForeignKey("WinnerId")]
        public User? Winner { get; set; }

        public int? LoserId { get; set; }
        [ForeignKey("LoserId")]
        public User? Loser { get; set; }

        // --- Game State ---
        public bool IsPlayer1Turn { get; set; }
        public bool IsPlayer2Turn { get; set; }
        public string? BoardJson { get; set; }
    }
}