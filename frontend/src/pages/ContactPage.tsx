export function ContactPage() {
  return (
    <section className="page">
      <h1>Contact Us</h1>
      <p className="lead">
        Request a consultation to discuss your matter. We respond to all inquiries within one business
        day.
      </p>

      <section className="content-grid">
        <article className="card">
          <h2>Office</h2>
          <p>123 Justice Avenue, Suite 400</p>
          <p>Columbus, OH 43215</p>
        </article>
        <article className="card">
          <h2>Phone & Email</h2>
          <p>(614) 555-0134</p>
          <p>intake@parkerreedlaw.com</p>
        </article>
        <article className="card">
          <h2>Hours</h2>
          <p>Monday-Friday: 8:30 AM-6:00 PM</p>
          <p>Saturday: By appointment</p>
          <p>Sunday: Closed</p>
        </article>
      </section>
    </section>
  );
}
