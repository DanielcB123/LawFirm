import { useEffect, useState } from "react";
import type { FormEvent } from "react";
import { useAuth } from "../features/auth/AuthContext";
import { ApiClientError } from "../services/apiClient";
import { createParty, searchParties } from "../services/partiesApi";
import type { Party } from "../types/parties";

export function PartiesPage() {
  const { token } = useAuth();
  const [query, setQuery] = useState("");
  const [parties, setParties] = useState<Party[]>([]);
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
        setParties(result);
      } catch (err) {
        const message = err instanceof ApiClientError ? err.message : "Failed to load parties.";
        setError(message);
      } finally {
        setIsLoading(false);
      }
    }

    void load();
  }, [query, token]);

  async function onCreateParty(event: FormEvent<HTMLFormElement>) {
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

      setParties((existing) => [created, ...existing]);
      setDisplayName("");
      setPrimaryEmail("");
      setPrimaryPhone("");
      setIsRestricted(false);
    } catch (err) {
      const message = err instanceof ApiClientError ? err.message : "Failed to create party.";
      setError(message);
    } finally {
      setIsSaving(false);
    }
  }

  return (
    <section className="page">
      <h1>Contacts & Organizations</h1>
      <p className="lead">Create and search party records used for conflict checks and relationship graphs.</p>

      <section className="content-grid">
        <article className="card">
          <h2>Create party</h2>
          <form className="workspace-form" onSubmit={onCreateParty}>
            <label htmlFor="displayName">Display name</label>
            <input
              id="displayName"
              value={displayName}
              onChange={(event) => setDisplayName(event.target.value)}
              required
            />

            <label htmlFor="partyType">Party type</label>
            <select
              id="partyType"
              value={partyType}
              onChange={(event) => setPartyType(event.target.value as "Person" | "Organization")}
            >
              <option value="Person">Person</option>
              <option value="Organization">Organization</option>
            </select>

            <label htmlFor="primaryEmail">Primary email</label>
            <input
              id="primaryEmail"
              type="email"
              value={primaryEmail}
              onChange={(event) => setPrimaryEmail(event.target.value)}
            />

            <label htmlFor="primaryPhone">Primary phone</label>
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
              {isSaving ? "Saving..." : "Create party"}
            </button>
          </form>
        </article>

        <article className="card">
          <h2>Search</h2>
          <label htmlFor="search">Name, email, or alias</label>
          <input id="search" value={query} onChange={(event) => setQuery(event.target.value)} />
          {isLoading && <p>Loading parties...</p>}
          {error && <p className="error-text">{error}</p>}

          {!isLoading && parties.length === 0 && <p>No parties found.</p>}

          <ul className="list-reset">
            {parties.map((party) => (
              <li key={party.id} className="party-row">
                <p>
                  <strong>{party.displayName}</strong> ({party.partyType})
                  {party.isRestricted && " · Restricted"}
                </p>
                {party.aliases.length > 0 && (
                  <p className="muted">
                    Aliases: {party.aliases.map((alias) => `${alias.alias} (${alias.aliasType})`).join(", ")}
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
