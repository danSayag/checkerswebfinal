using CheckersWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Move
{
    [Key]
    public int Id { get; set; }
    public int GameId { get; set; }
    [ForeignKey("GameId")]
    public Game Game { get; set; }
    public string From { get; set; }
    public string To { get; set; }
    public int MoveNumber { get; set; }       // order of the move
    public DateTime PlayedAt { get; set; }    // when it was made
}