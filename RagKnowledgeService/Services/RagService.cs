using System.Collections.Generic;
using Microsoft.Extensions.Options;
using RagKnowledgeService.Models;

namespace RagKnowledgeService.Services
{
    public interface IRagService
    {
        AskResponse Ask(string question);
    }

    public class RagService : IRagService
    {
        private readonly IKnowledgeStore _knowledgeStore;
        private readonly ILlmService _llmService;
        private readonly RagConfig _config;

        public RagService(IKnowledgeStore knowledgeStore, ILlmService llmService, IOptions<RagConfig> config)
        {
            _knowledgeStore = knowledgeStore;
            _llmService = llmService;
            _config = config.Value;

            // Auto-trigger internal dataset preparation on bootstrap
            _knowledgeStore.InitializeInboundData();
        }

        public AskResponse Ask(string question)
        {
            // 1. Retrieval
            List<KnowledgeChunk> relevantChunks = _knowledgeStore.GetTopKChunks(question, _config.TopK);

            // 2. Generation & Verification
            return _llmService.GenerateGroundedAnswer(question, relevantChunks);
        }
    }
}