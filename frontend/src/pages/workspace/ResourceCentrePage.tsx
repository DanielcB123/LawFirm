import { Link } from "react-router-dom";

const topics = [
  { label: "Dashboard", path: "/workspace", detail: "Today’s tasks, intake, and matter load." },
  { label: "Calendar", path: "/workspace/calendar", detail: "Deadlines, events, and tasks on a calendar." },
  { label: "Tasks", path: "/workspace/worklists", detail: "Open, due today, and overdue work." },
  { label: "Matters", path: "/workspace/matters", detail: "The files the firm is handling." },
  { label: "Contacts", path: "/workspace/contacts", detail: "People and organizations." },
  { label: "Activities", path: "/workspace/activities", detail: "What has been recorded on a matter." },
  { label: "Documents", path: "/workspace/documents", detail: "Files saved on matters." },
  { label: "Communications", path: "/workspace/communications", detail: "Consultation messages already received." },
  { label: "Intake", path: "/workspace/intake", detail: "Prospects before they become matters." },
  { label: "Consultations", path: "/workspace/consultations", detail: "Requested meetings and follow-up." },
  { label: "Conflicts", path: "/workspace/conflicts", detail: "Checks before the firm accepts work." },
];

export function ResourceCentrePage() {
  return (
    <section className="page">
      <h1>Resource centre</h1>
      <p className="lead">Where to go in the workspace for the work this firm already tracks.</p>
      <ul className="list-reset">
        {topics.map((topic) => (
          <li key={topic.path} className="contact-row">
            <Link to={topic.path}>
              <strong>{topic.label}</strong>
            </Link>
            <p className="muted">{topic.detail}</p>
          </li>
        ))}
      </ul>
    </section>
  );
}
