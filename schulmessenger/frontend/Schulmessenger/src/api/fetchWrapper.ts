import { getSignalRConnection } from "../services/signalrService";

let refreshPromise: Promise<string | null> | null = null;

async function refreshTokens(apiUrl: string): Promise<string | null> {
  const refreshToken = localStorage.getItem("refreshToken");
  if (!refreshToken) return null;

  try {
    const result = await fetch(apiUrl + "/api/Auth/refresh", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ refreshToken }),
    });

    if (!result.ok) return null;

    const data: { accessToken: string; refreshToken: string } =
      await result.json();

    localStorage.setItem("token", data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);

    return data.accessToken;
  } catch {
    return null;
  }
}

function handleSessionExpired(sessionMessage: string) {
  localStorage.removeItem("token");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("user");

  window.dispatchEvent(new CustomEvent("auth-expired"));

  window.dispatchEvent(
    new CustomEvent("app-error", { detail: sessionMessage }),
  );
}

export async function fetchWrapper(
  endpoint: string,
  options: RequestInit = {},
) {
  try {
    const apiUrl = import.meta.env.VITE_API_URL;
    const token = localStorage.getItem("token");
    const headers = new Headers(options.headers);

    const connection = getSignalRConnection();
    if (connection?.connectionId)
      headers.set("X-Connection-Id", connection.connectionId);

    if (!headers.has("Content-Type") && !(options.body instanceof FormData))
      headers.set("Content-Type", "application/json");

    if (token) headers.set("Authorization", `Bearer ${token}`);

    let res = await fetch(apiUrl + endpoint, { ...options, headers });

    if (res.status === 401 && !endpoint.includes("/api/Auth/login")) {
      const sessionMessage = "Ungültige Session. Bitte erneut anmelden.";

      if (!refreshPromise) {
        refreshPromise = refreshTokens(apiUrl).finally(() => {
          refreshPromise = null;
        });
      }

      const newAccessToken = await refreshPromise;

      if (newAccessToken) {
        headers.set("Authorization", `Bearer ${newAccessToken}`);
        res = await fetch(apiUrl + endpoint, { ...options, headers });
      } else {
        handleSessionExpired(sessionMessage);
        throw new Error(sessionMessage);
      }
    }

    if (!res.ok) {
      let errorMessage = `Fehler: ${res.status}`;

      try {
        const data = await res.json();
        errorMessage = data.detail || data.title || errorMessage;
      } catch {
        //
      }

      window.dispatchEvent(
        new CustomEvent("app-error", { detail: errorMessage }),
      );
      throw new Error(errorMessage);
    }

    if (res.status === 204) return null;

    return await res.json();
  } catch (err: unknown) {
    if (err instanceof TypeError) {
      const networkMsg = "Server nicht erreichbar.";
      window.dispatchEvent(
        new CustomEvent("app-error", { detail: networkMsg }),
      );
      throw new Error(networkMsg, { cause: err });
    }
    throw err;
  }
}
