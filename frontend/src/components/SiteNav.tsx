import { useMemo, useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../features/auth/AuthContext";
import type { ReactNode } from "react";

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
  { label: "Consultations", path: "/workspace/consultations", requiredPermission: "intake.read" },
  { label: "Intake", path: "/workspace/intake", requiredPermission: "intake.read" },
  { label: "Matters", path: "/workspace/matters", requiredPermissions: ["matters.read.assigned", "matters.read.all"] },
];
type WorkspaceNavLink = (typeof workspaceNavLinks)[number] & {
  requiredPermissions?: string[];
};

function navIcon(path: string): ReactNode {
  if (path === "/workspace") {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-5v-6h-4v6H5a1 1 0 0 1-1-1z" />
      </svg>
    );
  }
  if (path.includes("/workspace/parties")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M8 12a3 3 0 1 1 0-6 3 3 0 0 1 0 6m8 1a3 3 0 1 1 0-6 3 3 0 0 1 0 6M3 20a5 5 0 0 1 10 0m3 0a5 5 0 0 1 5-4.8" />
      </svg>
    );
  }
  if (path.includes("/workspace/conflicts")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M12 5v14M7 8h10M5 19h14M9 8l-3 5h6Zm9 0-3 5h6Z" />
      </svg>
    );
  }
  if (path.includes("/workspace/calendar")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M7 3v3m10-3v3M4 9h16M5 6h14a1 1 0 0 1 1 1v12a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V7a1 1 0 0 1 1-1" />
      </svg>
    );
  }
  if (path.includes("/workspace/consultations")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M4 6h16a1 1 0 0 1 1 1v10a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2V7a1 1 0 0 1 1-1m0 1 8 6 8-6" />
      </svg>
    );
  }
  if (path.includes("/workspace/intake")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M7 4h10a2 2 0 0 1 2 2v13l-3-2-3 2-3-2-3 2V6a2 2 0 0 1 2-2m2 4h6m-6 4h6" />
      </svg>
    );
  }
  if (path.includes("/workspace/matters")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M3 7a2 2 0 0 1 2-2h4l2 2h8a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />
      </svg>
    );
  }
  if (path === "/signout") {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M10 4H6a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h4m4-12 6 4-6 4m6-4H9" />
      </svg>
    );
  }

  return (
    <svg viewBox="0 0 24 24" aria-hidden="true">
      <path d="M12 12h.01" />
    </svg>
  );
}

export function SiteNav() {
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false);
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

  if (token) {
    return (
      <aside className={`site-sidebar ${isSidebarCollapsed ? "site-sidebar--collapsed" : ""}`} aria-label="Workspace navigation">
        <div className="site-sidebar__top">
          <button
            type="button"
            className="site-sidebar__toggle"
            onClick={() => setIsSidebarCollapsed((current) => !current)}
            aria-expanded={!isSidebarCollapsed}
            aria-label={isSidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
          >
            {isSidebarCollapsed ? "→" : "←"}
          </button>
          <NavLink to="/workspace" className="site-sidebar__brand">
            <span className="site-sidebar__brand-name">Brian & Sandra Law</span>
            <span className="site-sidebar__brand-short">BSL</span>
          </NavLink>
        </div>

        <nav className="site-sidebar__nav" aria-label="Primary">
          {navLinks.map((link) => (
            <NavLink
              key={link.path}
              to={link.path}
              end={link.path === "/" || link.path === "/workspace"}
              className={({ isActive }) => `site-sidebar__link ${isActive ? "site-sidebar__link--active" : ""}`}
            >
              <span className="site-sidebar__icon">{navIcon(link.path)}</span>
              <span className="site-sidebar__label">{link.label}</span>
            </NavLink>
          ))}
        </nav>

        <div className="site-sidebar__footer">
          <button
            type="button"
            className="site-sidebar__signout"
            onClick={() => {
              logout();
              setIsMenuOpen(false);
            }}
          >
            <span className="site-sidebar__icon">{navIcon("/signout")}</span>
            <span className="site-sidebar__label">Sign out</span>
          </button>
        </div>
      </aside>
    );
  }

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
