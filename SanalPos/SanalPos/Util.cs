using System;
using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;

namespace SanalPos
{
    public static class Util
    {
        #region Public Methods
        public static string CreateRandomValue(int Length, bool CharactersB, bool CharactersS, bool Numbers, bool SpecialCharacters)
        {
            string characters_b = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string characters_s = "abcdefghijklmnopqrstuvwxyz";
            string numbers = "0123456789";
            string special_characters = "-_*+/";
            string allowedChars = String.Empty;

            if (CharactersB)
                allowedChars += characters_b;

            if (CharactersS)
                allowedChars += characters_s;

            if (Numbers)
                allowedChars += numbers;

            if (SpecialCharacters)
                allowedChars += special_characters;

            char[] chars = new char[Length];
            Random rd = new Random();

            for (int i = 0; i < Length; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }

            return new string(chars);
        }

        public static string GetSHA1(string SHA1Data)
        {
            SHA1 sha = new SHA1CryptoServiceProvider();
            string HashedPassword = SHA1Data;
            byte[] hashbytes = Encoding.GetEncoding("ISO-8859-9").GetBytes(HashedPassword);
            byte[] inputbytes = sha.ComputeHash(hashbytes);

            return GetHexaDecimal(inputbytes);
        }

        public static string GetHexaDecimal(byte[] bytes)
        {
            StringBuilder s = new StringBuilder();
            int length = bytes.Length;

            for (int n = 0; n <= length - 1; n++)
            {
                s.Append(String.Format("{0,2:x}", bytes[n]).Replace(" ", "0"));
            }

            return s.ToString();
        }

        public static void RedirectAndPOST(Page page, string destinationUrl, NameValueCollection data)
        {
            string strForm = PreparePOSTForm(destinationUrl, data);
            page.Controls.Add(new LiteralControl(strForm));
        }

        public static string CreateOrderId()
        {
            string OrderCode = Util.CreateRandomValue(10, true, true, true, false);
            string yil = DateTime.Today.Year.ToString();
            yil = yil.Substring(2);

            string ay = DateTime.Today.Month.ToString();
            if (ay.Length == 1)
                ay = "0" + ay;

            string gun = DateTime.Today.Day.ToString();
            if (gun.Length == 1)
                gun = "0" + gun;

            string saat = DateTime.Now.Hour.ToString();
            if (saat.Length == 1)
                saat = "0" + saat;

            string dakika = DateTime.Now.Minute.ToString();
            if (dakika.Length == 1)
                dakika = "0" + dakika;

            string saniye = DateTime.Now.Second.ToString();
            if (saniye.Length == 1)
                saniye = "0" + saniye;

            try
            {
                return "POS_WEB_" + OrderCode.Substring(0, 5) + "" + yil + "" + ay + "" + gun + "" + saat + "" + dakika + "" + saniye;
            }
            catch
            {
                return "POS_WEB_" + OrderCode.Substring(0, 5) + "" + yil + "" + ay + "" + gun + "" + saat + "" + dakika + "" + saniye;
            }
        }
        #endregion

        #region Private Methods
        private static String PreparePOSTForm(string url, NameValueCollection data)
        {
            string formID = "PostForm";
            StringBuilder strForm = new StringBuilder();
            strForm.Append("<form id=\"" + formID + "\" name=\"" + formID + "\" action=\"" + url + "\" method=\"POST\">");

            foreach (string key in data)
            {
                strForm.Append("<input type=\"hidden\" name=\"" + key + "\" value=\"" + data[key] + "\">");
            }

            strForm.Append("</form>");
            StringBuilder strScript = new StringBuilder();
            strScript.Append("<script language=\"javascript\">");
            strScript.Append("var v" + formID + " = document." + formID + ";");
            strScript.Append("v" + formID + ".submit();");
            strScript.Append("</script>");

            return strForm.ToString() + strScript.ToString();
        }
        #endregion

        #region Enums
        public enum Banks
        {
            Garanti = 1,
            Akbank = 2,
            Garanti3D = 3,
            Vakifbank = 4,
            YapiKredi = 5,
            Isbank = 6,
            Finansbank = 7
        }
        #endregion

    }
}