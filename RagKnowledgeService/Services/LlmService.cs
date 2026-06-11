using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using RagKnowledgeService.Models;

namespace RagKnowledgeService.Services
{
    public interface ILlmService
    {
        AskResponse GenerateGroundedAnswer(string question, List<KnowledgeChunk> contextualChunks);
    }

    public class LlmService : ILlmService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _modelName;

        public LlmService(IConfiguration configuration)
        {
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(45) };
            _baseUrl = configuration["OllamaSettings:BaseUrl"] ?? "http://ollama-engine:11434";
            _modelName = configuration["OllamaSettings:ModelName"] ?? "llama3.2";
        }

        public AskResponse GenerateGroundedAnswer(string question, List<KnowledgeChunk> contextualChunks)
        {
            var response = new AskResponse();

            // Guardrail: If no context chunks match, instantly flag low confidence without spending LLM cycles 
            if (contextualChunks == null || !contextualChunks.Any())
            {
                response.Answer = "I cannot confidently answer this query based on the current internal knowledge documents.";
                response.IsConfident = false;
                return response;
            }

            // Map retrieved context fragments to formal citation DTOs [cite: 14]
            response.Citations = contextualChunks.Select(c => new CitationDto
            {
                DocumentId = c.DocumentId,
                Title = c.Title,
                Snippet = c.TextContent.Length > 180 ? c.TextContent.Substring(0, 177) + "..." : c.TextContent
            }).ToList();

            // Construct a highly structured context payload for the prompt [cite: 8]
            string aggregatedContext = string.Join("\n\n", contextualChunks.Select(c => $"[Source ID: {c.DocumentId} | Title: {c.Title}]\nContent: {c.TextContent}"));

            // Engineer a strict prompt that forces the local model to flag lack of knowledge 
            string strictPrompt = $@"
You are a secure, grounding-focused corporate operations AI assistant. Your task is to answer the User Question using only the explicitly provided Context Documents.

=== CONTEXT DOCUMENTS ===
{aggregatedContext}
=========================

=== INSTRUCTIONS ===
1. Base your response completely on the Context Documents above.
2. Cite the Source ID (e.g., [DOC-001]) when referencing details.
3. CRITICAL ANTI-HALLUCINATION RULE: If the answer cannot be confidently derived directly from the given Context Documents, you MUST respond exactly with these words: 'NOT_FOUND: I cannot confidently answer this query based on the current internal knowledge documents.' Do not fabricate details. [cite: 39]

User Question: {question}
Answer:";

            try
            {
                var targetUrl = $"{_baseUrl.TrimEnd('/')}/api/generate";
                var payload = new OllamaGenerateRequest
                {
                    Model = _modelName,
                    Prompt = strictPrompt,
                    Stream = false, // Keep communication synchronous and simple for delivery 
                    Options = new Dictionary<string, object> { { "temperature", 0.0 } } // Forces deterministic accuracy
                };

                var httpResponse = _httpClient.PostAsJsonAsync(targetUrl, payload).GetAwaiter().GetResult();

                if (!httpResponse.IsSuccessStatusCode)
                {
                    throw new Exception($"Ollama endpoint returned structural error code: {httpResponse.StatusCode}");
                }

                var rawJson = httpResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                var ollamaResponse = JsonSerializer.Deserialize<OllamaGenerateResponse>(rawJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                string modelOutput = ollamaResponse?.Response?.Trim() ?? string.Empty;

                // Process anti-hallucination signal flags 
                if (modelOutput.StartsWith("NOT_FOUND:") || modelOutput.Contains("cannot confidently answer"))
                {
                    response.Answer = "I cannot confidently answer this query based on the current internal knowledge documents.";
                    response.IsConfident = false;
                }
                else
                {
                    response.Answer = modelOutput;
                    response.IsConfident = true;
                }
            }
            catch (Exception ex)
            {
                // Fallback graceful degradation for live environments
                response.Answer = $"Error contacting internal LLM engine: {ex.Message}";
                response.IsConfident = false;
            }

            return response;
        }
    }
}