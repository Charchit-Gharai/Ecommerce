import { useMemo, useState } from "react";
import { Search, SlidersHorizontal } from "lucide-react";
import { useSearchParams } from "react-router-dom";
import { useStore } from "../context/StoreContext";
import ProductCard from "../components/ProductCard";

export default function Products() {
  const { products, categories, loading } = useStore();
  const [params, setParams] = useSearchParams();
  const [search, setSearch] = useState("");
  const [sort, setSort] = useState("featured");

  const selectedCategory = params.get("category") || "All";

  const filteredProducts = useMemo(() => {
    let result = products.filter((product) => {
      const matchesCategory =
        selectedCategory === "All" ||
        product.category?.toLowerCase() === selectedCategory.toLowerCase();

      const text = `${product.name} ${product.description || ""}`.toLowerCase();
      return matchesCategory && text.includes(search.toLowerCase());
    });

    if (sort === "price-low") {
      result = [...result].sort((a, b) => a.price - b.price);
    } else if (sort === "price-high") {
      result = [...result].sort((a, b) => b.price - a.price);
    } else if (sort === "name") {
      result = [...result].sort((a, b) => a.name.localeCompare(b.name));
    }

    return result;
  }, [products, selectedCategory, search, sort]);

  function changeCategory(value) {
    if (value === "All") {
      params.delete("category");
    } else {
      params.set("category", value);
    }
    setParams(params);
  }

  return (
    <main className="container catalog-page">
      <section className="catalog-heading">
        <p className="eyebrow">THE GOOD-FINDS DEPARTMENT</p>
        <h1>Things you'll want to keep.</h1>
        <p>A little bit of everything, picked for everyday living.</p>
      </section>

      <section className="catalog-toolbar clay-panel">
        <label className="search-field">
          <Search size={18} />
          <input
            type="search"
            placeholder="Search the shelves…"
            value={search}
            onChange={(event) => setSearch(event.target.value)}
            aria-label="Search products"
          />
        </label>

        <label className="filter-field">
          <span>Category</span>
          <select
            value={selectedCategory}
            onChange={(event) => changeCategory(event.target.value)}
            aria-label="Filter by category"
          >
            <option value="All">All categories</option>
            {categories.map((category) => (
              <option key={category} value={category}>
                {category}
              </option>
            ))}
          </select>
        </label>

        <label className="sort-field">
          <SlidersHorizontal size={17} />
          <select
            value={sort}
            onChange={(event) => setSort(event.target.value)}
            aria-label="Sort products"
          >
            <option value="featured">Recommended</option>
            <option value="price-low">Price: low to high</option>
            <option value="price-high">Price: high to low</option>
            <option value="name">Name: A–Z</option>
          </select>
        </label>
      </section>

      <section className="catalog-results">
        <div className="results-heading">
          <p>
            {loading
              ? "Finding the good stuff…"
              : `${filteredProducts.length} products`}
          </p>
          <p>Made for real life.</p>
        </div>

        {loading ? (
          <div className="empty-message">
            <p>Loading products…</p>
          </div>
        ) : filteredProducts.length ? (
          <div className="product-grid">
            {filteredProducts.map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
        ) : (
          <div className="empty-message clay-panel">
            <h2>No products found</h2>
            <p>Try another search or category.</p>
            <button
              className="button button-clay"
              type="button"
              onClick={() => {
                setSearch("");
                changeCategory("All");
              }}
            >
              Clear filters
            </button>
          </div>
        )}
      </section>
    </main>
  );
}
