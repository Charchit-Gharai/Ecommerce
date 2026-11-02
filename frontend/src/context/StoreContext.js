import { createContext, useContext, useEffect, useState } from "react";
import { api } from "../api/client";

const StoreContext = createContext(null);

function loadData(key, fallback) {
  try {
    const value = localStorage.getItem(key);
    return value ? JSON.parse(value) : fallback;
  } catch {
    return fallback;
  }
}

function toArray(value) {
  if (Array.isArray(value)) return value;
  if (Array.isArray(value?.items)) return value.items;
  return [];
}

function normalizeProduct(product) {
  return {
    ...product,
    id: product.id ?? product.productId,
    name: product.name ?? product.productName ?? "Product",
    category: product.category ?? product.categoryName ?? "Other",
    stockQuantity: Number(product.stockQuantity ?? product.stock ?? 0),
    price: Number(product.price ?? product.unitPrice ?? 0),
    imageUrl: product.imageUrl ?? product.image ?? "",
  };
}

export function StoreProvider({ children }) {
  const [products, setProducts] = useState([]);
  const [cart, setCart] = useState([]);
  const [user, setUser] = useState(() => loadData("morrow_user", null));
  const [orders, setOrders] = useState([]);
  const [wishlist, setWishlist] = useState(() =>
    loadData("morrow_wishlist", []),
  );
  const [loading, setLoading] = useState(true);
  const [cartLoading, setCartLoading] = useState(false);

  const categories = [
    ...new Set(products.map((p) => p.category).filter(Boolean)),
  ].sort();

  // Persist user to localStorage whenever it changes
  useEffect(() => {
    localStorage.setItem("morrow_user", JSON.stringify(user));
  }, [user]);

  useEffect(() => {
    localStorage.setItem("morrow_wishlist", JSON.stringify(wishlist));
  }, [wishlist]);

  // Load products on mount
  useEffect(() => {
    let cancelled = false;

    async function loadProducts() {
      try {
        const response = await api.get("/products");
        if (!cancelled) {
          setProducts(toArray(response).map(normalizeProduct));
        }
      } catch (error) {
        console.error("Could not load products:", error);
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    loadProducts();
    return () => {
      cancelled = true;
    };
  }, []);

  async function refreshCart() {
    if (!user?.token || user.role !== "Customer") {
      setCart([]);
      return [];
    }

    setCartLoading(true);

    try {
      const response = await api.get("/cart");
      const items = toArray(response?.items).map((item) => {
        const product = products.find(
          (p) => String(p.id) === String(item.productId),
        );

        return {
          id: item.id,
          productId: item.productId,
          product: product || {
            id: item.productId,
            name: item.productName ?? "Product",
            price: Number(item.unitPrice ?? 0),
            imageUrl: "",
            stockQuantity: 0,
            category: "Other",
          },
          productName: item.productName ?? product?.name ?? "Product",
          unitPrice: Number(item.unitPrice ?? product?.price ?? 0),
          quantity: Number(item.quantity ?? 1),
        };
      });

      setCart(items);
      return items;
    } finally {
      setCartLoading(false);
    }
  }

  useEffect(() => {
    if (!user?.token || user.role !== "Customer") {
      setCart([]);
      return;
    }
    if (!loading) {
      refreshCart().catch((err) => console.error("Could not load cart:", err));
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user?.token, user?.role, loading]);

  async function refreshOrders() {
    if (!user?.token || user.role !== "Customer") {
      setOrders([]);
      return [];
    }
    const response = await api.get("/orders/my-orders");
    const result = toArray(response);
    setOrders(result);
    return result;
  }

  useEffect(() => {
    if (user?.token && user.role === "Customer") {
      refreshOrders().catch((err) =>
        console.error("Could not load orders:", err),
      );
    } else {
      setOrders([]);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [user?.token, user?.role]);

  async function signIn(email, password) {
    const result = await api.post("/auth/login", { email, password });

    const nextUser = {
      id: result.userId,
      name: result.name ?? result.fullName ?? "",
      fullName: result.name ?? result.fullName ?? "",
      email: result.email,
      role: result.role,
      token: result.token,
      refreshToken: result.refreshToken,
      refreshTokenExpiresAt: result.refreshTokenExpiresAt,
    };

    setUser(nextUser);
    return nextUser;
  }

  async function register({ name, email, password }) {
    await api.post("/auth/register", { name, email, password });
    return signIn(email, password);
  }

  async function signOut() {
    try {
      await api.post("/auth/revoke");
    } catch {}

    setUser(null);
    setCart([]);
    setOrders([]);
    localStorage.removeItem("morrow_user");
  }

  async function refreshAccessToken() {
    const stored = localStorage.getItem("morrow_user");
    if (!stored) return false;

    const userData = JSON.parse(stored);
    if (!userData?.refreshToken) return false;

    try {
      const result = await api.post("/auth/refresh", {
        refreshToken: userData.refreshToken,
      });

      const updated = {
        ...userData,
        token: result.token,
        refreshToken: result.refreshToken,
        refreshTokenExpiresAt: result.refreshTokenExpiresAt,
      };

      localStorage.setItem("morrow_user", JSON.stringify(updated));
      setUser(updated);
      return true;
    } catch {
      await signOut();
      return false;
    }
  }

  async function addToCart(product, quantity = 1) {
    if (!user?.token)
      throw new Error("Please sign in to add items to your cart.");
    if (user.role !== "Customer")
      throw new Error("Only customer accounts can shop.");
    if (quantity < 1 || quantity > 10)
      throw new Error("Choose a quantity from 1 to 10.");

    await api.post("/cart/items", { productId: product.id, quantity });
    return refreshCart();
  }

  async function updateCartQuantity(cartItemId, quantity) {
    if (!user?.token) throw new Error("Please sign in to update your cart.");

    if (quantity <= 0) {
      await api.delete(`/cart/items/${cartItemId}`);
    } else {
      await api.put(`/cart/items/${cartItemId}`, { quantity });
    }

    return refreshCart();
  }

  async function removeFromCart(cartItemId) {
    await api.delete(`/cart/items/${cartItemId}`);
    return refreshCart();
  }

  function toggleWishlist(product) {
    setWishlist((current) => {
      const id = String(product.id);
      const exists = current.some((item) => String(item.id) === id);
      return exists
        ? current.filter((item) => String(item.id) !== id)
        : [...current, product];
    });
  }

  async function placeOrder(shippingAddress = "", phoneNumber = "") {
    if (!user?.token) throw new Error("Please sign in to place an order.");
    if (user.role !== "Customer")
      throw new Error("Only customers can place orders.");

    const order = await api.post("/orders", {
      shippingAddress: shippingAddress || null,
      phoneNumber: phoneNumber || null,
    });

    const payment = await api.post("/payments", {
      orderId: order.id,
      method: "Mock",
    });

    await Promise.all([refreshCart(), refreshOrders()]);

    return { order, payment };
  }

  const value = {
    products,
    setProducts,
    categories,
    cart,
    setCart,
    cartLoading,
    user,
    setUser,
    orders,
    setOrders,
    wishlist,
    loading,
    refreshCart,
    refreshOrders,
    refreshAccessToken,
    addToCart,
    updateCartQuantity,
    removeFromCart,
    toggleWishlist,
    signIn,
    register,
    signOut,
    placeOrder,
  };

  return (
    <StoreContext.Provider value={value}>{children}</StoreContext.Provider>
  );
}

export function useStore() {
  const context = useContext(StoreContext);
  if (!context) throw new Error("useStore must be used inside StoreProvider");
  return context;
}
