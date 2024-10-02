using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

class Program
{
    static HttpClient? client;
    internal static readonly int[] sourceArray = [2, 12, 13, 15, 16, 19];

    static void ConsoleWithTime(string word, ConsoleColor color)
    {
        string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        Console.ResetColor();
        Console.Write($"[{now}]");
        Console.ForegroundColor = color;
        Console.WriteLine($" {word}");
        Console.ResetColor();
    }

    static List<string> LoadQuery()
    {
        try
        {
            return File.ReadAllLines("data.txt").Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line)).ToList();
        }
        catch (Exception)
        {
            Console.WriteLine("File data.txt not found.");
            return new List<string>();
        }
    }

    static async Task<string?> GetToken(int index, string apiToken)
    {
        try
        {
            var url = "https://coub.com/api/v2/torus/token";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("Accept", "application/json, text/plain, */*");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9,fa;q=0.8");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Origin", "https://coub.com");
            request.Headers.Add("Referer", "https://coub.com/");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Site", "same-site");
            request.Headers.Add("User-Agent", GetUserAgents(index));
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", $"\"{GetPlatforms(index)}\"");
            request.Headers.Add("x-auth-token", apiToken);

            client = new();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            if (jsonResponse.TryGetProperty("access_token", out var accessTokenElement) &&
                jsonResponse.TryGetProperty("expires_in", out var expiresInElement))
            {
                string? accessToken = accessTokenElement.GetString();
                int expiresIn = expiresInElement.GetInt32();
                int expirationHours = (int)Math.Round(expiresIn / 3600.0);

                if (expirationHours > 0)
                {
                    ConsoleWithTime($"Success get token, Expired in {expirationHours} Hour", ConsoleColor.Green);
                }
                else
                {
                    ConsoleWithTime("Success get token, Expires in less than 1 hour", ConsoleColor.Green);
                }

                return accessToken;
            }
        }
        catch (Exception ex)
        {
            ConsoleWithTime($"Error getting token: {ex.Message}", ConsoleColor.Red);
        }
        return null;
    }

    static async Task<string?> Login(int index, string query)
    {
        try
        {
            var url = "https://coub.com/api/v2/sessions/login_mini_app?" + query;
            client = new();
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9,fa;q=0.8");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Origin", "https://coub.com");
            client.DefaultRequestHeaders.Add("Referer", "https://coub.com/");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
            client.DefaultRequestHeaders.Add("User-Agent", GetUserAgents(index));
            client.DefaultRequestHeaders.Add("sec-ch-ua-mobile", "?0");
            client.DefaultRequestHeaders.Add("sec-ch-ua-platform", $"\"{GetPlatforms(index)}\"");
            var response = await client.PostAsync(url, null);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonSerializer.Deserialize<JsonElement>(responseBody);

            if (jsonResponse.TryGetProperty("api_token", out var apiTokenElement))
            {
                string? apiToken = apiTokenElement.GetString();
                return await GetToken(index, apiToken ?? "") ?? "";
            }
        }
        catch (Exception ex)
        {
            ConsoleWithTime($"Error during login: {ex.Message}", ConsoleColor.Red);
        }
        return null;
    }

    static async Task<JsonElement?> GetRewards(int index, string token)
    {
        try
        {
            var url = "https://rewards.coub.com/api/v2/get_user_rewards";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9,fa;q=0.8");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Origin", "https://coub.com");
            request.Headers.Add("Referer", "https://coub.com/");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Site", "same-site");
            request.Headers.Add("User-Agent", GetUserAgents(index));
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", $"\"{GetPlatforms(index)}\"");

            client = new();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(responseBody);
        }
        catch (Exception)
        {
            ConsoleWithTime($"Failed to get reward.", ConsoleColor.Red);
            return null;
        }
    }

    static async Task<JsonElement?> ClaimTask(int index, string token, int taskId, string taskTitle)
    {
        try
        {
            var url = $"https://rewards.coub.com/api/v2/complete_task?task_reward_id={taskId}";
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("Authorization", $"Bearer {token}");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Accept-Language", "en-US,en;q=0.9,fa;q=0.8");
            request.Headers.Add("Connection", "keep-alive");
            request.Headers.Add("Origin", "https://coub.com");
            request.Headers.Add("Referer", "https://coub.com/");
            request.Headers.Add("Sec-Fetch-Dest", "empty");
            request.Headers.Add("Sec-Fetch-Mode", "cors");
            request.Headers.Add("Sec-Fetch-Site", "same-site");
            request.Headers.Add("User-Agent", GetUserAgents(index));
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", $"\"{GetPlatforms(index)}\"");

            client = new();
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(responseBody);
        }
        catch (Exception)
        {
            ConsoleWithTime($"ID {taskId} | Task '{taskTitle}' failed to claim", ConsoleColor.Red);
            return null;
        }
    }

    static async Task<int> Main()
    {
        Random rnd = new();
        int seconds = 5;
        int cnt = 0;

        while (true)
        {
            cnt++;
            var queries = LoadQuery();
            var tasks = JsonSerializer.Deserialize<List<Tasks>>(File.ReadAllText("coub_task.json"));
            int totalQueries = queries.Count;

            for (int index = 0; index < totalQueries; index++)
            {
                var query = queries[index];
                var parsed = HttpUtility.ParseQueryString(query);
                var user = JsonSerializer.Deserialize<User>(HttpUtility.UrlDecode(parsed["user"]) ?? "");

                string username = (user ?? new()).Username ?? "";
                Console.WriteLine();
                ConsoleWithTime($"====== Account {index + 1}/{totalQueries} | {username} ======", ConsoleColor.White);

                var token = await Login(index, query);
                var dataReward = await GetRewards(index, token ?? "");

                if (dataReward.HasValue)
                {
                    ConsoleWithTime("Balance: " + dataReward.Value.EnumerateArray().Sum(x => x.GetProperty("points").GetDouble()), ConsoleColor.Yellow);
                    Console.WriteLine();

                    var validTaskIds = dataReward.Value.EnumerateArray()
                        .Select(data => data.GetProperty("id").GetInt32())
                        .Where(id => !sourceArray.Contains(id))
                        .ToList();

                    foreach (var task in tasks ?? [])
                    {
                        if (validTaskIds.Contains(task.Id))
                        {
                            ConsoleWithTime($"{task.Title} Done...", ConsoleColor.Yellow);
                        }
                        else
                        {
                            ConsoleWithTime($"{task.Title} Starting task...", ConsoleColor.Cyan);
                            await ClaimTask(index, token ?? "", task.Id, task.Title);
                        }
                    }
                }

                if (index < totalQueries - 1)
                {
                    seconds = rnd.Next(60, 180);
                    Console.WriteLine();
                    ConsoleWithTime($"Waiting {seconds} seconds to continue...", ConsoleColor.DarkYellow);
                    Thread.Sleep(seconds * 1000);
                }
            }

            if (cnt % 2 == 1)
                seconds = rnd.Next(60, 180);
            else
                seconds = rnd.Next(7200, 14400);

            Console.WriteLine();
            ConsoleWithTime($"Waiting {seconds} seconds to continue...", ConsoleColor.DarkYellow);
            Thread.Sleep(seconds * 1000);
        }
    }

    static string GetUserAgents(int index)
    {
        string[] agents = {"Mozilla/5.0 (Linux; Android 7.0; SM-G925F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.45 Mobile Safari/537.36",
            "Mozilla/5.0 (iPhone; CPU iPhone OS 17_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148",
            "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/60.0.3096.19 Safari/537.36",
            "Mozilla/5.0 (Linux; Android 11; SAMSUNG SM-A202F) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/14.2 Chrome/87.0.4280.141 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 9) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/87.0.4280.86 Mobile DuckDuckGo/5 Safari/537.36",
            "Mozilla/5.0 (Linux; Android 11; GM1901) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/92.0.4515.159 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 10; SOV40) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.127 Mobile Safari/537.36",
            "Mozilla/5.0 (Linux; Android 8.0.0; SM-A520F) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/96.0.4664.46 Mobile Safari/537.36",
            "Dalvik/2.1.0 (Linux; U; Android 10; Redmi 7 Build/QQ3A.200605.002.A1)",
            "Mozilla/5.0 (Linux; Android 10; SM-J610G) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/94.0.4606.85 Mobile Safari/537.36",
            "MMozilla/5.0 (Linux; Android 8.1.0; B450) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/79.0.3945.136 Mobile Safari/537.36",
            "Opera/9.80 (Android; Opera Mini/10.0.1884/191.227; U; en) Presto/2.12.423 Version/12.16",
            "Mozilla/5.0 (Linux; arm_64; Android 8.1.0; DUA-L22) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 YaBrowser/20.4.4.76.00 SA/1 Mobile Safari/537.36",
            "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/93.0.4577.82 Safari/537.36 OPR/79.0.4143.66",
            "Mozilla/5.0 (Linux; Android 7.1.2; GT-P7300 Build/N2G48C; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/61.0.3163.81 Safari/537.36",
            "Mozilla/5.0 (iPad; CPU OS 14_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 EdgiOS/45.11.1 Mobile/15E148 Safari/605.1.15",
            "Mozilla/5.0 (iPhone; CPU OS 14_4_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) FxiOS/32.0 Mobile/15E148 Safari/605.1.15"};
        return agents[index];
    }

    static string GetPlatforms(int index)
    {
        string[] platforms = {
            "Android",
            "iPhone",
            "Windows",
            "Android",
            "Android",
            "Android",
            "Android",
            "Android",
            "Android",
            "Android",
            "Android",
            "Android",
            "Android",
            "Windows",
            "Android",
            "iPad",
            "iPhone"};
        return platforms[index];
    }
}

class User
{
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
}

class Tasks
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;
}

