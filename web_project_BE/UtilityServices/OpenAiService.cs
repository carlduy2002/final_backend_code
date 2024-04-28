using Microsoft.Extensions.Options;
using OpenAI_API;
using OpenAI_API.Completions;
using web_project_BE.Configurations;

namespace web_project_BE.UtilityServices
{
    public class OpenAiService : IOpenAiService
    {
        private readonly OpenAiConfig _openAiConfig;


        public OpenAiService
        (
            IOptions<OpenAiConfig> optionsMonitor

        ) 
        {
            _openAiConfig = optionsMonitor.Value;
        }

        public async Task<string> CompleteSentence(string text)
        {
            var api = new OpenAI_API.OpenAIAPI(_openAiConfig.Key);

            var result = await api.Completions.GetCompletion(text);

            return result;
        }
    }
}
