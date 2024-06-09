using OpenAI;
using OpenAI.Managers;
using OpenAI.ObjectModels.RequestModels;

namespace AItoSQL.Services
{
    public class AIService
    {
        public static async Task<List<ChatMessage>> SendPrompt(string prompt, string models, string key)
        {
            var openAiService = new OpenAIService(new OpenAiOptions() { ApiKey = key! });

            //// INFO: FromSystem (developer), FromAssistance (AI), FromUser (user)

            List<ChatMessage> messages = new();
            
            var modelPromt = @$"
                You are a SQL machine that writes queries in Transact-SQL for a SQL Server database.
                With a given database structure, your mission is to write SQL queries to answer the questions that will be asked to you. 
                Include ID fields whenever possible.
                IMPORTANT: Yout answer must be in JSON format with 'Sql_Code' field containing only the valid SQL Code to execute in a server, 
                    and another field called 'Notes' to add, if necessary, any other words, comments, notes, etc. 
                    There is an example of a valid response:
                    {{""Sql_Code"": ""SELECT * FROM table WHERE x = z"", ""Notes"": ""Only if needed, any other words in your answer, comments, notes, etc.""}}               
                DO NOT INCLUDE THE LANGUAGE NAME IN YOUR RESPONSE, LIKE ```json
                --------------------------
                The database structure is:
                {models}
                
                --------------------------
                And the question is:
            ";
            messages.Add(ChatMessage.FromSystem(modelPromt));

            var userPrompt = $"{prompt}";
            messages.Add(ChatMessage.FromUser(userPrompt));

            var completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
            {
                Messages = messages,
                Model = OpenAI.ObjectModels.Models.Gpt_4o,
                MaxTokens = 250,
                Temperature = (float?)0.8
            });
            if (completionResult.Successful)
            {
                var response = completionResult.Choices.First().Message.Content;
                messages.Add(ChatMessage.FromAssistant(response ?? ""));
            }
            else
            {
                if (completionResult.Error == null)
                {
                    throw new Exception("Unknown Error");
                }
                throw new Exception($"ERROR! {completionResult.Error.Code}: {completionResult.Error.Message}");
            }

            return messages;
        }
    }
}