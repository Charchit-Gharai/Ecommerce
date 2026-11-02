import { request } from "./client";

describe("request authentication errors", () => {
  afterEach(() => {
    jest.restoreAllMocks();
  });

  test("does not describe rejected login credentials as an expired session", async () => {
    jest.spyOn(global, "fetch").mockResolvedValue({
      status: 401,
      ok: false,
      json: async () => ({ title: "Unauthorized" }),
    });

    await expect(
      request("POST", "/auth/login", {
        email: "customer@example.com",
        password: "incorrect",
      }),
    ).rejects.toThrow("Invalid email or password.");
  });

  test("describes a 401 from an authenticated request as an expired session", async () => {
    localStorage.setItem(
      "morrow_user",
      JSON.stringify({ token: "test-token", role: "Customer" }),
    );
    jest.spyOn(global, "fetch").mockResolvedValue({
      status: 401,
      ok: false,
      json: async () => ({ title: "Unauthorized" }),
    });

    await expect(request("GET", "/cart")).rejects.toThrow(
      "Your session is invalid or expired. Please sign in again.",
    );
  });
});
