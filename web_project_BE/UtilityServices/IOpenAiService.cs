namespace web_project_BE.UtilityServices
{
    public interface IOpenAiService
    {
        Task<string> CompleteSentence(string text);
    }
}
