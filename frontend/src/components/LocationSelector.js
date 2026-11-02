import React, { useState } from "react";
import { MapPin, Navigation, X, Search, LoaderCircle } from "lucide-react";
import "./LocationSelector.css";

const LocationSelector = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [pinCode, setPinCode] = useState(
    localStorage.getItem("shopease_pin_code") || "",
  );
  const [locationName, setLocationName] = useState(
    localStorage.getItem("shopease_location_name") || "",
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const saveLocation = (pin, name) => {
    const cleanPin = String(pin).replace(/\D/g, "");

    if (!/^\d{6}$/.test(cleanPin)) {
      setError("Please enter a valid 6-digit Indian PIN code.");
      return;
    }

    localStorage.setItem("shopease_pin_code", cleanPin);
    localStorage.setItem("shopease_location_name", name || "Your location");

    setPinCode(cleanPin);
    setLocationName(name || "Your location");
    setError("");
    setIsOpen(false);
  };

  const detectLocation = () => {
    setError("");

    if (!navigator.geolocation) {
      setError("Location is not supported by your browser.");
      return;
    }

    setLoading(true);

    navigator.geolocation.getCurrentPosition(
      async ({ coords }) => {
        try {
          const url = new URL("https://nominatim.openstreetmap.org/reverse");

          url.searchParams.set("format", "jsonv2");
          url.searchParams.set("lat", coords.latitude);
          url.searchParams.set("lon", coords.longitude);
          url.searchParams.set("addressdetails", "1");

          const response = await fetch(url.toString());

          if (!response.ok) {
            throw new Error("Address lookup failed");
          }

          const data = await response.json();
          const address = data.address || {};

          const pin = String(address.postcode || "").replace(/\D/g, "");
          const city =
            address.city ||
            address.town ||
            address.village ||
            address.suburb ||
            address.county ||
            "";

          if (!/^\d{6}$/.test(pin)) {
            setError(
              "Could not find a valid PIN code. Please enter it manually.",
            );
            return;
          }

          saveLocation(pin, city || "Detected location");
        } catch {
          setError(
            "Unable to retrieve your address. Please enter your PIN code manually.",
          );
        } finally {
          setLoading(false);
        }
      },
      (geoError) => {
        setLoading(false);

        setError(
          geoError.code === 1
            ? "Location permission denied. Please allow access or enter your PIN code."
            : "Unable to detect your location. Please enter your PIN code.",
        );
      },
      { timeout: 10000, maximumAge: 60000 },
    );
  };

  return (
    <div className="location-selector">
      <button
        type="button"
        className="location-trigger"
        onClick={() => {
          setError("");
          setIsOpen((previous) => !previous);
        }}
        aria-expanded={isOpen}
        aria-label="Choose delivery location"
      >
        <MapPin size={20} />

        <span className="location-trigger-text">
          <small>Deliver to</small>
          <strong>{pinCode || "Select location"}</strong>
        </span>
      </button>

      {isOpen && (
        <>
          <button
            type="button"
            className="location-backdrop"
            aria-label="Close location selector"
            onClick={() => setIsOpen(false)}
          />

          <section className="location-popup">
            <div className="location-popup-header">
              <div>
                <h3>Choose your location</h3>
                <p>Check product availability at your address.</p>
              </div>

              <button
                type="button"
                className="location-close"
                onClick={() => setIsOpen(false)}
                aria-label="Close"
              >
                <X size={20} />
              </button>
            </div>

            <button
              type="button"
              className="detect-location-button"
              onClick={detectLocation}
              disabled={loading}
            >
              {loading ? (
                <LoaderCircle size={18} className="location-spinner" />
              ) : (
                <Navigation size={18} />
              )}

              {loading ? "Detecting location..." : "Use my current location"}
            </button>

            <div className="location-divider">
              <span>OR ENTER PIN CODE</span>
            </div>

            <form
              className="location-pin-form"
              onSubmit={(event) => {
                event.preventDefault();
                saveLocation(pinCode, "Selected location");
              }}
            >
              <input
                type="text"
                value={pinCode}
                onChange={(event) => {
                  setPinCode(event.target.value.replace(/\D/g, "").slice(0, 6));
                  setError("");
                }}
                placeholder="Enter 6-digit PIN code"
                inputMode="numeric"
                maxLength={6}
                aria-label="Delivery PIN code"
                required
              />

              <button
                type="submit"
                disabled={!/^\d{6}$/.test(pinCode)}
                aria-label="Save PIN code"
              >
                <Search size={18} />
              </button>
            </form>

            {error && (
              <p className="location-error" role="alert">
                {error}
              </p>
            )}

            {locationName && (
              <p className="location-current">
                Current location: <strong>{locationName}</strong>
              </p>
            )}

            <p className="location-note">
              Your location is used to select a delivery PIN code.
            </p>
          </section>
        </>
      )}
    </div>
  );
};

export default LocationSelector;
