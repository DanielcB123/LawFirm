import { Navigate, Route, Routes } from "react-router-dom";
import { RequireAuth } from "../features/auth";
import { NotFoundPage } from "../pages/NotFoundPage";
import { AboutPage } from "../pages/public/AboutPage";
import { AttorneyProfilePage } from "../pages/public/AttorneyProfilePage";
import { AttorneysPage } from "../pages/public/AttorneysPage";
import { ContactPage } from "../pages/public/ContactPage";
import { HomePage } from "../pages/public/HomePage";
import { PracticeAreasPage } from "../pages/public/PracticeAreasPage";
import { ConflictChecksPage } from "../pages/workspace/ConflictChecksPage";
import { IntakeBoardPage } from "../pages/workspace/IntakeBoardPage";
import { IntakeDetailPage } from "../pages/workspace/IntakeDetailPage";
import { MattersPage } from "../pages/workspace/MattersPage";
import { MatterWorkspacePage } from "../pages/workspace/MatterWorkspacePage";
import {
  MatterDocumentsTab,
  MatterOverviewTab,
  MatterTasksTab,
  MatterTimelineTab,
} from "../pages/workspace/MatterWorkspaceTabs";
import { PartiesPage } from "../pages/workspace/PartiesPage";
import { TasksCalendarPage } from "../pages/workspace/TasksCalendarPage";
import { WorkspaceConsultationsPage } from "../pages/workspace/WorkspaceConsultationsPage";
import { WorkspaceHomePage } from "../pages/workspace/WorkspaceHomePage";
import { WorkspaceLoginPage } from "../pages/workspace/WorkspaceLoginPage";
import { WorkspaceWorklistsPage } from "../pages/workspace/WorkspaceWorklistsPage";

export function AppRoutes() {
  return (
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
  );
}
