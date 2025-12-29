using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Qdrant.Client;

namespace SemanticKernelWebClient.Models
{
    public class VectorStoreFactory
    {
        private IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        public VectorStoreFactory(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator)
        {
            _embeddingGenerator = embeddingGenerator;
        }

        public QdrantVectorStore VectorStore
        {
            get
            {
                var vectorStore = new QdrantVectorStore(
                   new QdrantClient("localhost"),
                   ownsClient: true,
                   new QdrantVectorStoreOptions
                   {
                       EmbeddingGenerator = _embeddingGenerator
                   });

                return vectorStore;
            }
        }
    }
}
