import { Link } from "react-router-dom";
import { Heart, ShoppingBag, Trash2 } from "lucide-react";
import { useStore } from "../context/StoreContext";

export default function Wishlist() {
  const store = useStore();
  const wishlist = store?.wishlist || [];
  const products = store?.products || [];

  const getProduct = (item) => {
    if (typeof item === "object" && item !== null) {
      const id = item.id ?? item._id;
      return (
        products.find(
          (product) => String(product.id ?? product._id) === String(id),
        ) || item
      );
    }

    return products.find(
      (product) => String(product.id ?? product._id) === String(item),
    );
  };

  function removeFromWishlist(product) {
    if (product && typeof store?.toggleWishlist === "function") {
      store.toggleWishlist(product);
    }
  }

  function addToCart(product) {
    if (product && typeof store?.addToCart === "function") {
      store.addToCart(product);
    }
  }

  const savedProducts = wishlist.map(getProduct).filter(Boolean);

  return (
    <main className="container wishlist-page">
      <div className="catalog-heading">
        <p className="eyebrow">YOUR SAVED ITEMS</p>
        <h1>My wishlist</h1>
        <p>Keep the things you love in one place.</p>
      </div>

      {savedProducts.length === 0 ? (
        <section className="cart-empty">
          <Heart size={42} strokeWidth={1.3} />
          <h2>Your wishlist is empty</h2>
          <p>Save products here to find them easily later.</p>
          <Link to="/products" className="button button-dark">
            Explore products
          </Link>
        </section>
      ) : (
        <div className="product-grid">
          {savedProducts.map((product, index) => {
            const id = product.id ?? product._id;
            const name = product.name ?? product.title ?? "Product";
            const image =
              product.image ?? product.imageUrl ?? product.thumbnail ?? "";

            return (
              <article className="product-card" key={id ?? index}>
                <Link to={`/products/${id}`} className="product-image-wrap">
                  {image ? (
                    <img src={image} alt={name} className="product-image" />
                  ) : (
                    <div className="image-placeholder">No image available</div>
                  )}
                </Link>

                <div className="product-info">
                  <p className="product-category">
                    {product.category ?? "Essentials"}
                  </p>

                  <Link to={`/products/${id}`} className="product-name">
                    {name}
                  </Link>

                  <p className="product-price">
                    ₹{Number(product.price ?? 0).toLocaleString("en-IN")}
                  </p>

                  <div className="wishlist-actions">
                    <button
                      type="button"
                      className="button button-dark"
                      onClick={() => addToCart(product)}
                    >
                      <ShoppingBag size={16} />
                      Add to bag
                    </button>

                    <button
                      type="button"
                      className="button button-outline"
                      onClick={() => removeFromWishlist(product)}
                      aria-label={`Remove ${name} from wishlist`}
                    >
                      <Trash2 size={16} />
                    </button>
                  </div>
                </div>
              </article>
            );
          })}
        </div>
      )}
    </main>
  );
}
