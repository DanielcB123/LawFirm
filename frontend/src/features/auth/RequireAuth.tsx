import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "./AuthContext";

export function RequireAuth() {
  const { token, isLoading } = useAuth();
  const location = useLocation();

  if (isLoading) {
    return (
      <section className="page">
        <h1>Loading workspace…</h1>
      </section>
    );
  }

  if (!token) {
    return <Navigate to="/workspace/login" replace state={{ from: location.pathname }} />;
  }

  return <Outlet />;
}
