import { useMemo, useState } from "react";
import { NavLink } from "react-router-dom";
import { useAuth } from "../../features/auth";
import type { ReactNode } from "react";

const publicNavLinks = [
  { label: "Home", path: "/" },
  { label: "About", path: "/about" },
  { label: "Practice Areas", path: "/practice-areas" },
  { label: "Attorneys", path: "/attorneys" },
  { label: "Contact", path: "/contact" },
];

const matterReadPermissions = ["matters.read.assigned", "matters.read.all"];

const primaryNavLinks = [
  { label: "Dashboard", path: "/workspace" },
  { label: "Calendar", path: "/workspace/calendar", requiredPermission: "calendar.read" },
  { label: "Tasks", path: "/workspace/worklists", requiredPermission: "calendar.read" },
  { label: "Matters", path: "/workspace/matters", requiredPermissions: matterReadPermissions },
  { label: "Contacts", path: "/workspace/contacts", requiredPermission: "contacts.read" },
  { label: "Activities", path: "/workspace/activities", requiredPermissions: matterReadPermissions },
  { label: "Billing", path: "/workspace/billing", requiredPermissions: ["billing.manage", "billing.read.own"] },
  { label: "Accounts", path: "/workspace/accounts", requiredPermission: "billing.manage" },
  { label: "Documents", path: "/workspace/documents", requiredPermissions: matterReadPermissions },
  { label: "Communications", path: "/workspace/communications", requiredPermission: "intake.read" },
  { label: "Reports", path: "/workspace/reports" },
  { label: "Integrations", path: "/workspace/integrations" },
  { label: "Settings", path: "/workspace/settings" },
];

const practiceNavLinks = [
  { label: "Intake", path: "/workspace/intake", requiredPermission: "intake.read" },
  { label: "Consultations", path: "/workspace/consultations", requiredPermission: "intake.read" },
  { label: "Conflicts", path: "/workspace/conflicts", requiredPermission: "conflicts.search" },
];

type WorkspaceNavLink = (typeof primaryNavLinks)[number] & {
  requiredPermissions?: string[];
};

function initials(name: string): string {
  const letters = name
    .split(/\s+/)
    .filter((part) => part.length > 0)
    .slice(0, 2)
    .map((part) => part[0]?.toUpperCase() ?? "");

  return letters.join("") || "•";
}

