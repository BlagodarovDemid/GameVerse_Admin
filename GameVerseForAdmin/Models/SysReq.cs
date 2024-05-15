namespace GameVerseForAdmin.Models
{
    public class SysReq
    {
        public int GameId { get; set; }
        public string OS { get; set; }
        public string? CPUmin { get; set; }
        public string? CPUreq { get; set; }
        public string? RAMmin { get; set; }
        public string? RAMreq { get; set; }
        public string? GPUmin { get; set; }
        public string? GPUreq { get; set; }
        public string? Space { get; set; }
    }
}
