using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string logDir = Path.Combine(app.Environment.ContentRootPath, "logs");
Directory.CreateDirectory(logDir);

var fileLock = new object();

app.MapPost("/api/logs", async (HttpContext context) =>
{
    try
    {
        using var reader = new StreamReader(context.Request.Body);
        string body = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(body))
            return Results.BadRequest("Empty body");

        var logEntry = JsonSerializer.Deserialize<JsonElement>(body);

        // Unique log file
        string fileName = $"{DateTime.Now:yyyy-MM-dd}.json";
        string filePath = Path.Combine(logDir, fileName);

        lock (fileLock)
        {
            List<JsonElement> entries = new List<JsonElement>();
            if (File.Exists(filePath))
            {
                string existingJson = File.ReadAllText(filePath);
                if (!string.IsNullOrWhiteSpace(existingJson))
                {
                    var existingArray = JsonSerializer.Deserialize<List<JsonElement>>(existingJson);
                    if (existingArray != null)
                        entries = existingArray;
                }
            }

            entries.Add(logEntry);

            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(entries, options));
        }

        return Results.Ok(new { status = "logged" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error: {ex.Message}");
    }
});

app.MapGet("/api/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
