import { Link, useNavigate } from "react-router-dom";
import { useStore } from "../context/StoreContext";
import { Package, Heart, ShoppingBag, LogOut, UserRound } from "lucide-react";

export default function Account() {
  const store = useStore();
  const navigate = useNavigate();

  const user = store?.user;
  const orders = store?.orders || [];
  const wishlist = store?.wishlist || [];

  function handleLogout() {
    if (typeof store?.signOut === "function") {
      store.signOut();
    }

    navigate("/login", { replace: true });
  }

  if (!user) {
    return (
      <main className="container account-page">
        <section className="account-welcome">
          <p className="eyebrow">YOUR MORROW ACCOUNT</p>
          <h1>Welcome</h1>
          <p>Sign in to view your account information and orders.</p>

          <div className="account-actions">
            <Link to="/login" className="button button-dark">
              Sign in
            </Link>
            <Link to="/register" className="button button-outline">
              Create account
            </Link>
          </div>
        </section>
      </main>
    );
  }

  const displayName =
    user.name || user.fullName || user.username || user.email || "Customer";

  return (
    <main className="container account-page">
      <section className="account-welcome">
        <p className="eyebrow">YOUR MORROW ACCOUNT</p>
        <h1>Hello, {displayName}</h1>
        <p>Manage your account and keep track of your shopping.</p>
      </section>

      <div className="account-profile">
        <div className="account-avatar">
          <UserRound size={28} />
        </div>

        <div className="account-profile-info">
          <h2>{displayName}</h2>
          <p>{user.email || "Email not available"}</p>
          {user.role && <span className="account-role">{user.role}</span>}
        </div>

        <button
          type="button"
          className="button button-outline"
          onClick={handleLogout}
        >
          <LogOut size={16} />
          Sign out
        </button>
      </div>

      <section className="account-links">
        <Link to="/orders" className="account-link-card">
          <Package size={23} />
          <div>
            <h3>My orders</h3>
            <p>{orders.length} order(s)</p>
          </div>
        </Link>

        <Link to="/wishlist" className="account-link-card">
          <Heart size={23} />
          <div>
            <h3>My wishlist</h3>
            <p>{wishlist.length} saved item(s)</p>
          </div>
        </Link>

        <Link to="/cart" className="account-link-card">
          <ShoppingBag size={23} />
          <div>
            <h3>Shopping bag</h3>
            <p>Review items in your cart</p>
          </div>
        </Link>
      </section>
    </main>
  );
}
