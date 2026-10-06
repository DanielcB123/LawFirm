import type { HealthResponse } from "../types/health";
import { getJson } from "./apiClient";

export function getHealthStatus() {
  return getJson<HealthResponse>("/api/health");
}
