const attorneys = [
  {
    name: "Jordan Parker",
    role: "Founding Partner",
    bio: "Focuses on family law and mediation with over 15 years of courtroom and settlement experience.",
  },
  {
    name: "Alex Reed",
    role: "Partner",
    bio: "Leads business litigation matters and advises owners on dispute prevention and risk management.",
  },
  {
    name: "Morgan Lee",
    role: "Senior Associate",
    bio: "Supports estate planning and probate clients with clear, detail-oriented counsel.",
  },
];

export function AttorneysPage() {
  return (
    <section className="page">
      <h1>Our Attorneys</h1>
      <p className="lead">
        Our team combines litigation experience with practical counseling so you can make informed legal
        decisions with confidence.
      </p>

      <section className="content-grid" aria-label="Attorney profiles">
        {attorneys.map((attorney) => (
          <article key={attorney.name} className="card">
            <h2>{attorney.name}</h2>
            <p className="eyebrow">{attorney.role}</p>
            <p>{attorney.bio}</p>
          </article>
        ))}
      </section>
    </section>
  );
}
