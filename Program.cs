using ChatMemoryBot;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

class Program
{
    static async Task Main()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("OPENAI_API_KEY not set. Please set it and run again.");
            return;
        }

        var http = new HttpClient
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };
        http.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

        //var store = new MemoryStore("memory.json");
        //var userId = "rounak";

        var store = new MemoryStore("memory.json");
        Console.WriteLine("Enter your user name:");
        var userId = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(userId))
        {
            Console.WriteLine("User ID cannot be empty. Exiting.");
            return;
        }

        Console.WriteLine("Ask a question (type 'q' to quit):");
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine();
            if (input == null) continue;
            if (input.Trim().ToLower() == "q")
            {
                Console.WriteLine("Exiting the loop. Goodbye!");
                break;
            }

            // Build memory string 
            var past = store.GetAll(userId);
            var memoryString = string.Join("\n", past);

            // Build OpenAI Chat Completions payload
            var req = new ChatRequest(
                "gpt-4o-mini",
                new List<ChatMessage>
                {
                    new("system", $"You are a helpful assistant. Here is the user information:\n{memoryString}"),
                    new("user", input)
                },
                0.7
            );


            try
            {
                var res = await http.PostAsJsonAsync("chat/completions", req);
                if (!res.IsSuccessStatusCode)
                {
                    var errBody = await res.Content.ReadAsStringAsync();
                    Console.WriteLine($"API error: {(int)res.StatusCode} {res.ReasonPhrase}");
                    Console.WriteLine(errBody);
                }
                else
                {
                    var body = await res.Content.ReadFromJsonAsync<ChatResponse>();
                    var reply = body?.Choices?.FirstOrDefault()?.Message?.Content ?? "(no reply)";
                    Console.WriteLine($"AI response: {reply}");
                    Console.WriteLine("--------------------------------------------------------");

                    // Save this input as a “memory”
                    store.Add(userId, input);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error calling the API:");
                Console.WriteLine(ex.Message);
            }
        }
    }
}

// --- DTOs for request/response ---

public record ChatRequest(
    [property: JsonPropertyName("model")] string Model,
    [property: JsonPropertyName("messages")] List<ChatMessage> Messages,
    [property: JsonPropertyName("temperature")] double Temperature = 0.7
);

public record ChatMessage(
    [property: JsonPropertyName("role")] string Role,
    [property: JsonPropertyName("content")] string Content
);

public class ChatResponse
{
    [JsonPropertyName("choices")]
    public List<Choice>? Choices { get; set; }
}

public class Choice
{
    [JsonPropertyName("message")]
    public ChatMessage? Message { get; set; }
}
