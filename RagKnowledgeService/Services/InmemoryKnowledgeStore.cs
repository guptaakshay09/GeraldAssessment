using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using RagKnowledgeService.Models;

namespace RagKnowledgeService.Services
{
    public interface IKnowledgeStore
    {
        void InitializeInboundData();
        List<KnowledgeChunk> GetTopKChunks(string query, int topK);
    }

    public class InmemoryKnowledgeStore : IKnowledgeStore
    {
        private readonly List<KnowledgeChunk> _chunks = new();
        private readonly RagConfig _config;

        public InmemoryKnowledgeStore(IOptions<RagConfig> config)
        {
            _config = config.Value;
        }

        public void InitializeInboundData()
        {
            _chunks.Clear();
            var dataPath = Path.Combine(AppContext.BaseDirectory, "Data");
            if (!Directory.Exists(dataPath)) return;

            var files = Directory.GetFiles(dataPath, "*.md");
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                ParseAndChunkDocument(file, content);
            }
        }

        private void ParseAndChunkDocument(string filePath, string content)
        {
            var fileName = Path.GetFileName(filePath);
            var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            // Extract a clean Document ID and Title if available
            string docId = fileName;
            string title = fileName;
            
            var idLine = lines.FirstOrDefault(l => l.StartsWith("ID:"));
            if (idLine != null) docId = idLine.Replace("ID:", "").Trim();

            var titleLine = lines.FirstOrDefault(l => l.StartsWith("# "));
            if (titleLine != null) title = titleLine.Replace("# ", "").Trim();

            // Simple robust paragraph/character chunking based on configuration
            var words = content.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var currentChunkWords = new List<string>();
            int currentLength = 0;

            foreach (var word in words)
            {
                currentChunkWords.Add(word);
                currentLength += word.Length + 1;

                if (currentLength >= _config.ChunkSize)
                {
                    _chunks.Add(new KnowledgeChunk
                    {
                        DocumentId = docId,
                        Title = title,
                        TextContent = string.Join(" ", currentChunkWords)
                    });
                    currentChunkWords.Clear();
                    currentLength = 0;
                }
            }

            if (currentChunkWords.Any())
            {
                _chunks.Add(new KnowledgeChunk
                {
                    DocumentId = docId,
                    Title = title,
                    TextContent = string.Join(" ", currentChunkWords)
                });
            }
        }

        // Simulates semantic score ranking using term intersections 
        public List<KnowledgeChunk> GetTopKChunks(string query, int topK)
        {
            var queryTokens = query.ToLowerInvariant()
                .Split(new[] { ' ', '?', '.', ',' }, StringSplitOptions.RemoveEmptyEntries);

            var ranked = _chunks.Select(chunk =>
            {
                var text = chunk.TextContent.ToLowerInvariant();
                // Score based on keyword hits to mimic an inverted/vector index match
                double score = queryTokens.Count(token => text.Contains(token));
                
                return new { Chunk = chunk, Score = score };
            })
            .Where(x => x.Score > 0) // Ensure there is at least some alignment
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Chunk)
            .ToList();

            return ranked;
        }
    }
}