import { useEffect } from "react";
import { useLocation } from "react-router-dom";
import { SiteFooter } from "../components/layout/SiteFooter";
import { SiteNav } from "../components/layout/SiteNav";
import { useAuth } from "../features/auth";
import { AppRoutes } from "./AppRoutes";
import "./AppShell.css";

export function AppShell() {
  const { token } = useAuth();
  const location = useLocation();

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

  return (
    <div className={`app-shell ${token ? "app-shell--authenticated" : ""}`}>
      <SiteNav />
      <main className="app-content">
        <AppRoutes />
      </main>
      <SiteFooter />
    </div>
  );
}
