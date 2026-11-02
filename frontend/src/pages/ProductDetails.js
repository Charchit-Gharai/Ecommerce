import { useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, Minus, Plus, ShoppingBag } from "lucide-react";
import { useStore } from "../context/StoreContext";

const money = (value) =>
  new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(Number(value || 0));

export default function ProductDetails() {
  const { id } = useParams();
  const navigate = useNavigate();
  const store = useStore();

  const [quantity, setQuantity] = useState(1);
  const [busy, setBusy] = useState(false);
  const [message, setMessage] = useState("");

  const product = store.products.find((item) => String(item.id) === String(id));

  if (!product && store.loading) {
    return (
      <main className="container details-page">
        <p>Loading product…</p>
      </main>
    );
  }

  if (!product) {
    return (
      <main className="container details-page">
        <Link to="/products" className="text-link">
          <ArrowLeft size={17} /> Back to shop
        </Link>
        <section className="empty-message clay-panel">
          <h1>We couldn't find that one.</h1>
          <p>The product may have been removed or is unavailable.</p>
          <Link to="/products" className="button button-clay">
            Browse products
          </Link>
        </section>
      </main>
    );
  }

  const stock = Number(product.stockQuantity || 0);
  const inStock = stock > 0;

  async function handleAdd() {
    if (!store.user?.token) {
      navigate("/login");
      return;
    }

    if (store.user.role !== "Customer") {
      setMessage("Please sign in with a customer account to shop.");
      return;
    }

    if (!inStock) {
      setMessage("This product is currently out of stock.");
      return;
    }

    if (quantity > Math.min(10, stock)) {
      setMessage(`Only ${stock} item(s) are currently available.`);
      return;
    }

    setBusy(true);
    setMessage("");

    try {
      await store.addToCart(product, quantity);
      setMessage("Added to your cart.");
    } catch (error) {
      setMessage(error.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <main className="container details-page">
      <Link to="/products" className="text-link back-link">
        <ArrowLeft size={17} /> Back to shop
      </Link>

      <div className="details-layout">
        <div className="details-image clay-panel">
          {product.imageUrl ? (
            <img src={product.imageUrl} alt={product.name} />
          ) : (
            <div className="image-placeholder">
              <ShoppingBag size={38} />
              <span>Image coming soon</span>
            </div>
          )}
          {!inStock && (
            <span className="stock-badge out-stock">Out of stock</span>
          )}
        </div>

        <section className="details-info">
          <p className="eyebrow">{product.category}</p>
          <h1>{product.name}</h1>
          <p className="details-price">{money(product.price)}</p>

          <p className="details-description">
            {product.description || "A useful find for everyday life."}
          </p>

          <p
            className={`stock-line ${inStock ? "text-in-stock" : "text-out-stock"}`}
          >
            <span className="stock-dot" />
            {inStock ? `${stock} available` : "Currently unavailable"}
          </p>

          <div className="quantity-control">
            <span>Quantity</span>
            <div className="quantity-stepper">
              <button
                type="button"
                aria-label="Decrease quantity"
                disabled={quantity <= 1}
                onClick={() => setQuantity((value) => Math.max(1, value - 1))}
              >
                <Minus size={16} />
              </button>
              <span>{quantity}</span>
              <button
                type="button"
                aria-label="Increase quantity"
                disabled={!inStock || quantity >= Math.min(10, stock)}
                onClick={() =>
                  setQuantity((value) => Math.min(10, stock, value + 1))
                }
              >
                <Plus size={16} />
              </button>
            </div>
          </div>

          <button
            type="button"
            className="button button-clay details-add"
            onClick={handleAdd}
            disabled={!inStock || busy}
          >
            <ShoppingBag size={18} />
            {busy ? "Adding…" : inStock ? "Add to cart" : "Out of stock"}
          </button>

          {message && (
            <p className="inline-message" role="status">
              {message}
            </p>
          )}
        </section>
      </div>
    </main>
  );
}
