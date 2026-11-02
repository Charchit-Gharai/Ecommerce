import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useStore } from "../context/StoreContext";

export default function Register() {
  const store = useStore();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    name: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  function update(event) {
    setForm({ ...form, [event.target.name]: event.target.value });
  }

  async function submit(event) {
    event.preventDefault();
    setError("");

    if (form.name.trim().length < 1) {
      setError("Please enter your name.");
      return;
    }

    if (form.password.length < 8) {
      setError("Your password must contain at least 8 characters.");
      return;
    }

    if (!/[0-9]/.test(form.password)) {
      setError("Include at least one number in your password.");
      return;
    }

    if (form.password !== form.confirmPassword) {
      setError("Your passwords don't match.");
      return;
    }

    setBusy(true);

    try {
      await store.register({
        name: form.name.trim(),
        email: form.email.trim().toLowerCase(),
        password: form.password,
      });
      navigate("/", { replace: true });
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="auth-page container">
      <section className="auth-card clay-panel">
        <p className="eyebrow">A GOOD PLACE TO START</p>
        <h1>Make yourself at home.</h1>
        <p className="auth-subtitle">
          Create an account to keep track of your cart and orders.
        </p>

        {error && (
          <div className="form-error" role="alert">
            {error}
          </div>
        )}

        <form className="auth-form" onSubmit={submit}>
          <label htmlFor="register-name">Name</label>
          <input
            id="register-name"
            name="name"
            value={form.name}
            onChange={update}
            autoComplete="name"
            required
          />

          <label htmlFor="register-email">Email</label>
          <input
            id="register-email"
            name="email"
            type="email"
            value={form.email}
            onChange={update}
            autoComplete="email"
            required
          />

          <label htmlFor="register-password">Password</label>
          <input
            id="register-password"
            name="password"
            type="password"
            value={form.password}
            onChange={update}
            autoComplete="new-password"
            minLength={8}
            required
          />

          <label htmlFor="register-confirm">Confirm password</label>
          <input
            id="register-confirm"
            name="confirmPassword"
            type="password"
            value={form.confirmPassword}
            onChange={update}
            autoComplete="new-password"
            minLength={8}
            required
          />

          <button
            className="button button-clay auth-submit"
            disabled={busy}
            type="submit"
          >
            {busy ? "Creating your account…" : "Create account"}
          </button>
        </form>

        <p className="auth-switch">
          Already have an account? <Link to="/login">Sign in</Link>
        </p>
      </section>
    </main>
  );
}
