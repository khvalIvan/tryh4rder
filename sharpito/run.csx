using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log) {
    string data = await req.Content.ReadAsStringAsync();
    log.Info("Input: " + data);
    string[] result = await ParseTelegramMessage(data);
    log.Info("Result: " + result[0] + "; " + result[1]);
    string exitstring = await SendTelegramMessage(result);
    log.Info("Final: " + exitstring);

    return req.CreateResponse(HttpStatusCode.OK);
}

public static async Task<string[]> ParseTelegramMessage(string data) {
    // Initialize variables
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    dynamic parsed = JsonConvert.DeserializeObject(data);
    string key = Environment.GetEnvironmentVariable("APIKey");
    string chatId = "";
    string texto = "";
    string cb_ID = "";
    string json = "";
    string url = "";

   if (parsed.message == null) {
        chatId = parsed.callback_query.message.chat.id.ToString();
        texto  = parsed.callback_query.data.ToString();
        cb_ID  = parsed.callback_query.id.ToString();
    }
    else {
        chatId = parsed.message.chat.id.ToString();
        texto  = parsed.message.text.ToString();
    }

    switch(texto) {
        case "MrllamaSC":
            dictionary.Add("type", "tweet");
            dictionary.Add("searchfor", "MrllamaSC");
            dictionary.Add("chatId", chatId);
            dictionary.Add("count", "2");
            json = JsonConvert.SerializeObject(dictionary);
            url = "https://prod-14.northeurope.logic.azure.com/workflows/7d6c2b04c87b4efea8897f9533cab980/triggers/manual/paths/invoke/getTweets?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=CW1k8z_JJP8ML0hYssOsZANd6hCp5goCtmYbVU5fKhw";
            break;
        default:
            dictionary.Add("chat_id", chatId);
            dictionary.Add("text", texto);
            if (parsed.message.type != "tweet") {
                dictionary.Add("reply_markup", @"{""inline_keyboard"":[[{""text"":""MrllamaSC?"",""callback_data"":""MrllamaSC""}],[{""text"":""Placeholder"",""callback_data"":""Placeholder""}]]}");
            }
            if (cb_ID != null) {
                dictionary.Add("callback_query_id", cb_ID);
            }
            url = String.Format("https://api.telegram.org/bot{0}/sendMessage", key);
            json = JsonConvert.SerializeObject(dictionary);
            break;
    }

    string[] result = new string[] {json, url};
    return result;
}

public static async Task<string> SendTelegramMessage(string[] data) {
    using (var client = new HttpClient())
    {
        var requestData = new StringContent(data[0], Encoding.UTF8, "application/json");
        var response = await client.PostAsync(data[1], requestData);
        var result = await response.Content.ReadAsStringAsync();

        return result;
    }
}