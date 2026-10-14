const integrations = [
  { name: "Email", detail: "Send and file client email from the workspace." },
  { name: "Calendar sync", detail: "Keep deadlines on an outside calendar." },
  { name: "Payments", detail: "Take card or bank payments against an invoice." },
  { name: "E-signature", detail: "Send engagement letters out for signature." },
  { name: "Document storage", detail: "Store matter files in a connected drive." },
];

export function IntegrationsPage() {
  return (
    <section className="page">
      <h1>Integrations</h1>
      <p className="lead">Outside services the firm can connect later. None are connected.</p>
      <ul className="list-reset content-grid">
        {integrations.map((integration) => (
          <li key={integration.name} className="card">
            <h2>{integration.name}</h2>
            <p>{integration.detail}</p>
            <p className="muted">Not connected</p>
          </li>
        ))}
      </ul>
    </section>
  );
}
