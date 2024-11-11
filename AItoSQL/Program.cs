using AItoSQL.Models.DTOs;
using AItoSQL.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAnyOrigin", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});
var openAIApiKey = builder.Configuration.GetSection("OpenAIServiceOptions")["ApiKey"]!;
var dbConnectionString = builder.Configuration.GetSection("DbServiceOptions")["ConnectionString"]!;
var mySqlConnectionString = builder.Configuration.GetSection("DbServiceOptions")["MySqlConnectionString"]!;
var mariaDbConnectionString = builder.Configuration.GetSection("DbServiceOptions")["MariaDbConnectionString"]!;
var postgreSqlConnectionString = builder.Configuration.GetSection("DbServiceOptions")["PostgreSqlConnectionString"]!;
var sqliteConnectionString = builder.Configuration.GetSection("DbServiceOptions")["SqliteConnectionString"]!;

var app = builder.Build();

// Configure the HTTP request pipeline.
#if DEBUG
    app.UseSwagger();
    app.UseSwaggerUI();
#endif

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapPost("/AskTheDatabase", async (PromptDTO prompt) =>
{
    var models = DbService.ModelsToText(dbConnectionString);
    var messages = await AIService.SendPrompt(prompt.Prompt, models, openAIApiKey);
    var responseMessage = messages.Last().Content;
    ResponseDTO response = new();

    if (responseMessage != null && responseMessage.Contains("Sql_Code"))
        response = JsonSerializer.Deserialize<ResponseDTO>(responseMessage);

    if (response.Sql_Code != "")
    {
        string code = response.Sql_Code.Replace("\n", " ").Trim();
        var result = DbService.QueryDatabase(code, dbConnectionString);
        if(result == "[]")
        {
            var noResults = new { error = $"No results found." };
            return JsonSerializer.Serialize(noResults);
        }
        return result;
    }

    var clarification = new { error = $"{responseMessage}" };
    return JsonSerializer.Serialize(clarification);
})
.WithName("AskTheDatabase")
.WithOpenApi();

app.MapPost("/AskTheDatabaseMySql", async (PromptDTO prompt) =>
{
    var models = DbService.ModelsToText(mySqlConnectionString);
    var messages = await AIService.SendPrompt(prompt.Prompt, models, openAIApiKey);
    var responseMessage = messages.Last().Content;
    ResponseDTO response = new();

    if (responseMessage != null && responseMessage.Contains("Sql_Code"))
        response = JsonSerializer.Deserialize<ResponseDTO>(responseMessage);

    if (response.Sql_Code != "")
    {
        string code = response.Sql_Code.Replace("\n", " ").Trim();
        var result = DbService.QueryDatabase(code, mySqlConnectionString);
        if(result == "[]")
        {
            var noResults = new { error = $"No results found." };
            return JsonSerializer.Serialize(noResults);
        }
        return result;
    }

    var clarification = new { error = $"{responseMessage}" };
    return JsonSerializer.Serialize(clarification);
})
.WithName("AskTheDatabaseMySql")
.WithOpenApi();

app.MapPost("/AskTheDatabaseMariaDb", async (PromptDTO prompt) =>
{
    var models = DbService.ModelsToText(mariaDbConnectionString);
    var messages = await AIService.SendPrompt(prompt.Prompt, models, openAIApiKey);
    var responseMessage = messages.Last().Content;
    ResponseDTO response = new();

    if (responseMessage != null && responseMessage.Contains("Sql_Code"))
        response = JsonSerializer.Deserialize<ResponseDTO>(responseMessage);

    if (response.Sql_Code != "")
    {
        string code = response.Sql_Code.Replace("\n", " ").Trim();
        var result = DbService.QueryDatabase(code, mariaDbConnectionString);
        if(result == "[]")
        {
            var noResults = new { error = $"No results found." };
            return JsonSerializer.Serialize(noResults);
        }
        return result;
    }

    var clarification = new { error = $"{responseMessage}" };
    return JsonSerializer.Serialize(clarification);
})
.WithName("AskTheDatabaseMariaDb")
.WithOpenApi();

app.MapPost("/AskTheDatabasePostgreSql", async (PromptDTO prompt) =>
{
    var models = DbService.ModelsToText(postgreSqlConnectionString);
    var messages = await AIService.SendPrompt(prompt.Prompt, models, openAIApiKey);
    var responseMessage = messages.Last().Content;
    ResponseDTO response = new();

    if (responseMessage != null && responseMessage.Contains("Sql_Code"))
        response = JsonSerializer.Deserialize<ResponseDTO>(responseMessage);

    if (response.Sql_Code != "")
    {
        string code = response.Sql_Code.Replace("\n", " ").Trim();
        var result = DbService.QueryDatabase(code, postgreSqlConnectionString);
        if(result == "[]")
        {
            var noResults = new { error = $"No results found." };
            return JsonSerializer.Serialize(noResults);
        }
        return result;
    }

    var clarification = new { error = $"{responseMessage}" };
    return JsonSerializer.Serialize(clarification);
})
.WithName("AskTheDatabasePostgreSql")
.WithOpenApi();

app.MapPost("/AskTheDatabaseSqlite", async (PromptDTO prompt) =>
{
    var models = DbService.ModelsToText(sqliteConnectionString);
    var messages = await AIService.SendPrompt(prompt.Prompt, models, openAIApiKey);
    var responseMessage = messages.Last().Content;
    ResponseDTO response = new();

    if (responseMessage != null && responseMessage.Contains("Sql_Code"))
        response = JsonSerializer.Deserialize<ResponseDTO>(responseMessage);

    if (response.Sql_Code != "")
    {
        string code = response.Sql_Code.Replace("\n", " ").Trim();
        var result = DbService.QueryDatabase(code, sqliteConnectionString);
        if(result == "[]")
        {
            var noResults = new { error = $"No results found." };
            return JsonSerializer.Serialize(noResults);
        }
        return result;
    }

    var clarification = new { error = $"{responseMessage}" };
    return JsonSerializer.Serialize(clarification);
})
.WithName("AskTheDatabaseSqlite")
.WithOpenApi();

app.Run();
