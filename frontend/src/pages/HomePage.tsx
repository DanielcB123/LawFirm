import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { HealthStatusCard } from "../components/HealthStatusCard";
import { teamMembers } from "../data/teamMembers";

const valueProps = [
  "Responsive communication and transparent billing.",
  "Focused experience in high-stakes civil matters.",
  "Personalized strategy tailored to your goals.",
];

const quickStats = [
  { label: "Avg. response window", value: "< 1 business day" },
  { label: "Practice model", value: "Client-first, trial-ready" },
  { label: "Consultations", value: "Same-week availability" },
];

const practiceAreaHighlights = [
  {
    title: "Business & Contract Disputes",
    body: "Strategic negotiation and litigation support to protect your company, partnerships, and future growth.",
  },
  {
    title: "Risk & Compliance Advisory",
    body: "Proactive legal guidance to reduce exposure, improve policy discipline, and prevent costly disputes.",
  },
  {
    title: "Civil Litigation",
    body: "End-to-end courtroom representation from early case strategy through trial and post-judgment enforcement.",
  },
  {
    title: "Estate & Legacy Planning",
    body: "Wills, trusts, and planning frameworks that bring clarity and confidence to major life transitions.",
  },
];

const partnerProfiles = teamMembers.filter((member) => member.role.includes("Partner"));

const processSteps = [
  "Initial consultation to understand your goals, timeline, and constraints.",
  "Case roadmap with clear options, likely outcomes, and next actions.",
  "Execution phase with active communication and transparent milestones.",
  "Resolution and long-term planning to protect what comes next.",
];

const testimonials = [
  {
    quote:
      "They gave us a strategy we could actually follow. Every next step was clear, and we always knew where we stood.",
    source: "Client - Small business owner",
  },
  {
    quote:
      "Professional, responsive, and honest about options. The process felt manageable because their team stayed ahead of every detail.",
    source: "Client - Civil litigation matter",
  },
];

