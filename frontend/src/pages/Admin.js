import { useEffect, useMemo, useState } from "react";
import {
  Search,
  Plus,
  Pencil,
  Trash2,
  X,
  Package,
  LogOut,
  Image as ImageIcon,
  RefreshCw,
} from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useStore } from "../context/StoreContext";
import { api } from "../api/client";
import "./Admin.css";

const emptyForm = {
  name: "",
  price: "",
  stock: "",
  category: "",
  description: "",
  imageUrl: "",
};

function getProductId(product) {
  return product?.id ?? product?.Id;
}

function getProductName(product) {
  return product?.name ?? product?.Name ?? "";
}

function getProductPrice(product) {
  return Number(product?.price ?? product?.Price ?? 0);
}

function getProductStock(product) {
  return Number(
    product?.stockQuantity ??
      product?.StockQuantity ??
      product?.stock ??
      product?.Stock ??
      0,
  );
}

function getProductCategory(product) {
  return product?.category ?? product?.Category ?? "General";
}

function getProductDescription(product) {
  return product?.description ?? product?.Description ?? "";
}

function getProductImage(product) {
  return (
    product?.imageUrl ??
    product?.ImageUrl ??
    product?.imageURL ??
    product?.ImageURL ??
    product?.image ??
    product?.Image ??
    ""
  );
}

function isValidImageUrl(value) {
  if (!value.trim()) return true;
  try {
    const url = new URL(value.trim());
    return url.protocol === "http:" || url.protocol === "https:";
  } catch {
    return false;
  }
}

function formatCurrency(value) {
  return new Intl.NumberFormat("en-IN", {
    style: "currency",
    currency: "INR",
    maximumFractionDigits: 2,
  }).format(Number(value) || 0);
}

function normalizeProducts(response) {
  if (Array.isArray(response)) return response;
  if (Array.isArray(response?.items)) return response.items;
  if (Array.isArray(response?.Items)) return response.Items;
  if (Array.isArray(response?.data)) return response.data;
  if (Array.isArray(response?.Data)) return response.Data;
  return [];
}

