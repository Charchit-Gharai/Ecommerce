import { Link } from "react-router-dom";
import { ArrowUpRight, Camera, Mail } from "lucide-react";
import "./Footer.css";

export default function Footer() {
  return (
    <footer className="site-footer">
      <div className="footer-main container">
        <div className="footer-brand">
          <Link to="/" className="footer-logo">
            Iris<span>.</span>
          </Link>

          <p>
            Useful things, considered carefully. A little inspiration for
            everyday living.
          </p>

          <a className="footer-email" href="mailto:support@iris.com">
            <Mail size={15} />
            support@iris.com
          </a>
        </div>

        <div className="footer-column">
          <h3>Explore</h3>
          <Link to="/products">Shop all</Link>
          <Link to="/wishlist">Your wishlist</Link>
          <Link to="/cart">Shopping bag</Link>
        </div>

        <div className="footer-column">
          <h3>Your account</h3>
          <Link to="/account">Account details</Link>
          <Link to="/orders">Order history</Link>
          <Link to="/login">Sign in</Link>
        </div>
      </div>

      <div className="footer-bottom container">
        <p>© {new Date().getFullYear()} Iris. Made for everyday living.</p>

        <a
          href="https://instagram.com"
          target="_blank"
          rel="noreferrer"
          aria-label="Instagram"
        >
          Instagram <Camera size={14} />
        </a>

        <Link to="/products">
          Back to shopping <ArrowUpRight size={14} />
        </Link>
      </div>
    </footer>
  );
}
