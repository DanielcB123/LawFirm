import { useEffect } from "react";
import { useLocation } from "react-router-dom";
import { SiteFooter } from "../components/layout/SiteFooter";
import { SiteNav } from "../components/layout/SiteNav";
import { useAuth } from "../features/auth";
import { AppRoutes } from "./AppRoutes";
import { appBackgroundThemes } from "./backgroundThemes";
import "./AppShell.css";

const backgroundPreferenceKey = "matterforge.app.background-theme";
const pinnedBackgroundThemeId = "city-skyline-night";

export function AppShell() {
  const { token } = useAuth();
  const location = useLocation();
  const activeBackgroundTheme =
    appBackgroundThemes.find((theme) => theme.id === pinnedBackgroundThemeId) ?? appBackgroundThemes[0];

  useEffect(() => {
    if (token) {
      return;
    }

    const elements = Array.from(document.querySelectorAll<HTMLElement>("[data-reveal]"));
    if (elements.length === 0) {
      return;
    }

    const observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            entry.target.classList.add("is-revealed");
            observer.unobserve(entry.target);
          }
        }
      },
      {
        threshold: 0.18,
        rootMargin: "0px 0px -8% 0px",
      },
    );

    for (const element of elements) {
      observer.observe(element);
    }

    return () => observer.disconnect();
  }, [location.pathname, token]);

  useEffect(() => {
    if (!activeBackgroundTheme) {
      return;
    }

    document.documentElement.style.setProperty("--app-shell-background-image", `url("${activeBackgroundTheme.imageUrl}")`);
    window.localStorage.setItem(backgroundPreferenceKey, activeBackgroundTheme.id);
  }, [activeBackgroundTheme]);

  return (
    <div className={`app-shell ${token ? "app-shell--authenticated" : ""}`}>
      <SiteNav />
      <main className="app-content">
        <AppRoutes />
      </main>
      <SiteFooter />
      {/* Background picker temporarily disabled while we gather visual feedback with the
          City Skyline at Night background pinned for all pages.
      <section className={`background-picker ${isBackgroundPickerOpen ? "background-picker--open" : ""}`} aria-label="Background image options">
        <button
          type="button"
          className="background-picker__toggle"
          aria-expanded={isBackgroundPickerOpen}
          onClick={() => setIsBackgroundPickerOpen((current) => !current)}
        >
          Background
        </button>
        {isBackgroundPickerOpen && (
          <div className="background-picker__panel" role="group" aria-label="Choose app background">
            {appBackgroundThemes.map((theme) => (
              <button
                key={theme.id}
                type="button"
                className={`background-picker__option ${theme.id === activeBackgroundTheme?.id ? "background-picker__option--active" : ""}`}
                onClick={() => setBackgroundThemeId(theme.id)}
              >
                <span
                  className="background-picker__swatch"
                  style={{
                    backgroundImage: `linear-gradient(rgba(250, 243, 232, 0.45), rgba(250, 243, 232, 0.6)), url("${theme.imageUrl}")`,
                  }}
                  aria-hidden="true"
                />
                <span>{theme.name}</span>
              </button>
            ))}
          </div>
        )}
      </section>
      */}
    </div>
  );
}
