using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    string key = Environment.GetEnvironmentVariable("APIKey");
    log.Info(String.Format("Bot key {0}", key));

    var data = await req.Content.ReadAsStringAsync();
    dynamic parsed = JsonConvert.DeserializeObject(data);
    string json = JsonConvert.SerializeObject(parsed);
    log.Info(String.Format("{0}", json));

    var existanceCheck = parsed.message;

    if (existanceCheck == null) {
        string chatId = parsed.callback_query.message.chat.id.ToString();
        string texto  = parsed.callback_query.data.ToString();
        var results = await SendTelegramMessage(texto, chatId, key);
        log.Info(String.Format("{0}", results));
    }
    else {
        string chatId = parsed.message.chat.id.ToString();
        string texto  = parsed.message.text.ToString();
        var results = await SendTelegramMessage(texto, chatId, key);
        log.Info(String.Format("{0}", results));
    }

    return req.CreateResponse(HttpStatusCode.OK);

}

public static async Task<string> SendTelegramMessage(string text, string chat, string key)
{
    using (var client = new HttpClient())
    {

        Dictionary<string, string> dictionary = new Dictionary<string, string>();
        dictionary.Add("chat_id", chat);
        dictionary.Add("text", text);
        dictionary.Add("reply_markup", @"{""inline_keyboard"":[[{""text"":""Well, does this thing even work?"",""callback_data"":""zxc""}],[{""text"":""I sure hope it does!"",""callback_data"":""vbn""}]]}");

        string json = JsonConvert.SerializeObject(dictionary);
        var requestData = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(String.Format("https://api.telegram.org/bot{0}/sendMessage", key), requestData);
        var result = await response.Content.ReadAsStringAsync();

        return result;
    }
}
