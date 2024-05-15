using GameVerseForAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace GameVerseForAdmin.Controllers
{
    public class GamesController : Controller
    {
        private readonly List<Games> _games = new();
        private readonly List<Images> _images = new();
        private readonly List<SysReq> _sysReqs = new();
        private readonly List<Games_Info> _gamesInfo = new();

        private MySqlConnection cn = new MySqlConnection(@"server=192.168.56.1;Port=3307;uid=admin;pwd=1234;database=gameverse;");
        private MySqlCommand cmd;
        private MySqlDataReader dr;

        static Renci.SshNet.ConnectionInfo connection = new Renci.SshNet.ConnectionInfo("192.168.56.1", 22, "MSII", new PasswordAuthenticationMethod("MSII", "5689"));
        private SftpClient sftpClient = new SftpClient(connection);

        public GamesController()
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

        public ViewResult ShopList()
        {
            GameSearch();

            return View(_gamesInfo);
        }

        [HttpPost]
        public IActionResult ShopList(string GameName)
        {
            GameSearch(GameName);

            return View(_gamesInfo);
        }

        private void GameSearch(string SearchText = null)
        {
            if (SearchText != null && SearchText != "") { SearchText = " WHERE game.Title LIKE '%" + SearchText + "%'"; }
            try
            {
                dr.Close();
                cmd = new MySqlCommand("SELECT game.*, sysrequirement.OS, image.Pos1 FROM game INNER JOIN sysrequirement ON sysrequirement.Game_idGame = game.idGame INNER JOIN image ON image.Game_idGame = game.idGame" + SearchText, cn);
                dr = cmd.ExecuteReader();
            }
            catch
            {
                cmd = new MySqlCommand("SELECT game.*, sysrequirement.OS, image.Pos1 FROM game INNER JOIN sysrequirement ON sysrequirement.Game_idGame = game.idGame INNER JOIN image ON image.Game_idGame = game.idGame" + SearchText, cn);
                dr = cmd.ExecuteReader();
            }

            while (dr.Read())
            {
                int idGame = Convert.ToInt32(dr["idGame"]);
                string ImageServerPath = dr["Pos1"].ToString();

                _games.Add(new Games
                {
                    Id = idGame,
                    Title = dr["Title"].ToString(),
                    Category = dr["Category"].ToString(),
                    Price = Convert.ToInt32(dr["Price"]),
                });

                _images.Add(new Images
                {
                    GameId = idGame,
                    Image1 = "data:image/png;base64," + Convert.ToBase64String(DownloadServerImageFromPath(ImageServerPath))
                });

                _sysReqs.Add(new SysReq
                {
                    GameId = idGame,
                    OS = CheckOS(dr["OS"].ToString()) == true ? "Apple" : "Windows"
                });
            }
            dr.Close();

            _gamesInfo.Add(new Games_Info
            {
                Games = _games,
                Images = _images,
                SysReqs = _sysReqs
            });
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

        private bool CheckOS(string OS)
        {
            string[] AppleOS = { "OS" };
            foreach (string check in AppleOS)
            {
                if (OS.Contains(check))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
