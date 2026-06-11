# Product: Apex API Rate Limits

ID: DOC-002
Last Updated: 2026-03-01

The Apex API enforces a strict rate limit of 1,000 requests per minute (RPM) for standard tier API keys.
If an application exceeds this limit, the gateway responds with an HTTP 429 Too Many Requests status code.
Enterprise tier keys can be scaled up to 50,000 RPM upon approval from the infrastructure board.
