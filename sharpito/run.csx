using System.Net;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log) {
    // Main routine
    string data = await req.Content.ReadAsStringAsync();
    log.Info("Input:   " + data);
    string[] result = await ParseTelegramMessage(data);
    log.Info("Result:  " + result[0] + "; " + result[1]);
    string exitstring = await SendTelegramMessage(result);
    log.Info("Final:   " + exitstring);

    return exitstring == null
        ? req.CreateResponse(HttpStatusCode.BadRequest, "Something went wrong, sorry")
        : req.CreateResponse(HttpStatusCode.OK);
}

public static async Task<string[]> ParseTelegramMessage(string data) {
    // Initialize variables
    Dictionary<string, string> dictionary = new Dictionary<string, string>();
    dynamic parsed = JsonConvert.DeserializeObject(data);
    string key_tg = Environment.GetEnvironmentVariable("tgKey");
    string key_la = Environment.GetEnvironmentVariable("laKey");
    string callback_ID, chatId, texto, json, url;
    callback_ID = chatId = texto = json = url = string.Empty;

    // Retarded parse
    if (parsed.message == null) {
        callback_ID = parsed.callback_query.id.ToString();
        chatId = parsed.callback_query.message.chat.id.ToString();
        texto = parsed.callback_query.data.ToString(); 
    }
    else {
        chatId = parsed.message.chat.id.ToString();
        texto = parsed.message.text.ToString();
        if (parsed.callback_query != null) {
            callback_ID = parsed.callback_query.id.ToString();
        }
    }

    // Switcherino
    switch(texto) {
        case "MrllamaSC":
            dictionary.Add("type", "tweet");
            dictionary.Add("searchfor", "MrllamaSC");
            dictionary.Add("chatId", chatId);
            dictionary.Add("count", "1");
            dictionary.Add("callback_query_id", callback_ID);
            json = JsonConvert.SerializeObject(dictionary);
            url = String.Format("https://prod-14.northeurope.logic.azure.com/workflows/7d6c2b04c87b4efea8897f9533cab980/triggers/manual/paths/invoke/getTweets?api-version=2016-06-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig={0}", key_la);
            break;
        default:
            dictionary.Add("chat_id", chatId);
            dictionary.Add("text", texto);
            if (parsed.message.type != "tweet") {
                dictionary.Add("reply_markup", @"{""inline_keyboard"":[[{""text"":""MrllamaSC?"",""callback_data"":""MrllamaSC""}],[{""text"":""Placeholder"",""callback_data"":""Placeholder""}]]}");
            }
            if (callback_ID != "") {
                dictionary.Add("callback_query_id", callback_ID);
            }
            url = String.Format("https://api.telegram.org/bot{0}/sendMessage", key_tg);
            json = JsonConvert.SerializeObject(dictionary);
            break;
    }
    string[] result = new string[] {json, url};
    return result;
}

public static async Task<string> SendTelegramMessage(string[] data) {
    using (var client = new HttpClient())
    {
        // Send the request
        var requestData = new StringContent(data[0], Encoding.UTF8, "application/json");
        var response = await client.PostAsync(data[1], requestData);
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
}