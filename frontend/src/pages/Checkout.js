import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { CheckCircle2, LockKeyhole } from "lucide-react";
import { useStore } from "../context/StoreContext";

const money = (value) =>
  new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(Number(value || 0));

export default function Checkout() {
  const store = useStore();
  const navigate = useNavigate();
  const [form, setForm] = useState({ address: "", phone: "" });
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [completedOrder, setCompletedOrder] = useState(null);

  const cart = store.cart;
  const total = cart.reduce(
    (sum, item) => sum + item.unitPrice * item.quantity,
    0,
  );

  function update(event) {
    setForm({ ...form, [event.target.name]: event.target.value });
  }

  async function submit(event) {
    event.preventDefault();
    setError("");

    if (!form.address.trim()) {
      setError("Please enter your shipping address.");
      return;
    }

    if (!/^[0-9+\-\s()]{7,20}$/.test(form.phone.trim())) {
      setError("Enter a valid phone number.");
      return;
    }

    setBusy(true);

    try {
      const result = await store.placeOrder(
        form.address.trim(),
        form.phone.trim(),
      );
      setCompletedOrder(result.order);

      setTimeout(() => navigate("/orders", { replace: true }), 3000);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  if (!store.user?.token) {
    return (
      <main className="container checkout-page">
        <section className="cart-empty clay-panel">
          <h1>Sign in to check out.</h1>
          <Link to="/login" className="button button-clay">
            Sign in
          </Link>
        </section>
      </main>
    );
  }

  if (completedOrder) {
    return (
      <main className="container checkout-page">
        <section className="confirmation-card clay-panel">
          <CheckCircle2 size={52} />
          <p className="eyebrow">ALL SET</p>
          <h1>Your order is in.</h1>
          <p>Your order ID is:</p>
          <strong className="confirmation-order-id">{completedOrder.id}</strong>
          <p className="confirmation-note">
            Check your order history for the latest status and items.
          </p>
          <button
            type="button"
            className="button button-clay"
            onClick={() => navigate("/orders", { replace: true })}
          >
            View order history
          </button>
        </section>
      </main>
    );
  }

  if (cart.length === 0) {
    return (
      <main className="container checkout-page">
        <section className="cart-empty clay-panel">
          <h1>Your cart is empty.</h1>
          <Link to="/products" className="button button-clay">
            Browse products
          </Link>
        </section>
      </main>
    );
  }

  return (
    <main className="container checkout-page">
      <header className="catalog-heading">
        <p className="eyebrow">THE LAST LITTLE STEP</p>
        <h1>Checkout.</h1>
        <p>Where should we send your order?</p>
      </header>

      <div className="checkout-layout">
        <form className="checkout-form clay-panel" onSubmit={submit}>
          <h2>Delivery details</h2>

          <label htmlFor="shipping-address">Shipping address</label>
          <textarea
            id="shipping-address"
            name="address"
            rows={4}
            placeholder="House / flat, street, area, city, state and PIN code"
            value={form.address}
            onChange={update}
            required
          />

          <label htmlFor="shipping-phone">Phone number</label>
          <input
            id="shipping-phone"
            name="phone"
            type="tel"
            inputMode="tel"
            autoComplete="tel"
            placeholder="Your contact number"
            value={form.phone}
            onChange={update}
            required
          />

          <div className="checkout-payment-note">
            <LockKeyhole size={18} />
            <div>
              <strong>Demo payment</strong>
              <p>
                This project currently uses a mock payment endpoint. No real
                card details are collected.
              </p>
            </div>
          </div>

          {error && (
            <div className="form-error" role="alert">
              {error}
            </div>
          )}

          <button
            className="button button-clay checkout-button"
            type="submit"
            disabled={busy}
          >
            {busy ? "Placing order…" : `Place order · ${money(total)}`}
          </button>

          <Link to="/cart" className="continue-shopping">
            Back to cart
          </Link>
        </form>

        <aside className="order-summary clay-panel">
          <p className="eyebrow">IN YOUR CART</p>
          <h2>Order summary</h2>

          {cart.map((item) => (
            <div className="checkout-summary-item" key={item.id}>
              <div>
                <strong>{item.productName}</strong>
                <p>Qty {item.quantity}</p>
              </div>
              <span>{money(item.unitPrice * item.quantity)}</span>
            </div>
          ))}

          <div className="summary-total">
            <span>Total</span>
            <strong>{money(total)}</strong>
          </div>
        </aside>
      </div>
    </main>
  );
}