export function HomePage() {
  const [scrollY, setScrollY] = useState(0);
  const [prefersReducedMotion, setPrefersReducedMotion] = useState(false);

  useEffect(() => {
    const mediaQuery = window.matchMedia("(prefers-reduced-motion: reduce)");
    const updateMotionPreference = () => setPrefersReducedMotion(mediaQuery.matches);

    updateMotionPreference();
    mediaQuery.addEventListener("change", updateMotionPreference);
    return () => mediaQuery.removeEventListener("change", updateMotionPreference);
  }, []);

  useEffect(() => {
    if (prefersReducedMotion) {
      return;
    }

    let frameId = 0;
    const handleScroll = () => {
      cancelAnimationFrame(frameId);
      frameId = requestAnimationFrame(() => setScrollY(window.scrollY));
    };

    handleScroll();
    window.addEventListener("scroll", handleScroll, { passive: true });
    return () => {
      cancelAnimationFrame(frameId);
      window.removeEventListener("scroll", handleScroll);
    };
  }, [prefersReducedMotion]);

  const heroLayerStyle = prefersReducedMotion
    ? undefined
    : { transform: `translate3d(0, ${scrollY * 0.07}px, 0)` };
  const glowLayerStyle = prefersReducedMotion
    ? undefined
    : { transform: `translate3d(0, ${scrollY * -0.04}px, 0)` };
  const heroImageStyle = prefersReducedMotion
    ? undefined
    : { transform: `translate3d(0, ${scrollY * 0.05}px, 0)` };
  const bannerLayerStyle = prefersReducedMotion
    ? undefined
    : { transform: `translate3d(0, ${scrollY * 0.03}px, 0)` };

  return (
    <section className="page home-page">
      <section className="home-hero" aria-label="Intro" data-reveal="zoom">
        <div className="home-hero__image" style={heroImageStyle} aria-hidden="true" />
        <div className="home-hero__glow" style={glowLayerStyle} aria-hidden="true" />
        <div className="home-hero__grid" style={heroLayerStyle} aria-hidden="true" />
        <p className="eyebrow home-hero__eyebrow">Trusted legal counsel in Ohio</p>
        <h1>Legal strategy that moves as fast as your life.</h1>
        <p className="lead">
          Brian & Sandra Law helps individuals, families, and small businesses resolve legal issues with practical
          strategy, consistent communication, and strong courtroom advocacy.
        </p>
        <div className="home-hero__actions">
          <Link className="hero-button hero-button--primary" to="/contact">
            Book consultation
          </Link>
          <Link className="hero-button" to="/practice-areas">
            Explore practice areas
          </Link>
        </div>
        <div className="home-hero__stats" aria-label="Firm highlights">
          {quickStats.map((stat) => (
            <article key={stat.label} className="home-stat card">
              <p className="home-stat__label">{stat.label}</p>
              <p className="home-stat__value">{stat.value}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="content-grid home-panels" aria-label="Why clients choose us and consultation details" data-reveal="left">
        <article className="card home-panel home-panel--lift" data-reveal>
          <h2>Why clients choose us</h2>
          <ul>
            {valueProps.map((item) => (
              <li key={item}>{item}</li>
            ))}
          </ul>
        </article>
        <article className="card home-panel home-panel--lift" data-reveal="right">
          <h2>Consultation hours</h2>
          <p>Monday-Friday: 8:30 AM-6:00 PM</p>
          <p>Saturday: By appointment</p>
          <p>Same-week consultations available for urgent matters.</p>
        </article>
      </section>

      <section className="home-practice-grid" aria-label="Practice area highlights" data-reveal>
        {practiceAreaHighlights.map((item) => (
          <article key={item.title} className="card home-practice-card home-panel--lift" data-reveal="zoom">
            <p className="home-practice-card__tag">Practice area</p>
            <h3>{item.title}</h3>
            <p>{item.body}</p>
          </article>
        ))}
      </section>

      <section className="home-parallax-banner" aria-label="Approach" data-reveal>
        <div className="home-parallax-banner__inner" style={bannerLayerStyle}>
          <p className="home-parallax-banner__kicker">Approach</p>
          <h2>Built for clarity, speed, and confidence at every legal milestone.</h2>
        </div>
      </section>

      <section className="home-partners card" aria-label="Partner profiles" data-reveal="zoom">
        <p className="home-partners__kicker">Partner spotlight</p>
        <h2>Meet the attorneys behind every strategy.</h2>
        <div className="home-partners__grid">
          {partnerProfiles.map((partner) => (
            <Link key={partner.slug} to={`/attorneys/${partner.slug}`} className="home-partner-card home-partner-card--link">
              <img src={partner.image} alt={partner.name} loading="lazy" />
              <div className="home-partner-card__content">
                <h3>{partner.name}</h3>
                <p className="home-partner-card__title">{partner.role}</p>
                <p>{partner.shortBio}</p>
                <span className="home-partner-card__action">Read full biography</span>
              </div>
            </Link>
          ))}
        </div>
      </section>

      <section className="home-process card" aria-label="Client process" data-reveal>
        <p className="home-process__kicker">What to expect</p>
        <h2>A simple process designed to reduce uncertainty.</h2>
        <ol className="home-process__list">
          {processSteps.map((step) => (
            <li key={step}>{step}</li>
          ))}
        </ol>
      </section>

      <section className="content-grid home-testimonials" aria-label="Client testimonials" data-reveal="left">
        {testimonials.map((item) => (
          <article key={item.quote} className="card home-testimonial home-panel--lift" data-reveal="right">
            <p className="home-testimonial__quote">"{item.quote}"</p>
            <p className="home-testimonial__source">{item.source}</p>
          </article>
        ))}
      </section>

      <article className="card home-panel home-panel--health" data-reveal>
        <HealthStatusCard />
      </article>

      <section className="card home-final-cta" aria-label="Contact call to action" data-reveal="zoom">
        <p className="home-final-cta__kicker">Ready to move forward?</p>
        <h2>Bring your questions. We will bring a plan.</h2>
        <p>
          Schedule a consultation and get practical next steps tailored to your timeline, risk profile, and legal
          goals.
        </p>
        <Link className="hero-button hero-button--primary home-final-cta__button" to="/contact">
          Start your consultation
        </Link>
      </section>
    </section>
  );
}
