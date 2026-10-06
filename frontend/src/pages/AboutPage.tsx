import { Link } from "react-router-dom";

const differentiators = [
  "Direct attorney access from consultation through resolution.",
  "Proactive updates so clients are never left guessing.",
  "Trial-ready preparation that strengthens negotiation outcomes.",
];

export function AboutPage() {
  return (
    <section className="page marketing-page">
      <section className="marketing-hero marketing-hero--about">
        <p className="eyebrow">About our firm</p>
        <h1>Experienced counsel built on clarity, trust, and preparation.</h1>
        <p className="lead">
          Brian & Sandra Law is a client-centered law firm focused on practical outcomes and ethical advocacy. We
          partner with clients from first consultation through final resolution.
        </p>
      </section>

      <section className="content-grid marketing-grid">
        <article className="card marketing-card marketing-card--lift">
          <h2>Our mission</h2>
          <p>
            Provide dependable legal representation that protects client rights, reduces uncertainty, and helps people
            move forward confidently.
          </p>
        </article>
        <article className="card marketing-card marketing-card--lift">
          <h2>Our values</h2>
          <ul>
            <li>Integrity in every recommendation.</li>
            <li>Preparation before every negotiation or hearing.</li>
            <li>Respectful, clear communication at every step.</li>
          </ul>
        </article>
      </section>

      <section className="marketing-highlight marketing-highlight--about">
        <h2>What sets our team apart</h2>
        <ul>
          {differentiators.map((item) => (
            <li key={item}>{item}</li>
          ))}
        </ul>
      </section>

      <section className="card marketing-cta">
        <p className="marketing-cta__kicker">Need legal guidance now?</p>
        <h2>Talk to a team that combines empathy with legal precision.</h2>
        <Link className="hero-button hero-button--primary" to="/contact">
          Contact our office
        </Link>
      </section>
    </section>
  );
}
