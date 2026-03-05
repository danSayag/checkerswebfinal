namespace CheckersWeb.Models
{
    public class Piece
    {
        public required string Color { get; set; } // "white" or "black"
        public bool IsKing { get; set; }
    }
}
