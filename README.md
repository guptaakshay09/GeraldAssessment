# Internal RAG Knowledge Service Challenge

[cite_start]A lightweight, production-ready implementation of an AI-assisted Retrieval-Augmented Generation (RAG) knowledge engine[cite: 5, 8]. [cite_start]This solution splits, indexes, and queries a collection of internal product and policy markdown documents, serving grounded answers alongside precise source citations[cite: 8, 14, 39].

---

## 🛠️ Technology Stack & Tradeoffs

- [cite_start]**Backend:** .NET 8 Web API[cite: 5, 21]. [cite_start]Chosen for structural alignment with enterprise architectures, native dependency injection, and reliable performance[cite: 5, 6].
- [cite_start]**Frontend:** React (Vite + TypeScript)[cite: 5, 21]. [cite_start]Selected to establish a high-performance SPA dashboard with strict static typing to avoid runtime rendering friction.
- [cite_start]**Storage Layer:** In-Memory Processing & Token Alignment[cite: 21]. [cite_start]Given the strict 2-hour time boundary, an in-memory storage manager was used rather than provisioning external database hardware[cite: 21, 27]. [cite_start]This choice isolates storage components behind clean interfaces (`IKnowledgeStore`), enabling zero-friction drop-in scaling for a production database later[cite: 17, 24].
- [cite_start]**LLM Engine:** Mock Deterministic Fallback Engine[cite: 24]. [cite_start]Stubbed behind an `ILlmService` layer to guarantee 100% test and environment reliability without depending on active third-party API keys or rate-limiting barriers[cite: 24, 25].

---

## 🚀 How to Run the Solution

### 1. Start the Backend API

Navigate to the api folder and launch the web server hosting your endpoints:

````bash
cd RagKnowledgeService
dotnet run

Run test cases
```bash
dotnet test

### 2. Start the React FE
Navigate to rag-ui folder:
```bash
cd rag-ui
npm run dev
````

## Improvements:
