export function BillingPage() {
  return (
    <section className="page">
      <h1>Billing</h1>
      <p className="lead">Time, expenses, and invoices for client matters. None of those records are stored yet.</p>
      <section className="content-grid">
        <article className="card">
          <h2>Time</h2>
          <p>No time entries.</p>
        </article>
        <article className="card">
          <h2>Expenses</h2>
          <p>No expenses.</p>
        </article>
        <article className="card">
          <h2>Invoices</h2>
          <p>No invoices. Nothing on this page can create a bill or collect a payment.</p>
        </article>
      </section>
    </section>
  );
}
