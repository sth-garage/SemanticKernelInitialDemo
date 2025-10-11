
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using System.ComponentModel.DataAnnotations;

namespace SemanticKernelWebClient.Models
{
    public class ModelAndKey
    {
        [Required]
        public string ModelId { get; set; }

        [Required]
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

        public bool ShouldAddTestRAGPlugin { get; set; } = false;

        public bool ShouldTestUPSDemo { get; set; } = false;

        public bool ShouldUploadToRag { get; set; } = false;

        public bool ShouldAnalyzeCBR { get; set; } = false;


    }

    public class SemanticKernelBuilderResult
    {
        public AIServices AIServices { get; set; } = new AIServices();
    }


    public class AIServices
    {
        public IChatCompletionService ChatCompletionService { get; set; }

        public QdrantVectorStore QdrantVectorStore { get; set; }

        public Kernel Kernel { get; set; }

        public IEmbeddingGenerator<string, Embedding<float>> EmbeddingGenerator { get; set; }
    }

    public class SimpleDocumentEntry
    {
        public string Name { get; set; } = "";

        public string Description { get; set; } = "";

        public string TextContent { get; set; } = "";

        public string Category { get; set; } = "";

        public List<String> Terms { get; set; } = new List<String>();


    }


    public class UploadInput
    {
        public Kernel Kernel { get; set; }
        public ConfigurationValues ConfigValues { get; set; }
        public IChatCompletionService ChatCompletionService { get; set; }
    }

    public class RAGUploadEntry
    {
        public string Collection { get; set; }
        public string Category { get; set; }
        public List<string> Terms { get; set; } = new List<string>();

        public string TextContent { get; set; }
    }


    /// <summary>
    /// Sample model class that represents a glossary entry.
    /// </summary>
    /// <remarks>
    /// Note that each property is decorated with an attribute that specifies how the property should be treated by the vector store.
    /// This allows us to create a collection in the vector store and upsert and retrieve instances of this class without any further configuration.
    /// </remarks>
    /// <typeparam name="TKey">The type of the model key.</typeparam>
    public sealed class Glossary<TKey>
    {
        [VectorStoreKey]
        public TKey Key { get; set; }

        [VectorStoreData(IsIndexed = true)]
        public string Category { get; set; }

        [VectorStoreData]
        public string Term { get; set; }

        [VectorStoreData]
        public string Definition { get; set; }

        [VectorStoreVector(1536)]
        public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
    }



    public enum RAGCollections
    {
        TestCollection,
        Recipes
    }

    public enum TestCollectionCategories
    {
        TestCategory
    }


}
