export function SiteFooter() {
  const year = new Date().getFullYear();

  return (
    <footer className="site-footer border-t border-[var(--border)] bg-[color-mix(in_srgb,var(--bg)_92%,#fff4e8_8%)]">
      <div className="site-footer__inner mx-auto box-border w-full max-w-[1120px] px-4 pb-6 pt-4">
        <p>Brian & Sandra Law | Client-focused representation for families and businesses.</p>
        <p>123 Justice Avenue, Suite 400, Columbus, OH 43215 | (614) 555-0134</p>
        <p>{year} Brian & Sandra Law. All rights reserved.</p>
      </div>
    </footer>
  );
}
