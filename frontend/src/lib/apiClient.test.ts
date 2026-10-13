import { describe, expect, it, vi } from "vitest";
import { ApiClientError, apiUrl, getJson } from "./apiClient";

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

describe("apiClient", () => {
  it("uses same-origin paths when no base URL is configured", () => {
    expect(apiUrl("/api/health")).toBe("/api/health");
  });

  it("sends the bearer token and returns the parsed body", async () => {
    const fetchMock = vi.fn().mockResolvedValue(jsonResponse(200, { status: "healthy" }));
    vi.stubGlobal("fetch", fetchMock);

    const result = await getJson<{ status: string }>("/api/health", undefined, "token-123");

    expect(result).toEqual({ status: "healthy" });
    const [, init] = fetchMock.mock.calls[0] as [string, RequestInit];
    expect((init.headers as Record<string, string>).Authorization).toBe("Bearer token-123");
  });

  it("maps the API error envelope to ApiClientError", async () => {
    vi.stubGlobal(
      "fetch",
      vi.fn().mockResolvedValue(
        jsonResponse(400, {
          correlationId: "corr-1",
          error: { code: "validation_failed", message: "Invalid input.", validationErrors: { title: ["Required."] } },
        }),
      ),
    );

    const error = await getJson("/api/matters-v2", undefined, "t").catch((caught: unknown) => caught);

    expect(error).toBeInstanceOf(ApiClientError);
    expect(error).toMatchObject({
      message: "Invalid input.",
      statusCode: 400,
      correlationId: "corr-1",
      validationErrors: { title: ["Required."] },
    });
  });

  it("falls back to a generic error when the body is not the envelope", async () => {
    vi.stubGlobal("fetch", vi.fn().mockResolvedValue(new Response("<html>", { status: 502, statusText: "Bad Gateway" })));

    await expect(getJson("/api/health")).rejects.toMatchObject({ statusCode: 502 });
  });
});
