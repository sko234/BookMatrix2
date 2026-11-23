namespace BookMatrix.Models;

public class HomeIndexViewModel
{
    public BookPageDto Initial { get; set; } = new();
    public int PerPage { get; set; }
    public bool CanBorrow { get; set; }
}
