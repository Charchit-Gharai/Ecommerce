import React, { useState } from "react";
import { MessageCircle, X, Send, Bot } from "lucide-react";
import "./ShopBot.css";

const ShopBot = () => {
  const [isOpen, setIsOpen] = useState(false);
  const [message, setMessage] = useState("");
  const [messages, setMessages] = useState([
    {
      sender: "bot",
      text: "Hi! 👋 I'm ShopBot, your ShopKart shopping assistant. How can I help you today?",
    },
  ]);

  const addMessage = (sender, text) => {
    setMessages((prev) => [...prev, { sender, text }]);
  };

  const handleSend = (e) => {
    e.preventDefault();
    const text = message.trim();
    if (!text) return;

    addMessage("user", text);
    setMessage("");

    const lower = text.toLowerCase();

    if (
      lower.includes("delivery") ||
      lower.includes("deliver") ||
      lower.includes("shipping")
    ) {
      addMessage(
        "bot",
        "For delivery estimates and shipping inquiries, please contact our support team at support@shopkart.com with your order details.",
      );
    } else if (lower.includes("track") || lower.includes("order status")) {
      addMessage(
        "bot",
        "Visit the Orders page to view your orders. Live tracking requires your order and courier tracking service to be connected.",
      );
    } else if (lower.includes("return") || lower.includes("refund")) {
      addMessage(
        "bot",
        "For return and refund assistance, contact support@shopkart.com.",
      );
    } else if (
      lower.includes("hi") ||
      lower.includes("hello") ||
      lower.includes("hey")
    ) {
      addMessage(
        "bot",
        "Hello! 😊 I can help you with orders, shipping information, and returns.",
      );
    } else {
      addMessage(
        "bot",
        "I'm ShopBot! I can currently help with order tracking information, shipping inquiries, and returns. How can I help?",
      );
    }
  };

  return (
    <div className="shopbot-container">
      {isOpen && (
        <section className="shopbot-window" aria-label="ShopBot chat">
          <header className="shopbot-header">
            <div className="shopbot-avatar">
              <Bot size={24} />
            </div>
            <div className="shopbot-header-info">
              <h3>ShopBot</h3>
              <span>
                <span className="shopbot-status-dot" /> Shopping assistant
              </span>
            </div>
            <button
              type="button"
              className="shopbot-close"
              onClick={() => setIsOpen(false)}
              aria-label="Close ShopBot"
            >
              <X size={21} />
            </button>
          </header>

          <div className="shopbot-messages" aria-live="polite">
            {messages.map((item, index) => (
              <div
                key={`${item.sender}-${index}`}
                className={`shopbot-message-row ${
                  item.sender === "user" ? "user-row" : "bot-row"
                }`}
              >
                {item.sender === "bot" && (
                  <span className="shopbot-small-avatar">
                    <Bot size={15} />
                  </span>
                )}
                <div className={`shopbot-message ${item.sender}`}>
                  {item.text}
                </div>
              </div>
            ))}
          </div>

          <form className="shopbot-input-form" onSubmit={handleSend}>
            <input
              type="text"
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder="Ask ShopBot..."
              aria-label="Type your message"
              required
            />
            <button
              type="submit"
              aria-label="Send message"
              disabled={!message.trim()}
            >
              <Send size={18} />
            </button>
          </form>
        </section>
      )}

      <button
        type="button"
        className={`shopbot-floating-button ${isOpen ? "active" : ""}`}
        onClick={() => setIsOpen((previous) => !previous)}
        aria-label={isOpen ? "Close ShopBot" : "Open ShopBot"}
        aria-expanded={isOpen}
      >
        {isOpen ? <X size={25} /> : <MessageCircle size={27} />}
        {!isOpen && <span className="shopbot-floating-label">ShopBot</span>}
      </button>
    </div>
  );
};

export default ShopBot;
