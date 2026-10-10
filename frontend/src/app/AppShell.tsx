import { useEffect } from "react";
import { Navigate, Route, Routes, useLocation } from "react-router-dom";
import { SiteFooter } from "../components/SiteFooter";
import { SiteNav } from "../components/SiteNav";
import { useAuth } from "../features/auth/AuthContext";
import { RequireAuth } from "../features/auth/RequireAuth";
import { AboutPage } from "../pages/AboutPage";
import { AttorneyProfilePage } from "../pages/AttorneyProfilePage";
import { AttorneysPage } from "../pages/AttorneysPage";
import { ConflictChecksPage } from "../pages/ConflictChecksPage";
import { ContactPage } from "../pages/ContactPage";
import { HomePage } from "../pages/HomePage";
import { IntakeBoardPage } from "../pages/IntakeBoardPage";
import { IntakeDetailPage } from "../pages/IntakeDetailPage";
import { MattersPage } from "../pages/MattersPage";
import { MatterWorkspacePage } from "../pages/MatterWorkspacePage";
import {
  MatterDocumentsTab,
  MatterOverviewTab,
  MatterTasksTab,
  MatterTimelineTab,
} from "../pages/MatterWorkspaceTabs";
import { NotFoundPage } from "../pages/NotFoundPage";
import { PartiesPage } from "../pages/PartiesPage";
import { PracticeAreasPage } from "../pages/PracticeAreasPage";
import { TasksCalendarPage } from "../pages/TasksCalendarPage";
import { WorkspaceConsultationsPage } from "../pages/WorkspaceConsultationsPage";
import { WorkspaceHomePage } from "../pages/WorkspaceHomePage";
import { WorkspaceLoginPage } from "../pages/WorkspaceLoginPage";
import { WorkspaceWorklistsPage } from "../pages/WorkspaceWorklistsPage";
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
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/about" element={<AboutPage />} />
          <Route path="/practice-areas" element={<PracticeAreasPage />} />
          <Route path="/attorneys" element={<AttorneysPage />} />
          <Route path="/attorneys/:slug" element={<AttorneyProfilePage />} />
          <Route path="/contact" element={<ContactPage />} />
          <Route path="/workspace/login" element={<WorkspaceLoginPage />} />
          <Route path="/workspace" element={<RequireAuth />}>
            <Route index element={<WorkspaceHomePage />} />
            <Route path="parties" element={<PartiesPage />} />
            <Route path="conflicts" element={<ConflictChecksPage />} />
            <Route path="calendar" element={<TasksCalendarPage />} />
            <Route path="consultations" element={<WorkspaceConsultationsPage />} />
            <Route path="worklists" element={<WorkspaceWorklistsPage />} />
            <Route path="intake" element={<IntakeBoardPage />} />
            <Route path="intake/:intakeId" element={<IntakeDetailPage />} />
            <Route path="matters" element={<MattersPage />} />
            <Route path="matters/:matterId" element={<MatterWorkspacePage />}>
              <Route index element={<Navigate to="overview" replace />} />
              <Route path="overview" element={<MatterOverviewTab />} />
              <Route path="tasks" element={<MatterTasksTab />} />
              <Route path="documents" element={<MatterDocumentsTab />} />
              <Route path="timeline" element={<MatterTimelineTab />} />
            </Route>
          </Route>
          <Route path="*" element={<NotFoundPage />} />
        </Routes>
      </main>
      <SiteFooter />
    </div>
  );
}
