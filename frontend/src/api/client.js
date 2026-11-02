const API_URL = (process.env.REACT_APP_API_URL || "").replace(/\/$/, "");

function getUser() {
  try {
    return JSON.parse(localStorage.getItem("morrow_user") || "null");
  } catch {
    return null;
  }
}

function getToken() {
  return getUser()?.token || null;
}

async function tryRefreshToken() {
  const user = getUser();
  if (!user?.refreshToken) return null;

  try {
    const response = await fetch(`${API_URL}/api/auth/refresh`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
        Accept: "application/json",
      },
      body: JSON.stringify({ refreshToken: user.refreshToken }),
    });

    if (!response.ok) {
      localStorage.removeItem("morrow_user");
      return null;
    }

    const data = await response.json();

    const updated = {
      ...user,
      token: data.token,
      refreshToken: data.refreshToken,
      refreshTokenExpiresAt: data.refreshTokenExpiresAt,
    };

    localStorage.setItem("morrow_user", JSON.stringify(updated));

    return data.token;
  } catch {
    localStorage.removeItem("morrow_user");
    return null;
  }
}

async function request(method, path, body, isRetry = false) {
  const headers = { Accept: "application/json" };
  const token = getToken();

  if (body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`;
  }

  let response;

  try {
    response = await fetch(`${API_URL}/api${path}`, {
      method,
      headers,
      body: body === undefined ? undefined : JSON.stringify(body),
    });
  } catch {
    throw new Error(
      "Cannot connect to the API. Check that your .NET backend is running.",
    );
  }

  if (response.status === 401 && !isRetry) {
    if (method === "POST" && path === "/auth/login") {
      const data = await response.json().catch(() => null);
      throw new Error(data?.message || "Invalid email or password.");
    }

    const newToken = await tryRefreshToken();

    if (newToken) {
      return request(method, path, body, true);
    }

    window.location.href = "/login";
    throw new Error("Your session has expired. Please sign in again.");
  }

  if (response.status === 204) return null;

  const data = await response.json().catch(() => null);

  if (!response.ok) {
    const validationErrors = data?.errors
      ? Object.values(data.errors).flat().join(" ")
      : "";

    if (response.status === 401) {
      throw new Error(
        "Your session is invalid or expired. Please sign in again.",
      );
    }

    throw new Error(
      data?.message ||
        validationErrors ||
        data?.title ||
        `Request failed (${response.status}).`,
    );
  }

  return data?.data ?? data;
}

export const api = {
  get: (path) => request("GET", path),
  post: (path, body) => request("POST", path, body),
  put: (path, body) => request("PUT", path, body),
  delete: (path) => request("DELETE", path),
};
