import { Navigate, Outlet } from "react-router-dom";
import { useStore } from "../context/StoreContext";

export default function AdminRoute() {
  const { user } = useStore();
  const isAdmin = user?.role === "Admin" && !!user?.token;

  return isAdmin ? <Outlet /> : <Navigate to="/admin-login" replace />;
}
