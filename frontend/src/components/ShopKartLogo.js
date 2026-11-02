import { Link } from "react-router-dom";
import "./ShopKartLogo.css";

export default function ShopKartLogo({ light = false, footer = false }) {
  return (
    <Link
      to="/"
      className={`shopkart-logo-link${light ? " shopkart-logo-light" : ""}${
        footer ? " shopkart-logo-footer" : ""
      }`}
      aria-label="ShopKart home"
    >
      <img
        src="/shopkart-logo.png"
        alt="IRIS"
        className="shopkart-logo-image"
      />
    </Link>
  );
}
