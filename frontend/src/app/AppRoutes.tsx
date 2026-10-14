import { Navigate, Route, Routes } from "react-router-dom";
import { RequireAuth } from "../features/auth";
import { NotFoundPage } from "../pages/NotFoundPage";
import { AboutPage } from "../pages/public/AboutPage";
import { AttorneyProfilePage } from "../pages/public/AttorneyProfilePage";
import { AttorneysPage } from "../pages/public/AttorneysPage";
import { ContactPage } from "../pages/public/ContactPage";
import { HomePage } from "../pages/public/HomePage";
import { PracticeAreasPage } from "../pages/public/PracticeAreasPage";
import { AccountsPage } from "../pages/workspace/AccountsPage";
import { ActivitiesPage } from "../pages/workspace/ActivitiesPage";
import { BillingPage } from "../pages/workspace/BillingPage";
import { CommunicationsPage } from "../pages/workspace/CommunicationsPage";
import { ConflictChecksPage } from "../pages/workspace/ConflictChecksPage";
import { DocumentsPage } from "../pages/workspace/DocumentsPage";
import { IntakeBoardPage } from "../pages/workspace/IntakeBoardPage";
import { IntakeDetailPage } from "../pages/workspace/IntakeDetailPage";
import { IntegrationsPage } from "../pages/workspace/IntegrationsPage";
import { ReportsPage } from "../pages/workspace/ReportsPage";
import { ResourceCentrePage } from "../pages/workspace/ResourceCentrePage";
import { SettingsPage } from "../pages/workspace/SettingsPage";
import { MattersPage } from "../pages/workspace/MattersPage";
import { MatterWorkspacePage } from "../pages/workspace/MatterWorkspacePage";
import {
  MatterDocumentsTab,
  MatterOverviewTab,
  MatterTasksTab,
  MatterTimelineTab,
} from "../pages/workspace/MatterWorkspaceTabs";
import { ContactsPage } from "../pages/workspace/ContactsPage";
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
        <Route path="contacts" element={<ContactsPage />} />
        <Route path="activities" element={<ActivitiesPage />} />
        <Route path="billing" element={<BillingPage />} />
        <Route path="accounts" element={<AccountsPage />} />
        <Route path="documents" element={<DocumentsPage />} />
        <Route path="communications" element={<CommunicationsPage />} />
        <Route path="reports" element={<ReportsPage />} />
        <Route path="integrations" element={<IntegrationsPage />} />
        <Route path="settings" element={<SettingsPage />} />
        <Route path="resource-centre" element={<ResourceCentrePage />} />
        <Route path="parties" element={<Navigate to="/workspace/contacts" replace />} />
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
