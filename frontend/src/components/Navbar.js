import { Link, NavLink } from "react-router-dom";
import ShopKartLogo from "./ShopKartLogo";
import {
  ShoppingBag,
  Heart,
  UserRound,
  Search,
  ArrowUpRight,
} from "lucide-react";

import { useStore } from "../context/StoreContext";
import LocationSelector from "./LocationSelector";

export default function Navbar() {
  const store = useStore();

  const cartCount = (store?.cart || []).reduce(
    (total, item) => total + (Number(item.quantity) || 1),
    0,
  );

  const wishlistCount = (store?.wishlist || []).length;

  const displayName =
    store?.user?.name ||
    store?.user?.fullName ||
    store?.user?.email?.split("@")[0];

  return (
    <header className="site-header">
      <div className="announcement">
        A considered collection for everyday living
      </div>

      <div className="navbar container">
        <ShopKartLogo />

        <nav className="nav-links" aria-label="Main navigation">
          <NavLink to="/" end>
            Home
          </NavLink>
          <NavLink to="/products">Shop all</NavLink>
          <NavLink to="/orders">Orders</NavLink>
        </nav>

        <div className="nav-actions">
          <LocationSelector />

          <Link
            to="/products"
            className="icon-link"
            aria-label="Explore products"
            title="Explore products"
          >
            <Search size={19} />
          </Link>

          <Link
            to="/wishlist"
            className="icon-link"
            aria-label={`Wishlist, ${wishlistCount} items`}
          >
            <Heart size={19} />
            {wishlistCount > 0 && (
              <span className="count">{wishlistCount}</span>
            )}
          </Link>

          <Link
            to="/account"
            className="icon-link"
            aria-label="My account"
            title={displayName || "My account"}
          >
            <UserRound size={19} />
          </Link>

          <Link
            to="/cart"
            className="icon-link"
            aria-label={`Shopping bag, ${cartCount} items`}
          >
            <ShoppingBag size={19} />
            {cartCount > 0 && <span className="count">{cartCount}</span>}
          </Link>
        </div>
      </div>

      {displayName && (
        <div className="container nav-greeting">
          <span>
            Welcome back, <strong>{displayName}</strong>
          </span>
          <Link to="/account" aria-label="View account">
            Account <ArrowUpRight size={13} />
          </Link>
        </div>
      )}
    </header>
  );
}
