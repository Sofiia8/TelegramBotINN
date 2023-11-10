using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Text;

namespace TelegramBotINNConsole
{
    internal class RequestTaxService
    {
        static public HttpClient HClient { get; private set; }
        private IConfiguration _configuration;
        internal RequestTaxService(IConfiguration config)
        {
            HClient = new HttpClient();
            _configuration = config;
        }
        internal async Task<string> GetInfoByINNAsync(string request)
        {
            var key = _configuration["TaxServiceTokenKey"];
            StringBuilder stringBuilder = new StringBuilder(GlobalConstants.ApiFNSUrl);
            stringBuilder.AppendLine($"?req={request}&key={key}");

            try
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, stringBuilder.ToString());
                HttpResponseMessage httpResponse = await HClient.SendAsync(httpRequest);
                string json = await httpResponse.Content.ReadAsStringAsync();

                JObject jo = JObject.Parse(json);
                JArray? listEntities = jo["items"] as JArray;

                if (listEntities == null || listEntities.Count() == 0)
                {
                    return "Legal Entities are not found";
                }

                var listResult = listEntities?.Where(entity => entity["ЮЛ"] != null)
                                .Select(entityUL =>
                                {
                                    string inn = entityUL["ЮЛ"]["ИНН"]?.Value<string>() ?? "";
                                    string fullName = entityUL["ЮЛ"]["НаимПолнЮЛ"]?.Value<string>() ?? "";
                                    string index = entityUL["ЮЛ"]["Адрес"]["Индекс"]?.Value<string>() ?? "";
                                    string address = entityUL["ЮЛ"]["Адрес"]["АдресПолн"]?.Value<string>() ?? "";

                                    return $"{inn}: {fullName}, {index}, {address};\n";
                                });
                if (listResult == null || listResult.Count() == 0)
                {
                    return "Legal Entities are not found";
                }

                return string.Concat(listResult);
            }
            catch (Exception ex)
            {
                return "Unsuccessful connection with Tax Service";
            }
        }

        internal async Task<string> GetFullInfoByINNAsync(string request)
        {
            var key = _configuration["TaxServiceTokenKey"];
            StringBuilder stringBuilder = new StringBuilder(GlobalConstants.ApiFNSUrlFull);
            stringBuilder.AppendLine($"?req={request}&key={key}");

            try
            {
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Get, stringBuilder.ToString());
                HttpResponseMessage httpResponse = await HClient.SendAsync(httpRequest);
                string json = await httpResponse.Content.ReadAsStringAsync();

                JObject jo = JObject.Parse(json);
                JArray? listEntities = jo["items"] as JArray;

                if (listEntities == null || listEntities.Count() == 0)
                {
                    return "Legal Entities are not found";
                }

                var listResult = listEntities?.Where(entity => entity["ЮЛ"] != null)
                                .Select(entityUL => entityUL["ЮЛ"]).First();

                if (listResult == null || listResult.Count() == 0)
                {
                    return "Legal Entities are not found";
                }
                return listResult.ToString();
            }
            catch (Exception ex)
            {
                return "Unsuccessful connection with Tax Service";
            }
        }
        
    }
}