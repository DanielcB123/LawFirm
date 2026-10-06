import { Link } from "react-router-dom";

export function NotFoundPage() {
  return (
    <section className="page">
      <h1>Page Not Found</h1>
      <p className="lead">The page you are looking for does not exist or may have moved.</p>
      <p>
        Return to the <Link to="/">home page</Link>.
      </p>
    </section>
  );
}
