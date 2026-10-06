export function SiteFooter() {
  const year = new Date().getFullYear();

  return (
    <footer className="site-footer">
      <div className="site-footer__inner">
        <p>Brian & Sandra Law | Client-focused representation for families and businesses.</p>
        <p>123 Justice Avenue, Suite 400, Columbus, OH 43215 | (614) 555-0134</p>
        <p>{year} Brian & Sandra Law. All rights reserved.</p>
      </div>
    </footer>
  );
}
