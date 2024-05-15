using GameVerseForAdmin.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using MySqlConnector;

namespace GameVerseForAdmin.Controllers
{
    public class HomeController : Controller
    {
        private MySqlConnection cn = new MySqlConnection(@"server=192.168.56.1;Port=3307;uid=admin;pwd=1234;database=gameverse;");
        private MySqlCommand cmd;
        private MySqlDataReader dr;

        public HomeController()
        {
            try
            {
                cn.Close();
                cn.Open();
            }
            catch (Exception ex) { Console.WriteLine($"�������� ���������� � ��: {0}", ex.Message); }
        }

        public ViewResult Authorization()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Authorization(string Login, string Password)
        {
            if (Login == null || !Regex.IsMatch(Login, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.Message = "������������ ����� ����������� �����";
                return View();
            }

            if (Password == null || Password.Length < 8 || Password.Length > 15 || string.IsNullOrWhiteSpace(Password))
            {
                ViewBag.Message = "������ ������ ��������� ��� ������� 8 �������� � �������� 15";
                return View();
            }

            if (!Regex.IsMatch(Password, "[a-zA-Z]"))
            {
                ViewBag.Message = "������ ������ ��������� ���� �� ���� �����";
                return View();
            }

            if (!Regex.IsMatch(Password, "[0-9]"))
            {
                ViewBag.Message = "������ ������ ��������� ���� �� ���� �����";
                return View();
            }

            if (Password.Contains(' '))
            {
                ViewBag.Message = "������ �� ������ ��������� �������";
                return View();
            }

            try
            {
                dr.Close();
                cmd = new MySqlCommand("SELECT * FROM users WHERE Login='" + Login + "' and Password='" + GetHashString(Password) + "'", cn);
                dr = cmd.ExecuteReader();
            }
            catch
            {
                cmd = new MySqlCommand("SELECT * FROM users WHERE Login='" + Login + "' and Password='" + GetHashString(Password) + "'", cn);
                dr = cmd.ExecuteReader();
            }

            if (!dr.HasRows)
            {
                ViewBag.Message = "������ ������������ �� ����������!";
                return View();
            } else
            {
                while (dr.Read())
                {
                    if (Convert.ToBoolean(dr["IsAdmin"]))
                    {
                        ViewBag.Message = "����� ����������, " + Login + "!";

                        return Redirect("~/Games/ShopList");
                    }
                }

                ViewBag.Message = "�� �� ��������� ����������� �������������";
                return View();
            }
        }

        public string GetHashString(string s)
        {
            byte[] bytes = Encoding.Unicode.GetBytes(s);
            MD5CryptoServiceProvider CSP = new MD5CryptoServiceProvider();
            byte[] byteHash = CSP.ComputeHash(bytes);
            string hash = "";
            foreach (byte b in byteHash)
            {
                hash += string.Format("{0:x2}", b);
            }
            return hash;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
