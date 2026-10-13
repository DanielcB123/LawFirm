import type { HealthResponse } from "./types";
import { getJson } from "../../lib/apiClient";

export function getHealthStatus() {
  return getJson<HealthResponse>("/api/health");
}
