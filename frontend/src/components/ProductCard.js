import { useState } from "react";
import { Link } from "react-router-dom";
import { Heart, ShoppingBag } from "lucide-react";
import { useStore } from "../context/StoreContext";

export default function ProductCard({ product }) {
  const store = useStore();
  const [message, setMessage] = useState("");
  const [adding, setAdding] = useState(false);

  if (!product) return null;

  const id = product.id ?? product._id;
  const name = product.name ?? product.title ?? "Product";
  const image = product.imageUrl ?? product.image ?? product.thumbnail ?? "";
  const price = Number(product.price ?? 0);
  const category =
    product.categoryName ?? product.category ?? "Everyday essentials";
  const stock = Number(product.stockQuantity ?? product.stock ?? 0);
  const inStock = stock > 0;

  const isWishlisted = (store?.wishlist || []).some(
    (item) =>
      String(typeof item === "object" ? (item.id ?? item._id) : item) ===
      String(id),
  );

  function handleWishlist() {
    store?.toggleWishlist?.(product);
  }

  async function handleAddToCart() {
    if (!inStock || adding) return;

    if (!store?.user?.token) {
      setMessage("Sign in to add this item.");
      return;
    }

    setAdding(true);
    try {
      await store.addToCart(product);
      setMessage("Added to your bag.");
    } catch (error) {
      setMessage(error?.message || "Could not add this item.");
    } finally {
      setAdding(false);
    }
  }

  return (
    <article className="product-card">
      <div className="product-image-wrap">
        <Link
          to={`/products/${id}`}
          className="product-image-link"
          aria-label={`View ${name}`}
        >
          {image ? (
            <img
              src={image}
              alt={name}
              className="product-image"
              loading="lazy"
            />
          ) : (
            <div className="image-placeholder">Image coming soon</div>
          )}
        </Link>

        {!inStock ? (
          <span className="product-badge out-of-stock">Out of stock</span>
        ) : product.badge ? (
          <span className="product-badge">{product.badge}</span>
        ) : null}

        <button
          type="button"
          className={`wishlist-button ${isWishlisted ? "active" : ""}`}
          onClick={handleWishlist}
          aria-label={isWishlisted ? "Remove from wishlist" : "Add to wishlist"}
        >
          <Heart size={17} fill={isWishlisted ? "currentColor" : "none"} />
        </button>

        <button
          type="button"
          className="quick-add"
          onClick={handleAddToCart}
          disabled={!inStock || adding}
          aria-label={
            !inStock
              ? `${name} is out of stock`
              : adding
                ? `Adding ${name} to bag`
                : `Add ${name} to bag`
          }
        >
          <ShoppingBag size={15} />
          {!inStock ? "Unavailable" : adding ? "Adding…" : "Add to bag"}
        </button>
      </div>

      <div className="product-info">
        <p className="product-category">{category}</p>

        <Link to={`/products/${id}`} className="product-name">
          {name}
        </Link>

        {product.brand && <p className="product-brand">{product.brand}</p>}

        <p className="product-price">
          ₹{price.toLocaleString("en-IN")}
          {Number(product.originalPrice) > price && (
            <span className="product-original-price">
              ₹{Number(product.originalPrice).toLocaleString("en-IN")}
            </span>
          )}
        </p>

        {message && (
          <p className="product-card-message" role="status">
            {message}
          </p>
        )}
      </div>
    </article>
  );
}
