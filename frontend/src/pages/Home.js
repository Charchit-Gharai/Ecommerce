import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  ArrowRight,
  Smartphone,
  Shirt,
  Laptop,
  House,
  Sparkles,
  BookOpen,
  Dumbbell,
  Gamepad2,
  Armchair,
  Watch,
  ChevronLeft,
  ChevronRight,
  PackageCheck,
  RotateCcw,
  ShieldCheck,
  Headphones,
} from "lucide-react";

import ProductCard from "../components/ProductCard";
import { useStore } from "../context/StoreContext";
import "./Home.css";

const categories = [
  { name: "Electronics", icon: Laptop, category: "Electronics" },
  { name: "Mobiles", icon: Smartphone, category: "Mobiles" },
  { name: "Fashion", icon: Shirt, category: "Fashion" },
  { name: "Home & Living", icon: House, category: "Home" },
  { name: "Beauty", icon: Sparkles, category: "Beauty" },
  { name: "Books", icon: BookOpen, category: "Books" },
  { name: "Sports", icon: Dumbbell, category: "Sports" },
  { name: "Toys & Games", icon: Gamepad2, category: "Toys" },
  { name: "Furniture", icon: Armchair, category: "Furniture" },
  { name: "Accessories", icon: Watch, category: "Accessories" },
];

const banners = [
  {
    image:
      "https://images.unsplash.com/photo-1490481651871-ab68de25d43d?auto=format&fit=crop&w=1600&q=90",
    label: "THE DESIGNER EDIT",
    title: "Effortless style.\nEnduring elegance.",
    description:
      "Discover refined silhouettes, timeless dresses, and elevated wardrobe essentials.",
    position: "center 42%",
    link: "/products?category=Fashion",
  },
  {
    image:
      "https://images.unsplash.com/photo-1523170335258-f5ed11844a49?auto=format&fit=crop&w=1600&q=90",
    label: "THE WATCH COLLECTION",
    title: "Time, beautifully\ntold.",
    description:
      "Explore elegant timepieces where exceptional design meets everyday sophistication.",
    position: "center 48%",
    link: "/products?category=Accessories",
  },
  {
    image:
      "https://images.unsplash.com/photo-1616486338812-3dadae4b4ace?auto=format&fit=crop&w=1600&q=90",
    label: "THE ART OF LIVING",
    title: "Spaces with\nsoul and style.",
    description:
      "Elevate your interiors with sculptural accents, warm textures, and considered décor.",
    position: "center 50%",
    link: "/products?category=Home",
  },
  {
    image:
      "https://images.unsplash.com/photo-1541643600914-78b084683601?auto=format&fit=crop&w=1600&q=90",
    label: "THE FRAGRANCE EDIT",
    title: "A signature\nof your own.",
    description:
      "Discover exquisite fragrances and beauty essentials that make every moment feel special.",
    position: "center 50%",
    link: "/products?category=Beauty",
  },
];

const benefits = [
  {
    icon: PackageCheck,
    title: "Thoughtful selection",
    text: "Products across every category",
  },
  {
    icon: RotateCcw,
    title: "Easy returns",
    text: "A straightforward return experience",
  },
  {
    icon: ShieldCheck,
    title: "Secure checkout",
    text: "Shop with confidence",
  },
  {
    icon: Headphones,
    title: "Here to help",
    text: "Support when you need it",
  },
];

