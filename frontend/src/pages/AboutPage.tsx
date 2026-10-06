export function AboutPage() {
  return (
    <section className="page">
      <h1>About Our Firm</h1>
      <p className="lead">
        Brian & Sandra Law is a client-centered law firm focused on practical outcomes and ethical
        advocacy. We partner with clients from first consultation through final resolution.
      </p>

      <section className="content-grid">
        <article className="card">
          <h2>Our mission</h2>
          <p>
            Provide dependable legal representation that protects client rights, reduces uncertainty, and
            helps people move forward confidently.
          </p>
        </article>
        <article className="card">
          <h2>Our values</h2>
          <ul>
            <li>Integrity in every recommendation.</li>
            <li>Preparation before every negotiation or hearing.</li>
            <li>Respectful, clear communication at every step.</li>
          </ul>
        </article>
      </section>
    </section>
  );
}
