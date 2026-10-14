export function AccountsPage() {
  return (
    <section className="page">
      <h1>Accounts</h1>
      <p className="lead">
        Operating and trust accounts for the firm. No account ledger is stored, so this page does not show a balance.
      </p>
      <section className="content-grid">
        <article className="card">
          <h2>Operating</h2>
          <p>No operating account is open.</p>
        </article>
        <article className="card">
          <h2>Trust</h2>
          <p>No trust account is open. Client funds are not tracked here.</p>
        </article>
      </section>
    </section>
  );
}
