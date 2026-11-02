import { Routes, Route, Navigate, useLocation } from "react-router-dom";

import Navbar from "./components/Navbar";
import Footer from "./components/Footer";
import AdminRoute from "./components/AdminRoute";
import ShopBot from "./components/ShopBot";

import Home from "./pages/Home";
import Products from "./pages/Products";
import ProductDetails from "./pages/ProductDetails";

import Login from "./pages/Login";
import Register from "./pages/Register";
import Account from "./pages/Account";

import Cart from "./pages/Cart";
import Checkout from "./pages/Checkout";

import Wishlist from "./pages/Wishlist";
import Orders from "./pages/Orders";

import Admin from "./pages/Admin";
import AdminLogin from "./pages/AdminLogin";

function NotFound() {
  return (
    <main className="container placeholder-page">
      <p className="eyebrow">404 ERROR</p>
      <h1>Page not found</h1>
      <p>The page you are looking for does not exist or may have been moved.</p>
    </main>
  );
}

export default function App() {
  const location = useLocation();

  const isAdminArea = location.pathname.startsWith("/admin");

  return (
    <>
      {/* Customer navbar */}
      {!isAdminArea && <Navbar />}

      {/* Application routes */}
      <Routes>
        {/* Customer storefront */}
        <Route path="/" element={<Home />} />

        {/* Products */}
        <Route path="/products" element={<Products />} />
        <Route path="/products/:id" element={<ProductDetails />} />

        {/* Customer authentication and account */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/account" element={<Account />} />

        {/* Shopping cart and checkout */}
        <Route path="/cart" element={<Cart />} />
        <Route path="/checkout" element={<Checkout />} />

        {/* Wishlist and orders */}
        <Route path="/wishlist" element={<Wishlist />} />
        <Route path="/orders" element={<Orders />} />

        {/* Admin login */}
        <Route path="/admin-login" element={<AdminLogin />} />

        {/* Protected admin dashboard */}
        <Route element={<AdminRoute />}>
          <Route path="/admin" element={<Admin />} />
        </Route>

        {/* Redirect other admin paths to the dashboard guard */}
        <Route path="/admin/*" element={<Navigate to="/admin" replace />} />

        {/* Invalid routes */}
        <Route path="*" element={<NotFound />} />
      </Routes>

      {/* ShopBot: visible throughout the customer storefront */}
      {!isAdminArea && <ShopBot />}

      {/* Customer footer */}
      {!isAdminArea && <Footer />}
    </>
  );
}
