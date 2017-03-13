using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log) {
    var data = await req.Content.ReadAsStringAsync();
    dynamic parsed = JsonConvert.DeserializeObject(data);
    log.Info(data);

    string chatId = "";
    string texto = "";
    string cb_ID = "";

    if (parsed.message == null) {
        chatId = parsed.callback_query.message.chat.id.ToString();
        texto  = parsed.callback_query.data.ToString();
        cb_ID  = parsed.callback_query.id.ToString();
    }
    else {
        chatId = parsed.message.chat.id.ToString();
        texto  = parsed.message.text.ToString();      
    }

    string[] parse = await ParseTelegramMessage(texto, chatId);
    var results = await SendTelegramMessage(parse);

    log.Info(results);
    return req.CreateResponse(HttpStatusCode.OK);

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

public static async Task<string> ParseTelegramMessage(string text, string chat) {
    string key = Environment.GetEnvironmentVariable("APIKey");
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    string json = "";
    string url = "";

    switch(text) {
        case "MrllamaSC":
            dictionary.Add("type", "tweet");
            dictionary.Add("searchfor", "MrllamaSC");
            json = JsonConvert.SerializeObject(dictionary);
            url = "https://prod-14.northeurope.logic.azure.com/workflows/7d6c2b04c87b4efea8897f9533cab980/triggers/manual/paths/invoke/getTweets?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=CW1k8z_JJP8ML0hYssOsZANd6hCp5goCtmYbVU5fKhw";
            return new string[] {json, url};
        default:
            dictionary.Add("chat_id", chat);
            dictionary.Add("text", text);
            dictionary.Add("reply_markup", @"{""inline_keyboard"":[[{""text"":""MrllamaSC?"",""callback_data"":""MrllamaSC""}],[{""text"":""Placeholder"",""callback_data"":""Placeholder""}]]}");
            json = JsonConvert.SerializeObject(dictionary);
            url = String.Format("https://api.telegram.org/bot{0}/sendMessage", key);
            return new string[] {json, url};
    }
}