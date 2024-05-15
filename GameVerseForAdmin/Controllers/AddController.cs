using GameVerseForAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;
using Renci.SshNet;
using Renci.SshNet.Common;
using System.Diagnostics;
using System.IO;

namespace GameVerseForAdmin.Controllers
{
    public class AddController : Controller
    {
        private readonly List<Games> _games = new();
        private readonly List<Images> _images = new();
        private readonly List<SysReq> _sysReqs = new();
        private readonly List<Achievements> _achievements = new();
        private readonly List<Games_Info> _gamesInfo = new();

        private string CurrentGameID;
        private string CurrentGamePath;

        private MySqlConnection cn = new MySqlConnection(@"server=192.168.56.1;Port=3307;uid=admin;pwd=1234;database=gameverse;");
        private MySqlCommand cmd;
        private MySqlDataReader dr;

        static Renci.SshNet.ConnectionInfo connection = new Renci.SshNet.ConnectionInfo("192.168.56.1", 22, "MSII", new PasswordAuthenticationMethod("MSII", "5689"));
        private SftpClient sftpClient = new SftpClient(connection);

        public AddController()
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

        public ViewResult AddForm()
        {

            return View();
        }

        [HttpPost]
        public IActionResult AddForm(string Image)
        {
            ViewData["Image"] = Image;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            var files = Request.Form.Files;

            var gameAreas = new Dictionary<string, string>();
            var imageAreas = new Dictionary<string, string>();
            var achievementsAreas = new Dictionary<string, string>();
            var sysReqAreas = new Dictionary<string, string>();


            if (files.Count != 4 || !files.All(f => IsImage(f)))
            {
                return BadRequest("Выберите сразу 4 изображения: \n1: Изображение-заголовок \n2: С 2-4 изображения в ленте");
            }

            foreach (var key in Request.Form.Keys)
            {
                if(key == "GameTitle") { gameAreas.Add(key, Request.Form[key]); }
                if(key == "GameTitleDescription") { gameAreas.Add(key, Request.Form[key]); }
                if(key == "GameDevelopers") { gameAreas.Add(key, Request.Form[key]); }
                if(key == "GamePrice") { gameAreas.Add(key, Request.Form[key]); }
                if(key == "GamePublicDate") { gameAreas.Add(key, Request.Form[key]); }

                if(key == "GameGPUreq") { sysReqAreas.Add(key, Request.Form[key]); }
                if(key == "GameRAMreq") { sysReqAreas.Add(key, Request.Form[key]); }
                if(key == "GameCPUreq") { sysReqAreas.Add(key, Request.Form[key]); }
                if(key == "GameSpace") { sysReqAreas.Add(key, Request.Form[key]); }
                if (key == "GameGPUmin") { sysReqAreas.Add(key, Request.Form[key]); }
                if (key == "GameRAMmin") { sysReqAreas.Add(key, Request.Form[key]); }
                if (key == "GameCPUmin") { sysReqAreas.Add(key, Request.Form[key]); }
                if (key == "GameOS") { sysReqAreas.Add(key, Request.Form[key]); }
            }

            //SysReqUpload(sysReqAreas);
            GameUpload(gameAreas);



            var imageStreams = new List<MemoryStream>();
            foreach (var file in files)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    imageStreams.Add(memoryStream);
                }
            }




            
            /*            var remotePath = @"\GameVerse\Avatars\" + UserServerDictionaryName;
            avatarPath = remotePath + @"\Avatar.png";
            if (((App)Application.Current).IsDirectoryInServerExists(remotePath))
            {
                try
                {
                    ((App)Application.Current).sftpClient.Connect();
                    ((App)Application.Current).sftpClient.UploadFile(new MemoryStream(scaledImageBytes), avatarPath, true);
                    ((App)Application.Current).sftpClient.Disconnect();
                }
                catch { ((App)Application.Current).sftpClient.Disconnect(); }
            }
            else
            {
                try
                {
                    ((App)Application.Current).sftpClient.Connect();
                    ((App)Application.Current).sftpClient.CreateDirectory(remotePath);
                    ((App)Application.Current).sftpClient.UploadFile(new MemoryStream(scaledImageBytes), avatarPath, true);

                    try
                    {
                        ((App)Application.Current).dr.Close();
                        ((App)Application.Current).cmd = new MySqlCommand("UPDATE users SET users.Image='" + @"\\GameVerse\\Avatars\\" + UserServerDictionaryName + @"\\Avatar.png" + "' WHERE users.idUser='" + ID + "'", ((App)Application.Current).cn);
                        await ((App)Application.Current).cmd.ExecuteNonQueryAsync();
                    }
                    catch
                    {
                        ((App)Application.Current).cmd = new MySqlCommand("UPDATE users SET users.Image='" + @"\\GameVerse\\Avatars\\" + UserServerDictionaryName + @"\\Avatar.png" + "' WHERE users.idUser='" + ID + "'", ((App)Application.Current).cn);
                        await ((App)Application.Current).cmd.ExecuteNonQueryAsync();
                    }
                    ((App)Application.Current).sftpClient.Disconnect();
                }
                catch { ((App)Application.Current).sftpClient.Disconnect(); }
            }*/