export default function Home() {
  const store = useStore();
  const products = Array.isArray(store?.products) ? store.products : [];
  const featuredProducts = products.slice(0, 4);

  const [activeBanner, setActiveBanner] = useState(0);
  const currentBanner = banners[activeBanner];

  useEffect(() => {
    const timer = setInterval(() => {
      setActiveBanner((current) => (current + 1) % banners.length);
    }, 5000);

    return () => clearInterval(timer);
  }, []);

  const previousBanner = () => {
    setActiveBanner(
      (current) => (current - 1 + banners.length) % banners.length,
    );
  };

  const nextBanner = () => {
    setActiveBanner((current) => (current + 1) % banners.length);
  };

  return (
    <main className="sk-home">
      {/* Left introduction and right image carousel */}
      <section className="sk-hero-layout">
        <div className="sk-hero-intro">
          <p className="sk-eyebrow">A MORE CONSIDERED WAY TO SHOP</p>

          <h1>
            Little things.
            <br />
            <span>Better living.</span>
          </h1>

          <div className="sk-intro-divider" />

          <p className="sk-intro-description">
            From everyday essentials to exceptional finds, discover pieces that
            make your world feel a little more you.
          </p>

          <Link to="/products" className="sk-intro-link">
            Explore the collection
            <span className="sk-intro-link-icon">
              <ArrowRight size={16} />
            </span>
          </Link>

          <div className="sk-intro-note">
            <span className="sk-intro-note-dot" />
            <span>THOUGHTFULLY SELECTED. MADE FOR EVERYDAY.</span>
          </div>

          <span className="sk-intro-decoration" aria-hidden="true">
            S.
          </span>
        </div>

        <div className="sk-hero" aria-label="Featured collections">
          <img
            key={currentBanner.image}
            className="sk-hero-image"
            src={currentBanner.image}
            alt={currentBanner.title.replace("\n", " ")}
            style={{ objectPosition: currentBanner.position }}
          />

          <div className="sk-hero-shade" />

          <div className="sk-hero-content">
            <p className="sk-eyebrow">{currentBanner.label}</p>

            <h2>
              {currentBanner.title.split("\n").map((line, index) => (
                <span key={index}>
                  {line}
                  {index === 0 && <br />}
                </span>
              ))}
            </h2>

            <p className="sk-hero-description">{currentBanner.description}</p>

            <Link to={currentBanner.link} className="sk-button sk-button-light">
              Discover the collection <ArrowRight size={15} />
            </Link>
          </div>

          <div className="sk-hero-controls">
            <button
              type="button"
              onClick={previousBanner}
              aria-label="Previous banner"
            >
              <ChevronLeft size={18} />
            </button>

            <button type="button" onClick={nextBanner} aria-label="Next banner">
              <ChevronRight size={18} />
            </button>
          </div>

          <div className="sk-hero-pagination">
            {banners.map((banner, index) => (
              <button
                type="button"
                key={banner.label}
                className={index === activeBanner ? "active" : ""}
                onClick={() => setActiveBanner(index)}
                aria-label={`Show ${banner.label}`}
                aria-pressed={index === activeBanner}
              />
            ))}
          </div>

          <span className="sk-hero-count">
            {String(activeBanner + 1).padStart(2, "0")}
            <span>/</span>
            {String(banners.length).padStart(2, "0")}
          </span>
        </div>
      </section>

      {/* Category sidebar and featured products */}
      <section className="sk-discovery">
        <aside className="sk-category-sidebar">
          <p className="sk-eyebrow">A WORLD OF FINDS</p>
          <h2>Shop by category</h2>

          <nav aria-label="Product categories">
            {categories.map(({ name, icon: Icon, category }) => (
              <Link
                key={name}
                to={`/products?category=${encodeURIComponent(category)}`}
                className="sk-category-link"
              >
                <span className="sk-category-icon">
                  <Icon size={15} strokeWidth={1.5} />
                </span>

                <span>{name}</span>

                <ArrowRight className="sk-category-arrow" size={13} />
              </Link>
            ))}
          </nav>

          <Link to="/products" className="sk-underlined-link">
            Explore everything <ArrowRight size={13} />
          </Link>
        </aside>

        <div className="sk-collection">
          <div className="sk-collection-heading">
            <div>
              <p className="sk-eyebrow">SELECTED FOR YOU</p>
              <h2>The considered collection.</h2>
              <p>Good finds, across every part of life.</p>
            </div>

            <Link to="/products" className="sk-underlined-link">
              View all <ArrowRight size={14} />
            </Link>
          </div>

          {featuredProducts.length > 0 ? (
            <div className="sk-product-grid">
              {featuredProducts.map((product, index) => (
                <div
                  className="sk-product-item"
                  key={product.id ?? product._id ?? index}
                >
                  <ProductCard product={product} />
                </div>
              ))}
            </div>
          ) : (
            <div className="sk-empty-state">
              <h3>Something good is on its way.</h3>
              <p>Your product collection will appear here.</p>

              <Link to="/products" className="sk-button sk-button-dark">
                Browse products <ArrowRight size={15} />
              </Link>
            </div>
          )}

          <Link to="/products" className="sk-mobile-view-all">
            Explore all products <ArrowRight size={14} />
          </Link>
        </div>
      </section>

      {/* Brand statement */}
      <section className="sk-brand-statement">
        <div className="sk-brand-inner">
          <p className="sk-eyebrow">THE SHOPKART APPROACH</p>

          <h2>
            Everyday things.
            <br />A little more extraordinary.
          </h2>

          <p>
            From modern technology and personal style to thoughtful home pieces,
            discover products worth making room for.
          </p>

          <Link to="/products" className="sk-button sk-button-dark">
            Find your next favourite <ArrowRight size={15} />
          </Link>
        </div>
      </section>

      {/* Shopping benefits */}
      <section className="sk-benefits" aria-label="Shopping benefits">
        {benefits.map(({ icon: Icon, title, text }) => (
          <article className="sk-benefit" key={title}>
            <Icon size={23} strokeWidth={1.35} />
            <div>
              <h3>{title}</h3>
              <p>{text}</p>
            </div>
          </article>
        ))}
      </section>
    </main>
  );
}
