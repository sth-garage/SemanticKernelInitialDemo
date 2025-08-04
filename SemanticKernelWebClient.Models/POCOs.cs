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
}
