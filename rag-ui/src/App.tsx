import React, { useState } from "react";
import {
  Search,
  BookOpen,
  AlertTriangle,
  CheckCircle,
  Cpu,
} from "lucide-react";

interface Citation {
  documentId: string;
  title: string;
  snippet: string;
}

interface AskResponse {
  answer: string;
  isConfident: boolean;
  citations: Citation[];
}

export default function App() {
  const [question, setQuestion] = useState("");
  const [loading, setLoading] = useState(false);
  const [result, setResult] = useState<AskResponse | null>(null);
  const [error, setError] = useState<string | null>(null);

  const handleAsk = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!question.trim()) return;

    setLoading(true);
    setError(null);
    setResult(null);

    try {
      const response = await fetch("http://localhost:5209/api/knowledge/ask", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ question }),
      });

      if (!response.ok) {
        throw new Error(`Server returned HTTP status ${response.status}`);
      }

      const data: AskResponse = await response.json();
      setResult(data);
    } catch (err: any) {
      setError(err.message || "Failed to communicate with RAG Service.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div
      style={{
        fontFamily: "system-ui, sans-serif",
        backgroundColor: "#f3f4f6",
        minHeight: "100vh",
        padding: "2rem",
      }}
    >
      {/* Header Banner */}
      <header
        style={{
          maxWidth: "1200px",
          margin: "0 auto 2rem auto",
          display: "flex",
          alignItems: "center",
          gap: "0.75rem",
        }}
      >
        <Cpu size={32} color="#2563eb" />
        <div>
          <h1 style={{ margin: 0, fontSize: "1.5rem", color: "#1f2937" }}>
            Internal RAG Knowledge Hub
          </h1>
          <p style={{ margin: 0, fontSize: "0.875rem", color: "#4b5563" }}>
            Grounded QA Retrieval Assistant
          </p>
        </div>
      </header>

      <main
        style={{
          maxWidth: "1200px",
          margin: "0 auto",
          display: "grid",
          gridTemplateColumns: "1fr 1fr",
          gap: "2rem",
        }}
      >
        {/* Left Side: Query Panel */}
        <section
          style={{
            backgroundColor: "#ffffff",
            padding: "1.5rem",
            borderRadius: "8px",
            boxShadow: "0 1px 3px rgba(0,0,0,0.1)",
          }}
        >
          <h2
            style={{
              fontSize: "1.25rem",
              marginTop: 0,
              marginBottom: "1rem",
              color: "#374151",
            }}
          >
            Ask a Question
          </h2>
          <form onSubmit={handleAsk}>
            <textarea
              value={question}
              onChange={(e) => setQuestion(e.target.value)}
              placeholder="e.g., What are the standard tier API rate limits or remote stipends?"
              rows={4}
              style={{
                width: "100%",
                boxSizing: "border-box",
                padding: "0.75rem",
                borderRadius: "6px",
                border: "1px solid #d1d5db",
                fontSize: "1rem",
                resize: "vertical",
                marginBottom: "1rem",
              }}
            />
            <button
              type="submit"
              disabled={loading}
              style={{
                width: "100%",
                padding: "0.75rem",
                backgroundColor: loading ? "#9ca3af" : "#2563eb",
                color: "#ffffff",
                border: "none",
                borderRadius: "6px",
                fontSize: "1rem",
                cursor: loading ? "not-allowed" : "pointer",
                display: "flex",
                justifyContent: "center",
                alignItems: "center",
                gap: "0.5rem",
              }}
            >
              <Search size={18} />
              {loading ? "Retrieving Answers..." : "Submit Query"}
            </button>
          </form>

          {error && (
            <div
              style={{
                marginTop: "1rem",
                padding: "1rem",
                backgroundColor: "#fee2e2",
                borderRadius: "6px",
                color: "#991b1b",
                display: "flex",
                gap: "0.5rem",
              }}
            >
              <AlertTriangle size={20} />
              <span>{error}</span>
            </div>
          )}
        </section>

        {/* Right Side: Generated Output & Grounded Sources */}
        <section
          style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}
        >
          {result ? (
            <>
              {/* Answer Engine Presentation Block */}
              <div
                style={{
                  backgroundColor: "#ffffff",
                  padding: "1.5rem",
                  borderRadius: "8px",
                  boxShadow: "0 1px 3px rgba(0,0,0,0.1)",
                  borderTop: result.isConfident
                    ? "4px solid #16a34a"
                    : "4px solid #ea580c",
                }}
              >
                <div
                  style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    marginBottom: "0.75rem",
                  }}
                >
                  <h3
                    style={{ margin: 0, fontSize: "1.15rem", color: "#374151" }}
                  >
                    Generated Answer
                  </h3>
                  {result.isConfident ? (
                    <span
                      style={{
                        display: "flex",
                        alignItems: "center",
                        gap: "0.25rem",
                        color: "#16a34a",
                        fontSize: "0.85rem",
                        fontWeight: "bold",
                      }}
                    >
                      <CheckCircle size={16} /> Grounded Confidence
                    </span>
                  ) : (
                    <span
                      style={{
                        display: "flex",
                        alignItems: "center",
                        gap: "0.25rem",
                        color: "#ea580c",
                        fontSize: "0.85rem",
                        fontWeight: "bold",
                      }}
                    >
                      <AlertTriangle size={16} /> Low Confidence Signal
                    </span>
                  )}
                </div>
                <p style={{ color: "#111827", lineHeight: "1.6", margin: 0 }}>
                  {result.answer}
                </p>
              </div>

              {/* Source Document Citations Section */}
              <div
                style={{
                  backgroundColor: "#ffffff",
                  padding: "1.5rem",
                  borderRadius: "8px",
                  boxShadow: "0 1px 3px rgba(0,0,0,0.1)",
                }}
              >
                <h3
                  style={{
                    margin: "0 0 1rem 0",
                    fontSize: "1.15rem",
                    color: "#374151",
                    display: "flex",
                    alignItems: "center",
                    gap: "0.5rem",
                  }}
                >
                  <BookOpen size={18} color="#4b5563" /> Grounded Source
                  Citations ({result.citations.length})
                </h3>
                {result.citations.length > 0 ? (
                  <div
                    style={{
                      display: "flex",
                      flexDirection: "column",
                      gap: "1rem",
                    }}
                  >
                    {result.citations.map((citation, idx) => (
                      <div
                        key={idx}
                        style={{
                          padding: "0.75rem",
                          backgroundColor: "#f9fafb",
                          borderRadius: "6px",
                          borderLeft: "3px solid #6b7280",
                        }}
                      >
                        <div
                          style={{
                            display: "flex",
                            justifyContent: "space-between",
                            marginBottom: "0.25rem",
                          }}
                        >
                          <span
                            style={{
                              fontWeight: "bold",
                              fontSize: "0.875rem",
                              color: "#374151",
                            }}
                          >
                            {citation.title}
                          </span>
                          <span
                            style={{
                              fontSize: "0.75rem",
                              color: "#6b7280",
                              backgroundColor: "#e5e7eb",
                              padding: "2px 6px",
                              borderRadius: "4px",
                            }}
                          >
                            {citation.documentId}
                          </span>
                        </div>
                        <p
                          style={{
                            margin: 0,
                            fontSize: "0.875rem",
                            color: "#4b5563",
                            fontStyle: "italic",
                          }}
                        >
                          "{citation.snippet}"
                        </p>
                      </div>
                    ))}
                  </div>
                ) : (
                  <p
                    style={{
                      margin: 0,
                      fontSize: "0.875rem",
                      color: "#9ca3af",
                      fontStyle: "italic",
                    }}
                  >
                    No document identifiers are cited for this result.
                  </p>
                )}
              </div>
            </>
          ) : (
            <div
              style={{
                flex: 1,
                backgroundColor: "#ffffff",
                borderRadius: "8px",
                border: "2px dashed #d1d5db",
                display: "flex",
                justifyContent: "center",
                alignItems: "center",
                color: "#6b7280",
                minHeight: "200px",
              }}
            >
              Submit an internal policy question to evaluate RAG retrieval
              performance.
            </div>
          )}
        </section>
      </main>
    </div>
  );
}
