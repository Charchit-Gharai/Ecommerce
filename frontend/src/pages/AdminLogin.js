import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { ShieldCheck, Eye, EyeOff } from "lucide-react";
import { useStore } from "../context/StoreContext";

export default function AdminLogin() {
  const store = useStore();
  const navigate = useNavigate();

  const [adminId, setAdminId] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  async function handleSubmit(event) {
    event.preventDefault();
    setError("");

    const cleanEmail = adminId.trim().toLowerCase();

    if (!cleanEmail) {
      setError("Please enter your admin email address.");
      return;
    }

    if (!password) {
      setError("Please enter your password.");
      return;
    }

    setLoading(true);

    try {
      // Calls POST /api/auth/login and stores the JWT in context
      const user = await store.signIn(cleanEmail, password);

      // The API returns the real role from the database
      if (user.role !== "Admin") {
        setError("This account does not have admin access.");
        store.signOut();
        return;
      }

      navigate("/admin", { replace: true });
    } catch (err) {
      setError(err?.message || "Invalid credentials. Please try again.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <main className="admin-login-page">
      <section className="admin-login-card">
        <div className="admin-login-icon">
          <ShieldCheck size={32} />
        </div>

        <p className="eyebrow">RESTRICTED ACCESS</p>
        <h1>Admin Login</h1>
        <p className="admin-login-description">
          Sign in with your administrator credentials.
        </p>

        <form onSubmit={handleSubmit} className="admin-login-form">
          <label htmlFor="admin-id">Admin Email</label>
          <input
            id="admin-id"
            type="email"
            autoComplete="email"
            value={adminId}
            onChange={(event) => setAdminId(event.target.value)}
            placeholder="admin@example.com"
            required
          />

          <label htmlFor="admin-password">Password</label>
          <div className="admin-password-field">
            <input
              id="admin-password"
              type={showPassword ? "text" : "password"}
              autoComplete="current-password"
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              placeholder="Enter password"
              required
            />
            <button
              type="button"
              className="admin-password-toggle"
              onClick={() => setShowPassword((current) => !current)}
              aria-label={showPassword ? "Hide password" : "Show password"}
            >
              {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
            </button>
          </div>

          {error && (
            <p className="admin-error" role="alert">
              {error}
            </p>
          )}

          <button
            type="submit"
            className="button button-dark admin-login-submit"
            disabled={loading}
          >
            {loading ? "Signing in..." : "Sign In as Admin"}
          </button>
        </form>

        <p className="admin-login-note">
          Use your administrator email and password to access the admin panel.
        </p>
      </section>
    </main>
  );
}
