import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { Minus, Plus, Trash2, ShoppingBag } from "lucide-react";
import { useStore } from "../context/StoreContext";

const money = (value) =>
  new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(Number(value || 0));

export default function Cart() {
  const store = useStore();
  const navigate = useNavigate();
  const [busyId, setBusyId] = useState(null);
  const [error, setError] = useState("");

  const cart = store.cart;
  const subtotal = cart.reduce(
    (sum, item) => sum + item.unitPrice * item.quantity,
    0,
  );

  async function changeQuantity(item, quantity) {
    setBusyId(item.id);
    setError("");

    try {
      await store.updateCartQuantity(item.id, quantity);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusyId(null);
    }
  }

  async function remove(item) {
    setBusyId(item.id);
    setError("");

    try {
      await store.removeFromCart(item.id);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusyId(null);
    }
  }

  if (!store.user?.token) {
    return (
      <main className="container cart-page">
        <section className="cart-empty clay-panel">
          <ShoppingBag size={38} />
          <h1>Your cart is waiting.</h1>
          <p>Sign in to view and manage your shopping cart.</p>
          <Link className="button button-clay" to="/login">
            Sign in
          </Link>
        </section>
      </main>
    );
  }

  return (
    <main className="container cart-page">
      <header className="catalog-heading">
        <p className="eyebrow">YOUR LITTLE COLLECTION</p>
        <h1>Your cart.</h1>
        <p>Take one last look before checking out.</p>
      </header>

      {error && (
        <div className="form-error" role="alert">
          {error}
        </div>
      )}

      {cart.length === 0 ? (
        <section className="cart-empty clay-panel">
          <ShoppingBag size={40} />
          <h2>Nothing in here just yet.</h2>
          <p>Find something useful, lovely, or a little bit of both.</p>
          <Link className="button button-clay" to="/products">
            Explore the shop
          </Link>
        </section>
      ) : (
        <div className="cart-layout">
          <section className="cart-items">
            {cart.map((item) => (
              <article className="cart-item clay-card" key={item.id}>
                <Link
                  to={`/products/${item.productId}`}
                  className="cart-item-image"
                >
                  {item.product?.imageUrl ? (
                    <img src={item.product.imageUrl} alt={item.productName} />
                  ) : (
                    <div className="image-placeholder">
                      <ShoppingBag size={25} />
                    </div>
                  )}
                </Link>

                <div className="cart-item-info">
                  <p className="product-category">
                    {item.product?.category || "General"}
                  </p>
                  <Link
                    to={`/products/${item.productId}`}
                    className="product-name"
                  >
                    {item.productName}
                  </Link>
                  <p className="product-price">{money(item.unitPrice)}</p>

                  <div className="quantity-stepper">
                    <button
                      type="button"
                      aria-label="Decrease quantity"
                      disabled={busyId === item.id || item.quantity <= 1}
                      onClick={() => changeQuantity(item, item.quantity - 1)}
                    >
                      <Minus size={15} />
                    </button>
                    <span>{item.quantity}</span>
                    <button
                      type="button"
                      aria-label="Increase quantity"
                      disabled={
                        busyId === item.id ||
                        item.quantity >= 10 ||
                        item.quantity >= (item.product?.stockQuantity ?? 10)
                      }
                      onClick={() => changeQuantity(item, item.quantity + 1)}
                    >
                      <Plus size={15} />
                    </button>
                  </div>
                </div>

                <div className="cart-item-end">
                  <strong>{money(item.unitPrice * item.quantity)}</strong>
                  <button
                    className="remove-button"
                    type="button"
                    disabled={busyId === item.id}
                    onClick={() => remove(item)}
                  >
                    <Trash2 size={16} /> Remove
                  </button>
                </div>
              </article>
            ))}
          </section>

          <aside className="order-summary clay-panel">
            <p className="eyebrow">THE NUMBERS</p>
            <h2>Order summary</h2>

            <div className="summary-row">
              <span>
                Items ({cart.reduce((sum, item) => sum + item.quantity, 0)})
              </span>
              <span>{money(subtotal)}</span>
            </div>

            <div className="summary-total">
              <span>Total</span>
              <strong>{money(subtotal)}</strong>
            </div>

            <p className="shipping-note">
              Delivery charges, if any, will be confirmed separately.
            </p>

            <button
              type="button"
              className="button button-clay checkout-button"
              onClick={() => navigate("/checkout")}
            >
              Proceed to checkout
            </button>

            <Link to="/products" className="continue-shopping">
              Keep browsing
            </Link>
          </aside>
        </div>
      )}
    </main>
  );
}
