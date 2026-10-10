import { Link } from "react-router-dom";
import { teamMembers } from "../data/teamMembers";

const attorneys = teamMembers.filter((member) =>
  ["Founding Partner", "Partner", "Senior Associate"].some((keyword) => member.role.includes(keyword)),
);

const collaborationPoints = [
  "Lead attorney assigned based on case complexity and timeline.",
  "Dedicated support from legal assistant and paralegal staff.",
  "Strategic check-ins at each major filing or negotiation point.",
];

export function AttorneysPage() {
  return (
    <section className="page marketing-page">
      <section className="marketing-hero marketing-hero--attorneys" data-reveal="zoom">
        <p className="eyebrow">Our attorneys</p>
        <h1>Meet the legal team representing your next big decision.</h1>
        <p className="lead">
          Our team combines litigation experience with practical counseling so you can make informed legal decisions
          with confidence.
        </p>
      </section>

      <section className="content-grid marketing-grid" aria-label="Attorney profiles" data-reveal>
        {attorneys.map((attorney) => (
          <Link
            key={attorney.slug}
            to={`/attorneys/${attorney.slug}`}
            className="card marketing-card marketing-card--lift attorney-card attorney-card--link"
            data-reveal="zoom"
          >
            <img src={attorney.image} alt={attorney.name} loading="lazy" />
            <h2>{attorney.name}</h2>
            <p className="eyebrow attorney-card__role">{attorney.role}</p>
            <p className="attorney-card__bio">{attorney.shortBio}</p>
            <span className="attorney-card__action">View full profile</span>
          </Link>
        ))}
      </section>

      <section className="content-grid marketing-grid" data-reveal="left">
        <article className="card marketing-card" data-reveal="left">
          <h2>How your legal team works together</h2>
          <ul>
            {collaborationPoints.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
        <article className="card marketing-card" data-reveal="right">
          <h2>Planning your first meeting</h2>
          <p>
            Bring any contracts, notices, and key dates. We use your first meeting to map legal priorities, immediate
            risks, and the most effective next steps.
          </p>
        </article>
      </section>

      <section className="card marketing-cta" data-reveal="zoom">
        <p className="marketing-cta__kicker">Start with the right attorney</p>
        <h2>Share your goals and we will match your matter to the right legal lead.</h2>
        <Link className="hero-button hero-button--primary" to="/contact">
          Request consultation
        </Link>
      </section>
    </section>
  );
}
