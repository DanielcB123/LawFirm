import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../features/auth";
import { getConsultationQueue } from "../../features/consultations";
import type { ConsultationRequestItem } from "../../features/consultations";
import { ApiClientError } from "../../lib/apiClient";

export function CommunicationsPage() {
  const { token } = useAuth();
  const [messages, setMessages] = useState<ConsultationRequestItem[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    async function load() {
      if (!token) {
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        const queue = await getConsultationQueue(token);
        setMessages(
          [...queue].sort((left, right) => new Date(right.createdAtUtc).getTime() - new Date(left.createdAtUtc).getTime()),
        );
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load communications.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [token]);

  return (
    <section className="page">
      <h1>Communications</h1>
      <p className="lead">
        Messages people sent when they asked for a consultation. Secure messages inside a matter are not available yet.
      </p>
      {error && <p className="error-text">{error}</p>}
      {isLoading && <p>Loading messages...</p>}
      {!isLoading && !error && messages.length === 0 && <p>No consultation messages yet.</p>}
      {!isLoading && messages.length > 0 && (
        <article className="card">
          <p className="muted">
            {messages.length} {messages.length === 1 ? "message" : "messages"} ·{" "}
            <Link to="/workspace/consultations">Open the consultation queue</Link>
          </p>
          <ul className="list-reset">
            {messages.map((message) => (
              <li key={message.id} className="contact-row">
                <div className="contact-row__main">
                  <strong>{message.fullName}</strong>
                  <span className="contact-row__type">{message.status}</span>
                </div>
                <p className="muted">
                  {message.email}
                  {message.phone ? ` · ${message.phone}` : ""} · {message.practiceArea}
                </p>
                <p>{message.message?.trim() ? message.message : "No message text."}</p>
                <p className="muted">{new Date(message.createdAtUtc).toLocaleString()}</p>
              </li>
            ))}
          </ul>
        </article>
      )}
    </section>
  );
}
