import { Link, useParams } from "react-router-dom";
import { teamMemberBySlug } from "../data/teamMembers";

export function AttorneyProfilePage() {
  const { slug } = useParams<{ slug: string }>();
  const member = slug ? teamMemberBySlug[slug] : undefined;

  if (!member) {
    return (
      <section className="page marketing-page">
        <section className="marketing-hero marketing-hero--not-found" data-reveal="zoom">
          <p className="eyebrow">Team member</p>
          <h1>Profile not found.</h1>
          <p className="lead">The profile you are looking for may have moved or is not published yet.</p>
        </section>
        <section className="card marketing-cta marketing-cta--compact" data-reveal>
          <h2>Return to our attorneys list.</h2>
          <Link className="hero-button hero-button--primary" to="/attorneys">
            View attorneys
          </Link>
        </section>
      </section>
    );
  }

  return (
    <section className="page marketing-page team-profile-page">
      <section className="team-profile-hero card" data-reveal="zoom">
        <img src={member.image} alt={member.name} loading="lazy" />
        <div className="team-profile-hero__content">
          <p className="eyebrow">Attorney profile</p>
          <h1>{member.name}</h1>
          <p className="team-profile-hero__role">{member.role}</p>
          <p className="lead">{member.shortBio}</p>
          <Link className="hero-button" to="/contact">
            Schedule consultation
          </Link>
        </div>
      </section>

      <section className="content-grid team-profile-grid" data-reveal="left">
        <article className="card marketing-card" data-reveal="left">
          <h2>Personal history</h2>
          <ul>
            {member.personalHistory.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
        <article className="card marketing-card" data-reveal="right">
          <h2>Professional history</h2>
          <ul>
            {member.professionalHistory.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
      </section>

      <section className="content-grid team-profile-grid" data-reveal="zoom">
        <article className="card marketing-card" data-reveal="left">
          <h2>Education</h2>
          <ul>
            {(member.education ?? []).map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
        <article className="card marketing-card" data-reveal="right">
          <h2>Bar admissions</h2>
          <ul>
            {(member.admissions ?? []).map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
      </section>

      <section className="card marketing-cta" data-reveal="zoom">
        <p className="marketing-cta__kicker">Need legal support?</p>
        <h2>Speak directly with our team about your matter.</h2>
        <div className="team-profile-cta__actions">
          <Link className="hero-button hero-button--primary" to="/contact">
            Contact the firm
          </Link>
          <Link className="hero-button" to="/attorneys">
            Back to attorneys
          </Link>
        </div>
      </section>
    </section>
  );
}
