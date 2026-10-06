import { useEffect, useState } from "react";
import { getHealthStatus } from "../services/healthApi";
import type { HealthResponse } from "../types/health";

export function HealthStatusCard() {
  const [health, setHealth] = useState<HealthResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadHealth() {
      try {
        const result = await getHealthStatus();
        setHealth(result);
      } catch (err) {
        const message = err instanceof Error ? err.message : "Unknown error";
        setError(message);
      } finally {
        setIsLoading(false);
      }
    }

    void loadHealth();
  }, []);

  return (
    <div>
      <h2>System Status</h2>
      {isLoading && <p>Checking backend health...</p>}
      {error && <p>API error: {error}</p>}
      {health && (
        <p>
          <strong>API status:</strong> {health.status}
        </p>
      )}
    </div>
  );
}
