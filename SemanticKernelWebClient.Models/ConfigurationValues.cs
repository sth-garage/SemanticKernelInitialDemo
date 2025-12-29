namespace SemanticKernelWebClient.Models
{
    public class ConfigurationValues
    {
        public OpenAISettings OpenAISettings { get; set; } = new OpenAISettings();
    }

    public class OpenAISettings
    {
        public string OpenAI_ApiKey { get; set; } = "";
        public string OpenAI_Model { get; set; } = "";

        public string OpenAI_ApiUrl { get; set; } = "";

        public string OpenAI_EmbeddingModel { get; set; } = "";
        public string OpenAI_TextToAudioModel { get; set; } = "";
        public string OpenAI_ImageModel { get; set; } = "";
    }
}
