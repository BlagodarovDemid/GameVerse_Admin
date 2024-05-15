using GameVerseForAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Diagnostics;

namespace GameVerseForAdmin.Controllers
{
    public class ProfileController : Controller
    {
        private readonly List<Games> _games = new();
        private readonly List<Images> _images = new();
        private readonly List<SysReq> _sysReqs = new();
        private readonly List<Achievements> _achievements = new();
        private readonly List<Games_Info> _gamesInfo = new();

        private MySqlConnection cn = new MySqlConnection(@"server=192.168.56.1;Port=3307;uid=admin;pwd=1234;database=gameverse;");
        private MySqlCommand cmd;
        private MySqlDataReader dr;

        static Renci.SshNet.ConnectionInfo connection = new Renci.SshNet.ConnectionInfo("192.168.56.1", 22, "MSII", new PasswordAuthenticationMethod("MSII", "5689"));
        private SftpClient sftpClient = new SftpClient(connection);

        public ProfileController()
        {
            try
            {
                cn.Close();
                cn.Open();
            }
            catch (Exception ex) { Console.WriteLine($"Проблема соединения с БД: {0}", ex.Message); }

            try
            {
                sftpClient.Connect();
                sftpClient.Disconnect();
            }
            catch (Exception ex) { Console.WriteLine($"Проблема соединения с БД: {0}", ex.Message); }
        }

        [HttpPost]
        public IActionResult GameProfile(int GameID)
        {
            try
            {
                dr.Close();
                cmd = new MySqlCommand("SELECT game.*, sysrequirement.*, image.* FROM game INNER JOIN sysrequirement ON sysrequirement.Game_idGame = game.idGame INNER JOIN image ON image.Game_idGame = game.idGame WHERE game.idGame = " + GameID, cn);
                dr = cmd.ExecuteReader();
            }
            catch
            {
                cmd = new MySqlCommand("SELECT game.*, sysrequirement.*, image.* FROM game INNER JOIN sysrequirement ON sysrequirement.Game_idGame = game.idGame INNER JOIN image ON image.Game_idGame = game.idGame WHERE game.idGame = " + GameID, cn);
                dr = cmd.ExecuteReader();
            }

            while (dr.Read())
            {
                _games.Add(new Games
                {
                    Id = GameID,
                    Title = dr["Title"].ToString(),
                    TitleDescription = dr["TitleDescription"].ToString(),
                    Developers = dr["Developers"].ToString(),
                    Category = dr["Category"].ToString(),
                    Price = Convert.ToInt32(dr["Price"]),
                    PublicDate = Convert.ToDateTime(dr["PublicDate"])
                });

                _images.Add(new Images
                {
                    GameId = GameID,
                    Image1 = "data:image/png;base64," + Convert.ToBase64String(DownloadServerImageFromPath(dr["Pos1"].ToString())),
                    Image2 = "data:image/png;base64," + Convert.ToBase64String(DownloadServerImageFromPath(dr["Pos2"].ToString())),
                    Image3 = "data:image/png;base64," + Convert.ToBase64String(DownloadServerImageFromPath(dr["Pos3"].ToString())),
                    Image4 = "data:image/png;base64," + Convert.ToBase64String(DownloadServerImageFromPath(dr["Pos4"].ToString()))
                });

                _sysReqs.Add(new SysReq
                {
                    GameId = GameID,
                    OS = dr["OS"].ToString(),
                    CPUmin = dr["CPUmin"].ToString(),
                    RAMmin = dr["RAMmin"].ToString(),
                    GPUmin = dr["GPUmin"].ToString(),
                    CPUreq = dr["CPUreq"].ToString(),
                    RAMreq = dr["RAMreq"].ToString(),
                    GPUreq = dr["GPUreq"].ToString(),
                    Space = dr["Space"].ToString()
                });
            }
            dr.Close();

            Achievements_Appearing(GameID);

            _gamesInfo.Add(new Games_Info
            {
                Games = _games,
                Images = _images,
                SysReqs = _sysReqs,
                Achievements = _achievements
            });

            return View(_gamesInfo);
        }

        private void Achievements_Appearing(int GameID)
        {
            try
            {
                dr.Close();
                cmd = new MySqlCommand("SELECT achievements.* FROM achievements WHERE achievements.Game_idGame = " + GameID, cn);
                dr = cmd.ExecuteReader();
            }
            catch
            {
                cmd = new MySqlCommand("SELECT achievements.* FROM achievements WHERE achievements.Game_idGame = " + GameID, cn);
                dr = cmd.ExecuteReader();
            }

            while (dr.Read())
            {
                _achievements.Add(new Achievements
                {
                    GameId = GameID,
                    Image = "data:image/png;base64," + Convert.ToBase64String(DownloadServerImageFromPath(dr["Image"].ToString()))
                });
            }
            dr.Close();
        }

        private byte[] DownloadServerImageFromPath(string ImagePathInServer)
        {
            MemoryStream imageStream = new MemoryStream();
            try
            {
                sftpClient.Connect();
                sftpClient.DownloadFile(ImagePathInServer, imageStream);
                sftpClient.Disconnect();
                return imageStream.ToArray();
            }
            catch (SftpPathNotFoundException)
            {
                sftpClient.Disconnect();
                return Array.Empty<byte>();
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
