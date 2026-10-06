const practiceAreas = [
  {
    title: "Civil Litigation",
    description:
      "Case strategy and courtroom representation for complex disputes requiring decisive legal advocacy.",
    focus: "Evidence-driven preparation with negotiation leverage and trial-readiness.",
  },
  {
    title: "Business Litigation",
    description:
      "Contract disputes, partnership conflicts, and commercial claims for small and midsize businesses.",
    focus: "Protection of operations, reputation, and long-term growth plans.",
  },
  {
    title: "Estate Planning",
    description:
      "Wills, trusts, powers of attorney, and probate guidance to protect families and assets.",
    focus: "Simple, practical planning documents tailored to your household and goals.",
  },
  {
    title: "Personal Injury",
    description:
      "Representation for individuals injured by negligence, with a focus on full and fair compensation.",
    focus: "Clear case timelines and assertive negotiation with insurers.",
  },
];

export function PracticeAreasPage() {
  return (
    <section className="page marketing-page">
      <section className="marketing-hero marketing-hero--practice">
        <p className="eyebrow">Practice areas</p>
        <h1>Legal services designed around your goals, risks, and timeline.</h1>
        <p className="lead">
          We offer focused legal services across core civil practice areas, with strategy adapted to your specific
          objectives and timeline.
        </p>
      </section>

      <section className="content-grid marketing-grid" aria-label="Practice areas">
        {practiceAreas.map((area) => (
          <article key={area.title} className="card marketing-card marketing-card--lift">
            <h2>{area.title}</h2>
            <p>{area.description}</p>
            <p className="marketing-card__meta">{area.focus}</p>
          </article>
        ))}
      </section>

      <section className="marketing-highlight marketing-highlight--practice">
        <h2>From initial consultation to final resolution</h2>
        <p>
          Every case begins with a clear legal roadmap, expected milestones, and defined communication rhythms so you
          can make informed decisions quickly.
        </p>
      </section>
    </section>
  );
}
