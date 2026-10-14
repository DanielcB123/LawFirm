import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "../../features/auth";
import { ApiClientError } from "../../lib/apiClient";
import { createParty, searchParties } from "../../features/parties";
import type { Party } from "../../features/parties";

export function ContactsPage() {
  const { token } = useAuth();
  const [query, setQuery] = useState("");
  const [contacts, setContacts] = useState<Party[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const [displayName, setDisplayName] = useState("");
  const [partyType, setPartyType] = useState<"Person" | "Organization">("Person");
  const [primaryEmail, setPrimaryEmail] = useState("");
  const [primaryPhone, setPrimaryPhone] = useState("");
  const [isRestricted, setIsRestricted] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  useEffect(() => {
    async function load() {
      if (!token) {
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        const result = await searchParties(token, query);
        setContacts(result);
      } catch (err) {
        const message = err instanceof ApiClientError ? err.message : "Failed to load contacts.";
        setError(message);
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [query, token]);

  async function onCreateContact(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!token) {
      return;
    }

    setIsSaving(true);
    setError(null);

    try {
      const created = await createParty(token, {
        displayName,
        partyType,
        primaryEmail: primaryEmail || undefined,
        primaryPhone: primaryPhone || undefined,
        isRestricted,
      });

      setContacts((existing) => [created, ...existing]);
      setDisplayName("");
      setPrimaryEmail("");
      setPrimaryPhone("");
      setIsRestricted(false);
    } catch (err) {
      const message = err instanceof ApiClientError ? err.message : "Failed to add contact.";
      setError(message);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <section className="page">
      <h1>Contacts</h1>
      <p className="lead">
        People and organizations the firm works with, including clients, opposing parties, and related companies.
      </p>

      <section className="content-grid">
        <article className="card">
          <h2>Add contact</h2>
          <form className="workspace-form" onSubmit={onCreateContact}>
            <label htmlFor="displayName">Name</label>
            <input
              id="displayName"
              value={displayName}
              onChange={(event) => setDisplayName(event.target.value)}
              required
            />

            <label htmlFor="partyType">Type</label>
            <select
              id="partyType"
              value={partyType}
              onChange={(event) => setPartyType(event.target.value as "Person" | "Organization")}
            >
              <option value="Person">Person</option>
              <option value="Organization">Organization</option>
            </select>

            <label htmlFor="primaryEmail">Email</label>
            <input
              id="primaryEmail"
              type="email"
              value={primaryEmail}
              onChange={(event) => setPrimaryEmail(event.target.value)}
            />

            <label htmlFor="primaryPhone">Phone</label>
            <input
              id="primaryPhone"
              value={primaryPhone}
              onChange={(event) => setPrimaryPhone(event.target.value)}
            />

            <label className="checkbox-field">
              <input
                type="checkbox"
                checked={isRestricted}
                onChange={(event) => setIsRestricted(event.target.checked)}
              />
              Restricted visibility
            </label>

            <button type="submit" disabled={isSaving}>
              {isSaving ? "Saving..." : "Add contact"}
            </button>
          </form>
        </article>

        <article className="card">
          <h2>Directory</h2>
          <div className="workspace-form">
            <label htmlFor="search">Search by name, email, or alias</label>
            <input id="search" value={query} onChange={(event) => setQuery(event.target.value)} />
          </div>
          {isLoading && <p>Loading contacts...</p>}
          {error && <p className="error-text">{error}</p>}

          {!isLoading && contacts.length === 0 && <p>No contacts found.</p>}
          {!isLoading && contacts.length > 0 && (
            <p className="muted">
              {contacts.length} {contacts.length === 1 ? "contact" : "contacts"}
            </p>
          )}

          <ul className="list-reset">
            {contacts.map((contact) => (
              <li key={contact.id} className="contact-row">
                <div className="contact-row__main">
                  <strong>{contact.displayName}</strong>
                  <span className="contact-row__type">{contact.partyType === "Person" ? "Person" : "Organization"}</span>
                  {contact.isRestricted && <span className="contact-row__badge">Restricted</span>}
                </div>
                <p className="muted">
                  {contact.primaryEmail ?? "No email"} · {contact.primaryPhone ?? "No phone"}
                </p>
                {contact.aliases.length > 0 && (
                  <p className="muted">
                    Also known as {contact.aliases.map((alias) => alias.alias).join(", ")}
                  </p>
                )}
              </li>
            ))}
          </ul>
        </article>
      </section>
    </section>
  );
}
