namespace GameVerseForAdmin.Models
{
    public class Games
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string? TitleDescription { get; set; }
        public string? Developers { get; set; }
        public string Category { get; set; }
        public int Price { get; set; }
        public DateTime? PublicDate { get; set; }
        public string? Installer { get; set; }
    }
}
