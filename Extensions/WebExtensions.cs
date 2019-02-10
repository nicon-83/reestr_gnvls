using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReestrGNVLS.Extensions
{
    public class WebExtensions
    {
        public static async void  RedirectWithPost(HttpContext Context, string url, NameValueCollection data = null)
        {
            HttpResponse response = Context.Response;
            response.Clear();

            StringBuilder s = new StringBuilder();
            s.Append("<html>");
            s.AppendFormat("<body onload='document.forms[\"form\"].submit()'>");
            s.AppendFormat("<form name='form' action='{0}' method='post'>", url);
            if (data != null)
            {
                foreach (string key in data)
                {
                    s.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", key, data[key]);
                }
            }
            s.Append("</form></body></html>");
            await response.WriteAsync(s.ToString());
        }

        //запись в лог файл
        public static void WriteToLog(string path, string message)
        {
            if (!File.Exists(path))
            {
                File.Create(path).Dispose();
            }
            using (StreamWriter file = new StreamWriter(path, true, Encoding.UTF8))
            {
                file.WriteLine(message);
            }
        }
    }
}
