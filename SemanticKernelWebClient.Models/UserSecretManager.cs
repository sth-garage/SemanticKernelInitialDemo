using Microsoft.Extensions.Configuration;

namespace SemanticKernelWebClient.Models
{
    public class UserSecretManager
    {

        // dotnet user-secrets set "Movies:ServiceApiKey" "12345"
        public static ConfigurationValues GetSecrets(IConfigurationRoot? configurationRoot)
        {
            var result = new ConfigurationValues();

            if (configurationRoot != null)
            {

                result = new ConfigurationValues
                {

                    OpenAISettings = new OpenAISettings
                    {
                        OpenAI_ApiKey = configurationRoot["OpenAI_ApiKey"],
                        OpenAI_ApiUrl = configurationRoot["OpenAI_ApiUrl"],
                        OpenAI_EmbeddingModel = configurationRoot["OpenAI_EmbeddingModel"],
                        OpenAI_ImageModel = configurationRoot["OpenAI_ImageModel"],
                        OpenAI_Model = configurationRoot["OpenAI_Model"],
                        OpenAI_TextToAudioModel = configurationRoot["OpenAI_TextToAudioModel"],
                    },
                };
            }

            return result;
        }
    }
}
