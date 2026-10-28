import { stopConnection } from "../services/signalrService";

export async function logout() {
  const apiUrl = import.meta.env.VITE_API_URL;
  const refreshToken = localStorage.getItem("refreshToken");

  try {
    if (refreshToken) {
      await fetch(apiUrl + "/api/Auth/revoke", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ refreshToken }),
      });
    }
  } catch {
    //
  } finally {
    await stopConnection();

    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
  }
}