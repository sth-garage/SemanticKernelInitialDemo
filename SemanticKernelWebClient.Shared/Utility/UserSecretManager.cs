using Microsoft.Extensions.Configuration;
using SemanticKernelWebClient.Shared.Models;

namespace SemanticKernelWebClient.Shared.Utility
{
    public class UserSecretManager
    {

        // dotnet user-secrets set "LMStudio_Model" "12345"
        // dotnet user-secrets set "ConnectionString_SemanticKernelWebClient" "Data Source=127.0.0.1;Initial Catalog=SemanticKernelWebClient;User Id=semanticKernelWebClientServiceLogin;Password=Testing777!!;TrustServerCertificate=True"
        public static ConfigurationValues GetSecrets(IConfigurationRoot? configurationRoot)
        {
            var result = new ConfigurationValues();

            if (configurationRoot != null)
            {

                result = new ConfigurationValues
                {

                    LMStudioSettings = new LMStudioSettings
                    {
                        // openai/gpt-oss-20b
                        LMStudio_ApiKey = configurationRoot["LMStudio_ApiKey"] ?? "",

                        // http://127.0.0.1:1234/v1
                        LMStudio_ApiUrl = configurationRoot["LMStudio_ApiUrl"] ?? "",

                        // openai/gpt-oss-20b
                        LMStudio_Model = configurationRoot["LMStudio_Model"] ?? "",
                    },
                    ConnectionStrings = new ConnectionStrings
                    {
                        ConnectionString_SemanticKernelWebClient = configurationRoot["ConnectionString_SemanticKernelWebClient"] ?? ""
                    }
                };
            }

            return result;
        }
    }
}
