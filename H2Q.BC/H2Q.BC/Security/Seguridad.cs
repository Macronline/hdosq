using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using H2Q.BC.Base;
using H2Q.BC.DataAccess;

namespace H2Q.BC.Security
{
    [Serializable]
    public class Seguridad : Singular
    {
        public Seguridad()
        {
        }
        
        
        internal static string EncriptarPass(string PassUser)
        {
            string res = "";
            res = GetMD5(PassUser, "tokSCI==#2016", "MFPSci2015gdMCM");
            return res;
        }

        /// <summary>
        /// Appends a parameter to the QueryString
        /// </summary>
        /// <param name="Url"></param>
        /// <param name="Key"></param>
        /// <param name="Value"></param>
        /// <returns></returns>
        public static string GetAppendedQueryString(string Url, string Key, string Value)
        {
            if (Url.Contains("?"))
            {
                Url = string.Format("{0}&{1}={2}", Url, Key, Value);
            }
            else
            {
                Url = string.Format("{0}?{1}={2}", Url, Key, Value);
            }

            return Url;
        }

        ///// <summary>
        ///// Reads cookie value from the cookie
        ///// </summary>
        ///// <param name="cookie"></param>
        ///// <returns></returns>
        //public static string GetCookieValue(HttpCookie Cookie)
        //{
        //    if (string.IsNullOrEmpty(Cookie.Value))
        //    {
        //        return Cookie.Value;
        //    }
        //    return Cookie.Value.Substring(0, Cookie.Value.IndexOf("|"));
        //}

        ///// <summary>
        ///// Get cookie expiry date that was set in the cookie value 
        ///// </summary>
        ///// <param name="cookie"></param>
        ///// <returns></returns>
        //public static DateTime GetExpirationDate(HttpCookie Cookie)
        //{
        //    if (string.IsNullOrEmpty(Cookie.Value))
        //    {
        //        return DateTime.MinValue;
        //    }
        //    string strDateTime = Cookie.Value.Substring(Cookie.Value.IndexOf("|") + 1);
        //    return Convert.ToDateTime(strDateTime);
        //}

        /// <summary>
        /// Determines whether two string values are equals or not
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static bool StringEquals(string A, string B)
        {
            return string.Compare(A, B, true) == 0;
        }

        /// <summary>
        /// Set cookie value using the token and the expiry date
        /// </summary>
        /// <param name="Value"></param>
        /// <param name="Minutes"></param>
        /// <returns></returns>
        public static string BuildCookueValue(string Value, int Minutes)
        {
            return string.Format("{0}|{1}", Value, DateTime.Now.AddMinutes(Minutes).ToString());
        }

        public static string GetGuidHash()
        {
            return Guid.NewGuid().ToString().GetHashCode().ToString("x");
        }

        public static string GetMD5(string user, string token, string key_set)
        {
            //Declaraciones
            System.Security.Cryptography.MD5 md5;
            md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            string v_str = user + token;
            //Conversion
            Byte[] encodedBytes = md5.ComputeHash(ASCIIEncoding.Default.GetBytes(key_set + v_str));  //genero el hash a partir de la password original
            String v_str_2 = System.Text.RegularExpressions.Regex.Replace(BitConverter.ToString(encodedBytes).ToLower(), @"-", "");     //devuelve el hash continuo y en minuscula. (igual que en php)
            return v_str_2;
        }
        
    }

}
