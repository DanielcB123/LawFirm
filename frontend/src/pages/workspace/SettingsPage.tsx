import { useAuth } from "../../features/auth";

export function SettingsPage() {
  const { actor } = useAuth();

  return (
    <section className="page">
      <h1>Settings</h1>
      <p className="lead">The signed-in profile for this workspace. These details come from the current session.</p>
      {!actor && <p>Loading profile...</p>}
      {actor && (
        <article className="card">
          <h2>{actor.displayName}</h2>
          <p>{actor.email}</p>
          <p className="muted">Role: {actor.role}</p>
          <h3>Permissions</h3>
          {actor.permissions.length === 0 && <p>No permissions on this profile.</p>}
          {actor.permissions.length > 0 && (
            <ul>
              {actor.permissions.map((permission) => (
                <li key={permission}>{permission}</li>
              ))}
            </ul>
          )}
        </article>
      )}
    </section>
  );
}
