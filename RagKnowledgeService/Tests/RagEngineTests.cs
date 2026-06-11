using System.Collections.Generic;
using Microsoft.Extensions.Options;
using RagKnowledgeService.Models;
using RagKnowledgeService.Services;
using Xunit;

namespace RagKnowledgeService.Tests
{
    public class RagEngineTests
    {
        private readonly IOptions<RagConfig> _mockConfig;

        public RagEngineTests()
        {
            // Set up configurations matching target evaluation parameters
            _mockConfig = Options.Create(new RagConfig { ChunkSize = 200, TopK = 2 });
        }

        [Fact]
        public void RetrievalLayer_ShouldReturnCorrectChunks_WhenQueryMatchesKeywords()
        {
            // Arrange
            var store = new InmemoryKnowledgeStore(_mockConfig);
            // Simulate manually loading items for a predictable baseline
            var systemChunks = typeof(InmemoryKnowledgeStore)
                .GetField("_chunks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            var testChunks = new List<KnowledgeChunk>
            {
                new KnowledgeChunk { DocumentId = "DOC-001", Title = "Remote Work", TextContent = "Employees receive a monthly stipend of 50 dollars for remote internet access costs." },
                new KnowledgeChunk { DocumentId = "DOC-002", Title = "API limits", TextContent = "The gateway tracks metrics and implements strict rate limits capped at 1000 requests." }
            };
            systemChunks?.SetValue(store, testChunks);

            // Act
            var results = store.GetTopKChunks("What are the rate limits?", 2);

            // Assert
            Assert.NotEmpty(results);
            Assert.Equal("DOC-002", results[0].DocumentId);
            Assert.Contains("rate limits", results[0].TextContent);
        }

        [Fact]
        public void LlmService_ShouldReturnLowConfidence_WhenQueryIsNonsense()
        {
            // Arrange
            var llmService = new LlmService();
            var emptyChunks = new List<KnowledgeChunk>();

            // Act
            var response = llmService.GenerateGroundedAnswer("How do I bake a chocolate cake?", emptyChunks);

            // Assert
            Assert.False(response.IsConfident);
            Assert.Contains("cannot confidently answer", response.Answer);
            Assert.Empty(response.Citations);
        }
    }
}