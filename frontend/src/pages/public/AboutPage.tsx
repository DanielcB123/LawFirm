import { Link } from "react-router-dom";

const differentiators = [
  "Direct attorney access from consultation through resolution.",
  "Proactive updates so clients are never left guessing.",
  "Trial-ready preparation that strengthens negotiation outcomes.",
];

const firmTimeline = [
  { year: "2012", milestone: "Firm founded to provide litigation-ready legal support for families and businesses." },
  { year: "2016", milestone: "Expanded civil and business dispute team with dedicated trial preparation workflows." },
  { year: "2021", milestone: "Introduced digital intake and consultation planning for faster case kickoff." },
  { year: "2026", milestone: "Modernized client experience with transparent status updates and strategic planning sessions." },
];

const servicePillars = [
  "Case strategy rooted in facts, deadlines, and measurable risk.",
  "Clear communication cadence so clients stay informed throughout every phase.",
  "Collaborative planning with specialists when matters require extended expertise.",
];

export function AboutPage() {
  return (
    <section className="page marketing-page">
      <section className="marketing-hero marketing-hero--about" data-reveal="zoom">
        <p className="eyebrow">About our firm</p>
        <h1>Experienced counsel built on clarity, trust, and preparation.</h1>
        <p className="lead">
          Brian & Sandra Law is a client-centered law firm focused on practical outcomes and ethical advocacy. We
          partner with clients from first consultation through final resolution.
        </p>
      </section>

      <section className="content-grid marketing-grid" data-reveal="left">
        <article className="card marketing-card marketing-card--lift" data-reveal>
          <h2>Our mission</h2>
          <p>
            Provide dependable legal representation that protects client rights, reduces uncertainty, and helps people
            move forward confidently.
          </p>
        </article>
        <article className="card marketing-card marketing-card--lift" data-reveal="right">
          <h2>Our values</h2>
          <ul>
            <li>Integrity in every recommendation.</li>
            <li>Preparation before every negotiation or hearing.</li>
            <li>Respectful, clear communication at every step.</li>
          </ul>
        </article>
      </section>

      <section className="marketing-highlight marketing-highlight--about" data-reveal>
        <h2>What sets our team apart</h2>
        <ul>
          {differentiators.map((item) => (
            <li key={item}>{item}</li>
          ))}
        </ul>
      </section>

      <section className="content-grid marketing-grid" data-reveal="zoom">
        <article className="card marketing-card" data-reveal="left">
          <h2>Firm journey</h2>
          <ul>
            {firmTimeline.map((item) => (
              <li key={item.year}>
                <strong>{item.year}:</strong> {item.milestone}
              </li>
            ))}
          </ul>
        </article>
        <article className="card marketing-card" data-reveal="right">
          <h2>Service pillars</h2>
          <ul>
            {servicePillars.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
      </section>

      <section className="card marketing-cta" data-reveal="zoom">
        <p className="marketing-cta__kicker">Need legal guidance now?</p>
        <h2>Talk to a team that combines empathy with legal precision.</h2>
        <Link className="hero-button hero-button--primary" to="/contact">
          Contact our office
        </Link>
      </section>
    </section>
  );
}
