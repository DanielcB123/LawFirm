import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { useAuth } from "../../features/auth";
import { ApiClientError } from "../../lib/apiClient";
import { getMatterDocuments, getMatterList } from "../../features/matters";
import type { MatterDocument } from "../../features/matters";

type DocumentRow = MatterDocument & {
  matterId: string;
  matterNumber: string;
  matterTitle: string;
};

export function DocumentsPage() {
  const { token } = useAuth();
  const [rows, setRows] = useState<DocumentRow[]>([]);
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
        const matters = await getMatterList(token);
        const documents = await Promise.all(
          matters.map(async (matter) => {
            const matterDocuments = await getMatterDocuments(token, matter.id);
            return matterDocuments.map(
              (document): DocumentRow => ({
                ...document,
                matterId: matter.id,
                matterNumber: matter.matterNumber,
                matterTitle: matter.title,
              }),
            );
          }),
        );

        setRows(
          documents
            .flat()
            .sort((left, right) => new Date(right.uploadedAtUtc).getTime() - new Date(left.uploadedAtUtc).getTime()),
        );
      } catch (err) {
        setError(err instanceof ApiClientError ? err.message : "Failed to load documents.");
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [token]);

  return (
    <section className="page">
      <h1>Documents</h1>
      <p className="lead">Files saved on the matters you can open.</p>
      {error && <p className="error-text">{error}</p>}
      {isLoading && <p>Loading documents...</p>}
      {!isLoading && !error && rows.length === 0 && <p>No documents are saved on your matters.</p>}
      {!isLoading && rows.length > 0 && (
        <article className="card">
          <p className="muted">
            {rows.length} {rows.length === 1 ? "document" : "documents"}
          </p>
          <ul className="list-reset">
            {rows.map((row) => (
              <li key={row.id} className="contact-row">
                <div className="contact-row__main">
                  <Link to={`/workspace/matters/${row.matterId}/documents`}>
                    <strong>{row.fileName}</strong>
                  </Link>
                  <span className="contact-row__type">{row.documentType}</span>
                  <span className="contact-row__badge">{row.reviewStatus}</span>
                </div>
                <p className="muted">
                  {row.matterNumber} · {row.matterTitle}
                </p>
                <p className="muted">
                  Uploaded by {row.uploadedByDisplayName} · {new Date(row.uploadedAtUtc).toLocaleString()}
                </p>
              </li>
            ))}
          </ul>
        </article>
      )}
    </section>
  );
}
