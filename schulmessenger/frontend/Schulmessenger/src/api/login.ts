import type { LoginResponse } from "../types/loginReponse";
import type { LoginRequest } from "../types/loginRequest";
import type { User } from "../types/user";
import { fetchWrapper } from "./fetchWrapper";

export async function login(userOnLogin: LoginRequest) {
    try {
        const data: LoginResponse = await fetchWrapper("/api/Auth/login", {
            method: "POST",
            body: JSON.stringify(userOnLogin)
        });

        if (data?.accessToken) {
            localStorage.setItem("token", data.accessToken);
            localStorage.setItem("refreshToken", data.refreshToken);

            const user: User = {
                userId: data.userId,
                firstName: data.firstName,
                lastName: data.lastName,
                role: data.role,
                className: data.className ?? null
            };

            localStorage.setItem("user", JSON.stringify(user));
            return user;
        }
    }
    catch {
        // Fehlerbehandlung in fetchWrapper
    }

    return null;
}