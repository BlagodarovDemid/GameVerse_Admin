using GameVerseForAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using MySqlConnector;
using Word = Microsoft.Office.Interop.Word;

namespace GameVerseForAdmin.Controllers
{
    public class StatController : Controller
    {
        private MySqlConnection cn = new MySqlConnection(@"server=192.168.56.1;Port=3307;uid=admin;pwd=1234;database=gameverse;");
        private MySqlCommand cmd;
        private MySqlDataReader dr;

        public StatController()
        {
            try
            {
                cn.Close();
                cn.Open();
            }
            catch (Exception ex) { Console.WriteLine($"Проблема соединения с БД: {0}", ex.Message); }
        }

        public ViewResult Statistic()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Statistic(string TotalSales, string CountOfSales, string CategorySales, string RegionSales, string AveragePrice, string CountOfClients, string CountOfFavouriteGames, string AdminsCom)
        {
            string sourceFilePath = "C:\\Users\\MSII\\GameVerse\\Statistic.docx";

            string outputFilePath = "C:\\Users\\MSII\\GameVerse\\StatisticComplete.docx";

            Word.Application wordApp = new Word.Application(); 
            Word.Document wordDoc = wordApp.Documents.Open(sourceFilePath);

            Word.Bookmark bookmark1 = wordDoc.Bookmarks["GamesTotalSales"]; 
            Word.Bookmark bookmark2 = wordDoc.Bookmarks["CountOfSales"];
            Word.Bookmark bookmark3 = wordDoc.Bookmarks["CategorySales"]; 
            Word.Bookmark bookmark4 = wordDoc.Bookmarks["RegionSales"];
            Word.Bookmark bookmark5 = wordDoc.Bookmarks["AveragePrice"]; 
            Word.Bookmark bookmark6 = wordDoc.Bookmarks["CountOfClients"];
            Word.Bookmark bookmark7 = wordDoc.Bookmarks["CountOfFavouriteGames"]; 
            Word.Bookmark bookmark8 = wordDoc.Bookmarks["AdminsCom"];

            bookmark1.Range.Text = TotalSales; 
            bookmark2.Range.Text = CountOfSales;
            bookmark3.Range.Text = CategorySales; 
            bookmark4.Range.Text = RegionSales;
            bookmark5.Range.Text = AveragePrice; 
            bookmark6.Range.Text = CountOfClients;
            bookmark7.Range.Text = CountOfFavouriteGames; 
            bookmark8.Range.Text = AdminsCom;

            wordDoc.SaveAs2(outputFilePath);

            wordDoc.Close(); 
            wordApp.Quit();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
