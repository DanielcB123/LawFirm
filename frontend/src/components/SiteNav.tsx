import { useMemo, useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";

const publicNavLinks = [
  { label: "Home", path: "/" },
  { label: "About", path: "/about" },
  { label: "Practice Areas", path: "/practice-areas" },
  { label: "Attorneys", path: "/attorneys" },
  { label: "Contact", path: "/contact" },
];

const workspaceNavLinks = [
  { label: "Workspace", path: "/workspace" },
  { label: "Parties", path: "/workspace/parties", requiredPermission: "contacts.read" },
  { label: "Conflicts", path: "/workspace/conflicts", requiredPermission: "conflicts.search" },
  { label: "Tasks & Calendar", path: "/workspace/calendar", requiredPermission: "calendar.read" },
  { label: "Intake", path: "/workspace/intake", requiredPermission: "intake.read" },
  { label: "Matters", path: "/workspace/matters", requiredPermissions: ["matters.read.assigned", "matters.read.all"] },
];
type WorkspaceNavLink = (typeof workspaceNavLinks)[number] & {
  requiredPermissions?: string[];
};

export function SiteNav() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const { token, actor, logout } = useAuth();
  const navLinks = useMemo(() => {
    if (!token) {
      return publicNavLinks;
    }

    const grantedPermissions = new Set(actor?.permissions ?? []);
    return workspaceNavLinks.filter((link: WorkspaceNavLink) => {
      if (!link.requiredPermission && !link.requiredPermissions) {
        return true;
      }

      if (link.requiredPermission) {
        return grantedPermissions.has(link.requiredPermission);
      }

      return (link.requiredPermissions ?? []).some((permission) => grantedPermissions.has(permission));
    });
  }, [actor?.permissions, token]);

  return (
    <header className="site-header">
      <div className="site-header__inner">
        <NavLink to="/" className="site-brand" onClick={() => setIsMenuOpen(false)}>
          Brian & Sandra Law
        </NavLink>

        <button
          type="button"
          className="site-menu-toggle"
          aria-expanded={isMenuOpen}
          aria-controls="primary-navigation"
          onClick={() => setIsMenuOpen((current) => !current)}
        >
          <span className="sr-only">Toggle navigation menu</span>
          <span aria-hidden="true">{isMenuOpen ? "Close" : "Menu"}</span>
        </button>

        <nav
          id="primary-navigation"
          className={`site-nav ${isMenuOpen ? "site-nav--open" : ""}`}
          aria-label="Primary"
        >
          {navLinks.map((link) => (
            <NavLink
              key={link.path}
              to={link.path}
              end={link.path === "/" || link.path === "/workspace"}
              className={({ isActive }) => `site-nav__link ${isActive ? "site-nav__link--active" : ""}`}
              onClick={() => setIsMenuOpen(false)}
            >
              {link.label}
            </NavLink>
          ))}
          {token ? (
            <button
              type="button"
              className="site-nav__button"
              onClick={() => {
                logout();
                setIsMenuOpen(false);
              }}
            >
              Sign out
            </button>
          ) : (
            <NavLink
              to="/workspace/login"
              className={({ isActive }) => `site-nav__link ${isActive ? "site-nav__link--active" : ""}`}
              onClick={() => setIsMenuOpen(false)}
            >
              Sign in
            </NavLink>
          )}
        </nav>
      </div>
    </header>
  );
}
