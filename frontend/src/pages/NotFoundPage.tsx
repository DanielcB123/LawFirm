import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <section className="page marketing-page">
      <section className="marketing-hero marketing-hero--not-found" data-reveal="zoom">
        <p className="eyebrow">404</p>
        <h1>This page stepped out of session.</h1>
        <p className="lead">The page you are looking for does not exist or may have moved.</p>
      </section>
      <section className="content-grid marketing-grid" data-reveal>
        <article className="card marketing-card" data-reveal="left">
          <h2>Try one of these pages</h2>
          <ul>
            <li>Home for firm overview and consultation links.</li>
            <li>Practice Areas for services and legal focus.</li>
            <li>Attorneys for partner and team profiles.</li>
          </ul>
        </article>
        <article className="card marketing-card" data-reveal="right">
          <h2>Need immediate guidance?</h2>
          <p>
            If your matter has a deadline, contact us directly and include your timeline so we can prioritize urgent
            follow-up.
          </p>
        </article>
      </section>
      <section className="card marketing-cta marketing-cta--compact" data-reveal>
        <h2>Return to the homepage and continue browsing.</h2>
        <Link to="/" className="hero-button hero-button--primary">
          Go home
        </Link>
      </section>
    </section>
  );
}
