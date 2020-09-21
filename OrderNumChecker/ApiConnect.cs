using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;


namespace OrderNumChecker
{
     
    

    public class ApiConnect
    {
        public static HttpResponseMessage jRes = new HttpResponseMessage();
        public static string ReadS(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                string result = appSettings[key] ?? "Not Found";
                //Console.WriteLine(result);
                return result;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings");
                return "";
            }

        }

        public static JToken GetRestTimes(string restname)
        {
            try
            {
                WebClient wcl = new WebClient();
                string[] getstring = { ApiConnect.ReadS("apiaddrtime"), "/Restaurant/", restname };
                String jsonResult = wcl.DownloadString(String.Concat(getstring));
                JArray jRes = JArray.Parse(jsonResult);
                return jRes;
            }
            catch(Exception ex)
            {
                Logger.Log.Info(ex.ToString());
                JToken error = JArray.Parse("[{error:1}]");
                return error;
            }
        }

        public static async Task<HttpResponseMessage> SetTime(string zone, string time, string date, long restid)
        {
            
            HttpClient hClient = new HttpClient();
            hClient.DefaultRequestHeaders.Add("Authorization", "Basic YXBpdXNlcjpjZGpoYmQ4SGdldEdHNjc0a0JYdg==");
            //string[] paramstring = { ApiConnect.ReadS("apiaddr"), "?Zone=", zone, "&Time=", time, "&DateOfStop=", data };
            //Logger.Log.Info(String.Concat(paramstring));
            
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Zone",zone),
                new KeyValuePair<string, string>("Time", time),
                new KeyValuePair<string, string>("DateOfStop",date),
                new KeyValuePair<string, string>("RestExtId",restid.ToString()),
            });
            Logger.Log.Info(ReadS("apiaddrtime"));
            
            try
            {
                jRes = await hClient.PostAsync(ReadS("apiaddrtime"), content);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            Logger.Log.Info(jRes.StatusCode + " " +jRes.RequestMessage.Method + " " + time + " выставлено на стоп");
            return jRes;
            
        }
    }
}
