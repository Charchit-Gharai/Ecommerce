import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useStore } from "../context/StoreContext";

export default function Login() {
  const store = useStore();
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  async function submit(event) {
    event.preventDefault();
    setError("");
    setBusy(true);

    try {
      const user = await store.signIn(email.trim().toLowerCase(), password);
      navigate(user.role === "Admin" ? "/admin" : "/", { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="auth-page container">
      <section className="auth-card clay-panel">
        <p className="eyebrow">GOOD TO SEE YOU AGAIN</p>
        <h1>Welcome back.</h1>
        <p className="auth-subtitle">Sign in to pick up where you left off.</p>

        {error && (
          <div className="form-error" role="alert">
            {error}
          </div>
        )}

        <form className="auth-form" onSubmit={submit}>
          <label htmlFor="login-email">Email</label>
          <input
            id="login-email"
            type="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            autoComplete="email"
            required
          />

          <label htmlFor="login-password">Password</label>
          <input
            id="login-password"
            type="password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            autoComplete="current-password"
            required
          />

          <button
            className="button button-clay auth-submit"
            type="submit"
            disabled={busy}
          >
            {busy ? "Signing in…" : "Sign in"}
          </button>
        </form>

        <p className="auth-switch">
          New here? <Link to="/register">Create an account</Link>
        </p>
      </section>
    </main>
  );
}
