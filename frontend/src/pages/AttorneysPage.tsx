import { Link } from "react-router-dom";
import { teamMembers } from "../data/teamMembers";

const attorneys = teamMembers.filter((member) =>
  ["Founding Partner", "Partner", "Senior Associate"].some((keyword) => member.role.includes(keyword)),
);

export function AttorneysPage() {
  return (
    <section className="page marketing-page">
      <section className="marketing-hero marketing-hero--attorneys">
        <p className="eyebrow">Our attorneys</p>
        <h1>Meet the legal team representing your next big decision.</h1>
        <p className="lead">
          Our team combines litigation experience with practical counseling so you can make informed legal decisions
          with confidence.
        </p>
      </section>

      <section className="content-grid marketing-grid" aria-label="Attorney profiles">
        {attorneys.map((attorney) => (
          <Link
            key={attorney.slug}
            to={`/attorneys/${attorney.slug}`}
            className="card marketing-card marketing-card--lift attorney-card attorney-card--link"
          >
            <img src={attorney.image} alt={attorney.name} loading="lazy" />
            <h2>{attorney.name}</h2>
            <p className="eyebrow attorney-card__role">{attorney.role}</p>
            <p className="attorney-card__bio">{attorney.shortBio}</p>
            <span className="attorney-card__action">View full profile</span>
          </Link>
        ))}
      </section>

      <section className="card marketing-cta">
        <p className="marketing-cta__kicker">Start with the right attorney</p>
        <h2>Share your goals and we will match your matter to the right legal lead.</h2>
        <Link className="hero-button hero-button--primary" to="/contact">
          Request consultation
        </Link>
      </section>
    </section>
  );
}