function navIcon(path: string): ReactNode {
  if (path === "/workspace") {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-5v-6h-4v6H5a1 1 0 0 1-1-1z" />
      </svg>
    );
  }
  if (path.includes("/workspace/contacts")) {
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
  if (path.includes("/workspace/worklists")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M8 6h12M8 12h12M8 18h12M4 6h.01M4 12h.01M4 18h.01" />
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
  if (path.includes("/workspace/activities")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M12 8v5l3 2M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18" />
      </svg>
    );
  }
  if (path.includes("/workspace/billing")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M7 3h10v18l-2.5-2-2.5 2-2.5-2L7 21zM9 8h6M9 12h6" />
      </svg>
    );
  }
  if (path.includes("/workspace/accounts")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M4 10h16M6 10V8l6-4 6 4v2M6 10v8M10 10v8M14 10v8M18 10v8M4 18h16" />
      </svg>
    );
  }
  if (path.includes("/workspace/documents")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M7 3h7l5 5v13H7zM14 3v5h5M9 13h6M9 17h6" />
      </svg>
    );
  }
  if (path.includes("/workspace/communications")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M6 4h4l2 4-3 2a12 12 0 0 0 5 5l2-3 4 2v4a2 2 0 0 1-2 2A16 16 0 0 1 4 6a2 2 0 0 1 2-2" />
      </svg>
    );
  }
  if (path.includes("/workspace/reports")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M4 19V5M4 19h16M8 16v-5M12 16V8M16 16v-3" />
      </svg>
    );
  }
  if (path.includes("/workspace/integrations")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M4 8h6v6H4zM14 4h6v6h-6zM14 14h6v6h-6zM10 11h4" />
      </svg>
    );
  }
  if (path.includes("/workspace/settings")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M12 15a3 3 0 1 0 0-6 3 3 0 0 0 0 6M12 3v2M12 19v2M4.9 6.5l1.5 1.5M17.6 16l1.5 1.5M3 12h2M19 12h2M4.9 17.5l1.5-1.5M17.6 8l1.5-1.5" />
      </svg>
    );
  }
  if (path.includes("/workspace/resource-centre")) {
    return (
      <svg viewBox="0 0 24 24" aria-hidden="true">
        <path d="M12 17h.01M9.5 9a2.5 2.5 0 1 1 4.2 1.8c-.7.6-1.2 1-1.2 2.2M12 21a9 9 0 1 0 0-18 9 9 0 0 0 0 18" />
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
  const grantedPermissions = useMemo(() => new Set(actor?.permissions ?? []), [actor?.permissions]);
  const canSeeWorkspaceLink = (link: WorkspaceNavLink) => {
    if (!link.requiredPermission && !link.requiredPermissions) {
      return true;
    }

    if (link.requiredPermission) {
      return grantedPermissions.has(link.requiredPermission);
    }

    return (link.requiredPermissions ?? []).some((permission) => grantedPermissions.has(permission));
  };
  const navLinks = token ? [] : publicNavLinks;
  const primaryLinks = token ? primaryNavLinks.filter(canSeeWorkspaceLink) : [];
  const practiceLinks = token ? practiceNavLinks.filter(canSeeWorkspaceLink) : [];

  if (token) {
    return (
      <aside
        className={`site-sidebar border-r border-[var(--border)] bg-[color-mix(in_srgb,var(--bg)_96%,#fff4e8_4%)] ${isSidebarCollapsed ? "site-sidebar--collapsed" : ""}`}
        aria-label="Workspace navigation"
      >
        <div className="site-sidebar__top">
          <NavLink to="/workspace" className="site-sidebar__brand">
            <span className="site-sidebar__brand-name">Brian & Sandra Law</span>
            <span className="site-sidebar__brand-short">BSL</span>
          </NavLink>
        </div>

        <nav className="site-sidebar__nav" aria-label="Primary">
          {[primaryLinks, practiceLinks].map((group, index) => (
            <div key={index === 0 ? "primary" : "practice"} className="site-sidebar__group">
              {index === 1 && group.length > 0 && <p className="site-sidebar__group-label">Practice</p>}
              {group.map((link) => (
                <NavLink
                  key={link.path}
                  to={link.path}
                  end={link.path === "/workspace"}
                  className={({ isActive }) => `site-sidebar__link ${isActive ? "site-sidebar__link--active" : ""}`}
                >
                  <span className="site-sidebar__icon">{navIcon(link.path)}</span>
                  <span className="site-sidebar__label">{link.label}</span>
                </NavLink>
              ))}
            </div>
          ))}
        </nav>

        <div className="site-sidebar__footer border-t border-[var(--border)]">
          <NavLink
            to="/workspace/resource-centre"
            className={({ isActive }) => `site-sidebar__link ${isActive ? "site-sidebar__link--active" : ""}`}
          >
            <span className="site-sidebar__icon">{navIcon("/workspace/resource-centre")}</span>
            <span className="site-sidebar__label">Resource centre</span>
          </NavLink>
          {actor && (
            <div className="site-sidebar__identity">
              <span className="site-sidebar__avatar" aria-hidden="true">
                {initials(actor.displayName)}
              </span>
              <span className="site-sidebar__identity-copy">
                <span className="site-sidebar__identity-name">{actor.displayName}</span>
                <span className="site-sidebar__identity-meta">{actor.role}</span>
              </span>
            </div>
          )}
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
          <button
            type="button"
            className="site-sidebar__signout"
            onClick={() => setIsSidebarCollapsed((current) => !current)}
            aria-expanded={!isSidebarCollapsed}
            aria-label={isSidebarCollapsed ? "Expand sidebar" : "Collapse sidebar"}
          >
            <span className="site-sidebar__icon" aria-hidden="true">
              {isSidebarCollapsed ? "→" : "←"}
            </span>
            <span className="site-sidebar__label">{isSidebarCollapsed ? "Expand" : "Collapse"}</span>
          </button>
        </div>
      </aside>
    );
  }

  return (
    <header className="site-header border-b border-[var(--border)]">
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
