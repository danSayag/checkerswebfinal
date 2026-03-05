namespace CheckersWeb.Models
{
    public class PieceInfo
    {
        public int Row { get; set; }
        public int Col { get; set; }
        public bool Exists { get; set; }
        public Piece? piece { get; set; }
    }
}
