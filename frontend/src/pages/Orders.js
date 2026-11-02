import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import { Package, ShoppingBag, ChevronDown } from "lucide-react";
import { api } from "../api/client";
import { useStore } from "../context/StoreContext";

const statusNames = ["Pending", "Confirmed", "Cancelled"];

function getStatus(status) {
  if (typeof status === "number") return statusNames[status] ?? "Unknown";
  return String(status || "Pending").replace(/([a-z])([A-Z])/g, "$1 $2");
}

const money = (value) =>
  new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(Number(value || 0));

function getDate(value) {
  if (!value) return "Date unavailable";
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? String(value)
    : date.toLocaleDateString("en-IN", {
        day: "numeric",
        month: "short",
        year: "numeric",
      });
}

export default function Orders() {
  const store = useStore();
  const [orders, setOrders] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    let active = true;

    async function load() {
      if (!store.user?.token || store.user.role !== "Customer") {
        setOrders([]);
        return;
      }

      setLoading(true);
      setError("");

      try {
        const result = await api.get("/orders/my-orders");
        if (active)
          setOrders(Array.isArray(result) ? result : (result?.items ?? []));
      } catch (err) {
        if (active) setError(err.message);
      } finally {
        if (active) setLoading(false);
      }
    }

    load();
    return () => {
      active = false;
    };
  }, [store.user?.token, store.user?.role]);

  if (!store.user?.token) {
    return (
      <main className="container orders-page">
        <section className="cart-empty clay-panel">
          <Package size={42} />
          <h1>Your orders live here.</h1>
          <p>Sign in to view purchases you've made.</p>
          <Link to="/login" className="button button-clay">
            Sign in
          </Link>
        </section>
      </main>
    );
  }

  return (
    <main className="container orders-page">
      <header className="catalog-heading">
        <p className="eyebrow">YOUR SHOPPING STORY</p>
        <h1>Order history.</h1>
        <p>All your past purchases, together in one place.</p>
      </header>

      {error && (
        <div className="form-error" role="alert">
          {error}
        </div>
      )}

      {loading ? (
        <div className="empty-message clay-panel">Loading your orders…</div>
      ) : orders.length === 0 ? (
        <section className="cart-empty clay-panel">
          <ShoppingBag size={40} />
          <h2>No orders just yet.</h2>
          <p>Your next good find could be one click away.</p>
          <Link to="/products" className="button button-clay">
            Start shopping
          </Link>
        </section>
      ) : (
        <div className="orders-list">
          {orders.map((order) => {
            const items = order.items ?? [];
            const status = getStatus(order.status);

            return (
              <article className="order-card clay-card" key={order.id}>
                <div className="order-card-header">
                  <div>
                    <p className="eyebrow">ORDER NUMBER</p>
                    <h2 className="order-id">{order.id}</h2>
                    <p className="order-date">
                      Placed {getDate(order.orderDate)}
                    </p>
                  </div>
                  <span
                    className={`order-status status-${status.toLowerCase().replace(/\s+/g, "-")}`}
                  >
                    {status}
                  </span>
                </div>

                <div className="order-card-body">
                  <div className="order-items-preview">
                    <ShoppingBag size={18} />
                    <span>
                      {items.length} item{items.length === 1 ? "" : "s"}
                    </span>
                  </div>
                  <div className="order-total">
                    <span>Order total</span>
                    <strong>{money(order.totalAmount)}</strong>
                  </div>
                </div>

                <details className="order-details">
                  <summary>
                    View order details <ChevronDown size={17} />
                  </summary>
                  {items.length === 0 ? (
                    <p>No item details were returned by the API.</p>
                  ) : (
                    <ul>
                      {items.map((item, index) => (
                        <li key={`${item.productId}-${index}`}>
                          <span>
                            <strong>{item.productName}</strong>
                            <small>
                              Qty {item.quantity} × {money(item.unitPrice)}
                            </small>
                          </span>
                          <strong>
                            {money(
                              item.totalPrice ?? item.unitPrice * item.quantity,
                            )}
                          </strong>
                        </li>
                      ))}
                    </ul>
                  )}
                </details>
              </article>
            );
          })}
        </div>
      )}
    </main>
  );
}
