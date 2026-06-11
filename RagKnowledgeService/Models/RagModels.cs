namespace RagKnowledgeService.Models
{
    public class KnowledgeChunk
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string DocumentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string TextContent { get; set; } = string.Empty;
        public float[]? Embedding { get; set; }
    }

    public class RagConfig
    {
        public int ChunkSize { get; set; } = 200; // Character count approximation
        public int TopK { get; set; } = 3;
    }

    public class AskRequest
    {
        public string Question { get; set; } = string.Empty;
    }

    public class AskResponse
    {
        public string Answer { get; set; } = string.Empty;
        public bool IsConfident { get; set; } = true;
        public List<CitationDto> Citations { get; set; } = new();
    }

    public class CitationDto
    {
        public string DocumentId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Snippet { get; set; } = string.Empty;
    }

    // Ollama API request payload schema
    public class OllamaGenerateRequest
    {
        public string Model { get; set; } = "llama3.2";
        public string Prompt { get; set; } = string.Empty;
        public bool Stream { get; set; } = false;
        public Dictionary<string, object>? Options { get; set; }
    }

    // Ollama API response payload schema
    public class OllamaGenerateResponse
    {
        public string Model { get; set; } = string.Empty;
        public string Response { get; set; } = string.Empty;
        public bool Done { get; set; }
    }
}