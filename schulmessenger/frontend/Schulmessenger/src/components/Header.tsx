import { NavLink } from "react-router-dom";
import "../styles/Header.css";
import type { User } from "../types/user";

function Header({ currentUser, onLogout }: { currentUser: User | null, onLogout: () => void; }) {
    return (
        <header className="headerWrapper">
            <h4>Schulmessenger</h4>

            {currentUser && <p>Angemeldet als: {currentUser.firstName} {currentUser.lastName}
                {currentUser.className &&
                    <span className="userClassBadge">{currentUser.className ? currentUser.className : ""}</span>}
                {currentUser.role === "Employee" &&
                    <span className="userEmployeeBadge">{"Mitarbeiter"}</span>}
            </p>
            }

            <nav className="pageButtons">
                <NavLink
                    to="/chats" className={({ isActive }) => `navBtn ${isActive ? "active" : ""} ${!currentUser ? "disabled" : ""}`}>
                    Chats
                </NavLink>

                {!currentUser && <NavLink
                    to="/" className={({ isActive }) => (isActive ? "navBtn active" : "navBtn")}>
                    Login
                </NavLink>}

                {currentUser && <NavLink
                    to="/" className={({ isActive }) => (isActive ? "navBtn active" : "navBtn")} onClick={() => onLogout()}>
                    Logout
                </NavLink>}
            </nav>
        </header>
    );
}

export default Header;