import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <section className="page marketing-page">
      <section className="marketing-hero marketing-hero--not-found">
        <p className="eyebrow">404</p>
        <h1>This page stepped out of session.</h1>
        <p className="lead">The page you are looking for does not exist or may have moved.</p>
      </section>
      <section className="card marketing-cta marketing-cta--compact">
        <h2>Return to the homepage and continue browsing.</h2>
        <Link to="/" className="hero-button hero-button--primary">
          Go home
        </Link>
      </section>
    </section>
  );
}