export default function Admin() {
  const store = useStore();
  const navigate = useNavigate();

  const [products, setProducts] = useState([]);
  const [search, setSearch] = useState("");
  const [editingId, setEditingId] = useState(null);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm);

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [apiLoading, setApiLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [deletingId, setDeletingId] = useState(null);

  async function refreshProducts() {
    try {
      const response = await api.get("/products");
      const list = normalizeProducts(response);
      setProducts(list);
      return list;
    } catch (err) {
      setError(
        err?.message ||
          "Unable to load products. Please check your backend connection.",
      );
      return [];
    }
  }

  useEffect(() => {
    let active = true;

    async function loadProducts() {
      setApiLoading(true);
      setError("");

      try {
        const response = await api.get("/products");
        if (!active) return;
        const list = normalizeProducts(response);
        setProducts(list);
      } catch (err) {
        if (active) {
          setError(
            err?.message ||
              "Unable to load products. Please check your backend connection.",
          );
        }
      } finally {
        if (active) setApiLoading(false);
      }
    }

    loadProducts();

    return () => {
      active = false;
    };
  }, []);

  const filteredProducts = useMemo(() => {
    const query = search.trim().toLowerCase();
    if (!query) return products;
    return products.filter((product) =>
      getProductName(product).toLowerCase().includes(query),
    );
  }, [products, search]);

  const totalProducts = products.length;

  const lowStockCount = products.filter((product) => {
    const stock = getProductStock(product);
    return stock > 0 && stock <= 5;
  }).length;

  const outOfStockCount = products.filter(
    (product) => getProductStock(product) <= 0,
  ).length;

  const inventoryValue = products.reduce(
    (total, product) =>
      total + getProductPrice(product) * getProductStock(product),
    0,
  );

  function resetMessages() {
    setError("");
    setSuccess("");
  }

  function openAddForm() {
    resetMessages();
    setEditingId(null);
    setForm(emptyForm);
    setShowForm(true);
  }

  function openEditForm(product) {
    resetMessages();
    setEditingId(getProductId(product));
    setForm({
      name: getProductName(product),
      price: String(getProductPrice(product)),
      stock: String(getProductStock(product)),
      category: getProductCategory(product),
      description: getProductDescription(product),
      imageUrl: getProductImage(product),
    });
    setShowForm(true);
  }

  function closeForm() {
    setShowForm(false);
    setEditingId(null);
    setForm(emptyForm);
  }

  function handleChange(event) {
    const { name, value } = event.target;
    setForm((previous) => ({
      ...previous,
      [name]: value,
    }));
  }

  function handleAdminLogout() {
    if (store?.signOut) {
      store.signOut();
    }
    navigate("/admin-login", { replace: true });
  }

  async function handleSubmit(event) {
    event.preventDefault();
    resetMessages();

    const name = form.name.trim();
    const price = Number(form.price);
    const stock = Number(form.stock);
    const category = form.category.trim() || "General";
    const description = form.description.trim() || name;
    const imageUrl = form.imageUrl.trim();

    if (!name) {
      setError("Please enter a product name.");
      return;
    }

    if (name.length > 200) {
      setError("Product name must not exceed 200 characters.");
      return;
    }

    if (!Number.isFinite(price) || price <= 0) {
      setError("Please enter a valid price greater than zero.");
      return;
    }

    if (form.stock.trim() === "" || !Number.isInteger(stock) || stock < 0) {
      setError("Stock must be a whole number greater than or equal to zero.");
      return;
    }

    if (!isValidImageUrl(imageUrl)) {
      setError("Image URL must be a valid HTTP or HTTPS URL.");
      return;
    }

    const payload = {
      name,
      description,
      category,
      price,
      stockQuantity: stock,
      imageUrl: imageUrl || null,
    };

    setSaving(true);

    try {
      if (editingId !== null && editingId !== undefined) {
        await api.put(`/products/${editingId}`, payload);
        setSuccess("Product updated successfully.");
      } else {
        await api.post("/products", payload);
        setSuccess("Product added successfully.");
      }

      closeForm();
      await refreshProducts();
    } catch (err) {
      setError(err?.message || "Unable to save the product. Please try again.");
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(product) {
    const id = getProductId(product);

    if (id === null || id === undefined) {
      setError("Cannot delete this product because its ID is missing.");
      return;
    }

    const confirmed = window.confirm(
      `Are you sure you want to delete "${getProductName(product)}"?`,
    );

    if (!confirmed) return;

    resetMessages();
    setDeletingId(id);

    try {
      await api.delete(`/products/${id}`);
      await refreshProducts();
      setSuccess("Product deleted successfully.");
    } catch (err) {
      setError(err?.message || "Unable to delete the product.");
    } finally {
      setDeletingId(null);
    }
  }

  return (
    <main className="admin-page">
      <div className="admin-container">
        <header className="admin-heading">
          <div>
            <span className="admin-eyebrow">SHOPKART MANAGEMENT</span>
            <h1>Admin Dashboard</h1>
            <p>Manage your products and monitor inventory.</p>
          </div>

          <div className="admin-heading-actions">
            <button
              type="button"
              className="admin-secondary-button"
              onClick={refreshProducts}
              disabled={apiLoading || saving}
            >
              <RefreshCw size={17} />
              Refresh
            </button>

            <button
              type="button"
              className="admin-secondary-button"
              onClick={handleAdminLogout}
            >
              <LogOut size={17} />
              Logout
            </button>
          </div>
        </header>

        {error && (
          <div className="admin-alert admin-alert-error" role="alert">
            {error}
          </div>
        )}

        {success && (
          <div className="admin-alert admin-alert-success" role="status">
            {success}
          </div>
        )}

        <section className="admin-summary">
          <article className="admin-summary-card">
            <div className="admin-summary-icon">
              <Package size={21} />
            </div>
            <div>
              <p>Total products</p>
              <h2>{totalProducts}</h2>
            </div>
          </article>

          <article className="admin-summary-card">
            <div className="admin-summary-icon">
              <ImageIcon size={21} />
            </div>
            <div>
              <p>Low stock</p>
              <h2>{lowStockCount}</h2>
            </div>
          </article>

          <article className="admin-summary-card">
            <div className="admin-summary-icon">
              <Package size={21} />
            </div>
            <div>
              <p>Out of stock</p>
              <h2>{outOfStockCount}</h2>
            </div>
          </article>

          <article className="admin-summary-card">
            <div className="admin-summary-icon">
              <Package size={21} />
            </div>
            <div>
              <p>Inventory value</p>
              <h2>{formatCurrency(inventoryValue)}</h2>
            </div>
          </article>
        </section>

        <section className="admin-panel">
          <div className="admin-toolbar">
            <div>
              <h2>Product inventory</h2>
              <p>Manage product details, pricing and stock.</p>
            </div>

            <button
              type="button"
              className="admin-primary-button"
              onClick={openAddForm}
            >
              <Plus size={18} />
              Add product
            </button>
          </div>

          <div className="admin-search">
            <Search size={18} />
            <input
              type="search"
              value={search}
              onChange={(event) => setSearch(event.target.value)}
              placeholder="Search products..."
              aria-label="Search products"
            />
          </div>

          {showForm && (
            <form className="admin-product-form" onSubmit={handleSubmit}>
              <div className="admin-form-heading">
                <div>
                  <h3>{editingId !== null ? "Edit product" : "Add product"}</h3>
                  <p>Enter product information below.</p>
                </div>

                <button
                  type="button"
                  className="admin-icon-button"
                  onClick={closeForm}
                  aria-label="Close product form"
                >
                  <X size={20} />
                </button>
              </div>

              <div className="admin-form-grid">
                <div className="admin-form-field">
                  <label htmlFor="productName">Product name *</label>
                  <input
                    id="productName"
                    name="name"
                    type="text"
                    value={form.name}
                    onChange={handleChange}
                    maxLength={200}
                    placeholder="Enter product name"
                    required
                  />
                </div>

                <div className="admin-form-field">
                  <label htmlFor="productCategory">Category *</label>
                  <input
                    id="productCategory"
                    name="category"
                    type="text"
                    value={form.category}
                    onChange={handleChange}
                    maxLength={100}
                    placeholder="e.g. Electronics, Clothing"
                    required
                  />
                </div>

                <div className="admin-form-field">
                  <label htmlFor="productPrice">Price (₹) *</label>
                  <input
                    id="productPrice"
                    name="price"
                    type="number"
                    min="0.01"
                    step="0.01"
                    value={form.price}
                    onChange={handleChange}
                    placeholder="Enter price"
                    required
                  />
                </div>

                <div className="admin-form-field">
                  <label htmlFor="productStock">Stock quantity *</label>
                  <input
                    id="productStock"
                    name="stock"
                    type="number"
                    min="0"
                    step="1"
                    value={form.stock}
                    onChange={handleChange}
                    placeholder="Enter quantity"
                    required
                  />
                </div>

                <div
                  className="admin-form-field"
                  style={{ gridColumn: "1 / -1" }}
                >
                  <label htmlFor="productDescription">Description</label>
                  <input
                    id="productDescription"
                    name="description"
                    type="text"
                    value={form.description}
                    onChange={handleChange}
                    maxLength={1000}
                    placeholder="Short product description"
                  />
                </div>

                <div
                  className="admin-form-field"
                  style={{ gridColumn: "1 / -1" }}
                >
                  <label htmlFor="imageUrl">Product image URL</label>
                  <input
                    id="imageUrl"
                    name="imageUrl"
                    type="url"
                    value={form.imageUrl}
                    onChange={handleChange}
                    placeholder="https://example.com/product.jpg"
                  />
                  <small>
                    Use a publicly accessible HTTP or HTTPS image URL.
                  </small>
                </div>
              </div>

              {form.imageUrl && isValidImageUrl(form.imageUrl) && (
                <div className="admin-image-preview">
                  <img
                    src={form.imageUrl}
                    alt="Product preview"
                    onError={(event) => {
                      event.currentTarget.style.display = "none";
                    }}
                    onLoad={(event) => {
                      event.currentTarget.style.display = "block";
                    }}
                  />
                </div>
              )}

              <div className="admin-form-actions">
                <button
                  type="button"
                  className="admin-secondary-button"
                  onClick={closeForm}
                  disabled={saving}
                >
                  Cancel
                </button>

                <button
                  type="submit"
                  className="admin-primary-button"
                  disabled={saving}
                >
                  {saving
                    ? "Saving..."
                    : editingId !== null
                      ? "Save changes"
                      : "Create product"}
                </button>
              </div>
            </form>
          )}

          {apiLoading ? (
            <div className="admin-loading">Loading products...</div>
          ) : filteredProducts.length === 0 ? (
            <div className="admin-empty">
              <Package size={34} />
              <h3>No products found</h3>
              <p>
                {search
                  ? "Try another search term."
                  : "Add your first product to get started."}
              </p>
            </div>
          ) : (
            <div className="admin-table-wrapper">
              <table className="admin-products-table">
                <thead>
                  <tr>
                    <th>Product</th>
                    <th>Category</th>
                    <th>Price</th>
                    <th>Stock</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>

                <tbody>
                  {filteredProducts.map((product, index) => {
                    const id = getProductId(product);
                    const stock = getProductStock(product);
                    const imageUrl = getProductImage(product);

                    let stockLabel = "In stock";
                    let stockClass = "in-stock";

                    if (stock <= 0) {
                      stockLabel = "Out of stock";
                      stockClass = "out-of-stock";
                    } else if (stock <= 5) {
                      stockLabel = "Low stock";
                      stockClass = "low-stock";
                    }

                    return (
                      <tr key={id ?? `product-${index}`}>
                        <td>
                          <div className="admin-product-cell">
                            {imageUrl ? (
                              <img
                                className="admin-product-thumbnail"
                                src={imageUrl}
                                alt={getProductName(product)}
                                onError={(event) => {
                                  event.currentTarget.style.display = "none";
                                }}
                              />
                            ) : (
                              <div className="admin-product-placeholder">
                                <ImageIcon size={20} />
                              </div>
                            )}
                            <span>{getProductName(product)}</span>
                          </div>
                        </td>

                        <td>{getProductCategory(product)}</td>

                        <td>{formatCurrency(getProductPrice(product))}</td>

                        <td>{stock}</td>

                        <td>
                          <span className={`admin-stock-badge ${stockClass}`}>
                            {stockLabel}
                          </span>
                        </td>

                        <td>
                          <div className="admin-actions">
                            <button
                              type="button"
                              className="admin-icon-button"
                              onClick={() => openEditForm(product)}
                              aria-label={`Edit ${getProductName(product)}`}
                              title="Edit product"
                            >
                              <Pencil size={17} />
                            </button>

                            <button
                              type="button"
                              className="admin-icon-button admin-delete-button"
                              onClick={() => handleDelete(product)}
                              disabled={deletingId === id}
                              aria-label={`Delete ${getProductName(product)}`}
                              title="Delete product"
                            >
                              <Trash2 size={17} />
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </section>
      </div>
    </main>
  );
}
