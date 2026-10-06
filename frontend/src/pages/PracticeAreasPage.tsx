const practiceAreas = [
  {
    title: "Family Law",
    description:
      "Divorce, custody, support, and parenting plans handled with compassion and strategic focus.",
  },
  {
    title: "Business Litigation",
    description:
      "Contract disputes, partnership conflicts, and commercial claims for small and midsize businesses.",
  },
  {
    title: "Estate Planning",
    description:
      "Wills, trusts, powers of attorney, and probate guidance to protect families and assets.",
  },
  {
    title: "Personal Injury",
    description:
      "Representation for individuals injured by negligence, with a focus on full and fair compensation.",
  },
];

export function PracticeAreasPage() {
  return (
    <section className="page">
      <h1>Practice Areas</h1>
      <p className="lead">
        We offer focused legal services across core civil practice areas, with strategy adapted to your
        specific objectives and timeline.
      </p>

      <section className="content-grid" aria-label="Practice areas">
        {practiceAreas.map((area) => (
          <article key={area.title} className="card">
            <h2>{area.title}</h2>
            <p>{area.description}</p>
          </article>
        ))}
      </section>
    </section>
  );
}