            // Выполняем необходимые действия с изображениями и значениями из элементов <textarea>.

            return View();
        }

        private void GameUpload(Dictionary<string, string> gameAreas)
        {
            var game = gameAreas.Values.ToArray();

            string Title = game[0].ToString();
            string TitleDescription = game[4].ToString();
            string Developers = game[2].ToString();
            string Category = "Приключенческие игры, Инди";
            string Price = game[1].ToString();
            string PublicDate = game[3].ToString();

            if (Title == null || Title == "") { return; }
            if (TitleDescription == null || TitleDescription == "") { return; }
            if (Developers == null || Developers == "") { return; }
            if (Category == null || Category == "") { return; }
            if (Price == null || Price == "" || Price == "руб.") { Price = "0"; }
            if (PublicDate == null || PublicDate == "") { return; }

            try
            {
                dr.Close();
                cmd = new MySqlCommand("SELECT game.* FROM game WHERE game.Title = '" + Title + "'", cn);
                dr = cmd.ExecuteReader();
            }
            catch
            {
                cmd = new MySqlCommand("SELECT game.* FROM game WHERE game.Title = '" + Title + "'", cn);
                dr = cmd.ExecuteReader();
            }
            if (!dr.HasRows)
            {
                try
                {
                    dr.Close();
                    cmd = new MySqlCommand("INSERT INTO game VALUES(default,'" + Title + "','" + TitleDescription + "','" + Developers + "','" + Category + "','" + Price.Replace("руб.", "") + "','" + PublicDate + "','')", cn);
                    dr = cmd.ExecuteReader();
                }
                catch
                {
                    cmd = new MySqlCommand("INSERT INTO game VALUES(default,'" + Title + "','" + TitleDescription + "','" + Developers + "','" + Category + "','" + Price.Replace("руб.", "") + "','" + PublicDate + "','')", cn);
                    dr = cmd.ExecuteReader();
                }

                try
                {
                    dr.Close();
                    cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", cn);
                    CurrentGameID = cmd.ExecuteScalar().ToString(); ;
                }
                catch
                {
                    cmd = new MySqlCommand("SELECT LAST_INSERT_ID()", cn);
                    CurrentGameID = cmd.ExecuteScalar().ToString();
                }

                CurrentGamePath = CurrentGameID + "_" + Title;
                string Installer = @"GameVerse\\Games\\" + CurrentGamePath + @"\\Installer.exe";
                try
                {
                    dr.Close();
                    cmd = new MySqlCommand("UPDATE game SET game.Installer = '" + Installer + "' WHERE game.idGame = '" + CurrentGameID + "'", cn);
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    cmd = new MySqlCommand("UPDATE game SET game.Installer = '" + Installer + "' WHERE game.idGame = '" + CurrentGameID + "'", cn);
                    cmd.ExecuteNonQuery();
                }

                try
                {
                    var remotePath = @"\GameVerse\Games\" + CurrentGamePath;
                    sftpClient.Connect();
                    sftpClient.CreateDirectory(remotePath);
                    sftpClient.Disconnect();
                }
                catch { sftpClient.Disconnect(); }
            }
        }

        private void SysReqUpload(Dictionary<string, string> sysReqAreas)
        {
            var game = sysReqAreas.Values.ToArray();

            string OS = game[7].ToString();
            string CPUmin = game[6].ToString();
            string RAMmin = game[5].ToString();
            string GPUmin = game[4].ToString();
            string CPUreq = game[2].ToString();
            string RAMreq = game[1].ToString();
            string GPUreq = game[0].ToString();
            string Space = game[3].ToString();

            if (OS == null || OS == "") { return; }


        }

        private bool IsImage(IFormFile file)
        {
            return file.ContentType.Contains("image/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
