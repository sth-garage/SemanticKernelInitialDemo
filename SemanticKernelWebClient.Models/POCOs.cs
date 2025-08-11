using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SemanticKernelWebClient.Models
{
    public class ModelAndKey
    {
        public string ModelId { get; set; }

        public string Key { get; set; }
    }

    public class AgentFromWeb
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public bool FinalReviewer { get; set; }
    }

    public class AgentPayload
    {
        public string Type { get; set; }

        public List<AgentFromWeb> Agents { get; set; } = new List<AgentFromWeb>();
    }


    public class FinanceInfo
    {
        [VectorStoreKey]
        public Guid Key { get; set; } = Guid.NewGuid();

        [VectorStoreData]
        public string Text { get; set; } = string.Empty;

        // Note that the vector property is typed as a string, and
        // its value is derived from the Text property. The string
        // value will however be converted to a vector on upsert and
        // stored in the database as a vector.
        [VectorStoreVector(1536)]
        public string Embedding => this.Text;
    }


    public class SKQuickTestOptions
    {
        public Kernel Kernel { get; set; }

        public bool ShouldTestImage { get; set; } = false;

        public bool ShouldTestTextToAudio { get; set; } = false;

        public bool ShouldTestLocalRAG { get; set; } = false;

        public bool ShouldTestRAGSearch { get; set; } = false;


        public bool ShouldTestRAGUploadAndSearch { get; set; } = false;

    }

    public class SemanticKernelBuilderResult
    {
        public IChatCompletionService ChatCompletionService { get; set; }

        public QdrantVectorStore QdrantVectorStore { get; set; }

        public Kernel Kernel { get; set; }
    }
}
