using RagKnowledgeService.Models;
using RagKnowledgeService.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Core API Controllers and JSON formatting configuration
builder.Services.AddControllers();

// 2. Bind RAG Configuration parameters from appsettings
builder.Services.Configure<RagConfig>(options =>
{
    options.ChunkSize = 250; // Character count for chunk evaluation [cite: 18]
    options.TopK = 3;        // Top matches to extract [cite: 18]
});

// 3. Register Application Services inside DI pipeline
builder.Services.AddSingleton<IKnowledgeStore, InmemoryKnowledgeStore>();
builder.Services.AddTransient<ILlmService, LlmService>();
builder.Services.AddTransient<IRagService, RagService>();

// 4. Configure cross-origin isolation (CORS) for local React interaction
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Enable routing endpoints and route access parameters
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();

// Automatically parse source documents into memory upon initialization [cite: 12]
try
{
    app.Services.GetRequiredService<IKnowledgeStore>().InitializeInboundData();
}
catch (Exception ex)
{
    Console.WriteLine($"Warning: Initial ingestion step failed: {ex.Message}");
}

app.Run();