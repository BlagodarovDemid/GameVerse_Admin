namespace GameVerseForAdmin.Models
{
    public class Games_Info
    {
        public List<Games> Games { get; set; }
        public List<Images> Images { get; set; }
        public List<SysReq> SysReqs { get; set; }
        public List<Achievements>? Achievements { get; set; }
    }
}
