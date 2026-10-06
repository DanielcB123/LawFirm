import { HealthStatusCard } from "../components/HealthStatusCard";

export function HomePage() {
  return (
    <section className="page">
      <div className="hero">
        <p className="eyebrow">Trusted legal counsel in Ohio</p>
        <h1>Clear guidance when legal decisions matter most.</h1>
        <p className="lead">
          Brian & Sandra Law helps individuals, families, and small businesses resolve legal issues with
          practical strategy, consistent communication, and strong courtroom advocacy.
        </p>
      </div>

      <section className="content-sgrid" aria-label="Firm highlights">
        <article className="card">
          <h2>Why clients choose us</h2>
          <ul>
            <li>Responsive communication and transparent billing.</li>
            <li>Focused experience in high-stakes civil matters.</li>
            <li>Personalized strategy tailored to your goals.</li>
          </ul>
        </article>
        <article className="card">
          <h2>Consultation hours</h2>
          <p>Monday-Friday: 8:30 AM-6:00 PM</p>
          <p>Saturday: By appointment</p>
          <p>Same-week consultations available for urgent matters.</p>
        </article>
      </section>

      <article className="card">
        <HealthStatusCard />
      </article>
    </section>
  );
}
