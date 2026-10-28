import { useState } from "react";
import { login } from "../api/login";
import "../styles/LoginPage.css";
import type { LoginRequest } from "../types/loginRequest";
import type { User } from "../types/user";

function LoginPage({ onSetCurrentUser }: { onSetCurrentUser: (user: User) => void; }) {
    const [userOnLogin, setUserOnLogin] = useState<LoginRequest>({
        username: "",
        password: ""
    });

    const [usernameError, setUsernameError] = useState<string | null>(null);
    const [passwordError, setPasswordError] = useState<string | null>(null);

    return (
        <div className="loginLayout">
            <h1 className="loginTitle">Login</h1>

            <form className="loginForm" onSubmit={async (e) => {
                e.preventDefault();

                if (userOnLogin.username.length === 0 || userOnLogin.username.length > 40) {
                    setUsernameError("Benutzername muss zwischen 1 und 40 Zeichen haben!");
                    return;
                }

                if (userOnLogin.password.length === 0 || userOnLogin.password.length > 40) {
                    setPasswordError("Passwort muss zwischen 1 und 40 Zeichen haben!");
                    return;
                }

                setUsernameError(null);
                setPasswordError(null);

                const user = await login(userOnLogin);

                if (user)
                    onSetCurrentUser(user);

            }}>
                <input value={userOnLogin.username} type="text" placeholder="Benutzername..." onChange={(e) => { setUserOnLogin(prev => ({ ...prev, username: e.target.value })); setUsernameError(null); }} />
                <p className={`loginError ${usernameError ? "visible" : ""}`}>{usernameError}</p>

                <input value={userOnLogin.password} type="password" placeholder="Passwort..." onChange={(e) => { setUserOnLogin(prev => ({ ...prev, password: e.target.value })); setPasswordError(null); }} />
                <p className={`loginError ${passwordError ? "visible" : ""}`}>{passwordError}</p>

                <button className="loginButton" type="submit">Anmelden</button>
            </form>
        </div>
    );
}

export default LoginPage;