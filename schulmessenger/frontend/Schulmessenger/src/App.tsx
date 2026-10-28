import ChatPage from "./pages/ChatPage";
import Header from "./components/Header";
import { startConnection, stopConnection } from "./services/signalrService";
import './styles/App.css';
import type { User } from "./types/user";
import { useEffect, useState } from "react";
import GlobalErrorBanner from "./banners/GlobalErrorBanner";
import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import { logout } from "./api/logout";

function App() {
  const [currentUser, setCurrentUser] = useState<User | null>(() => {
    const user = localStorage.getItem("user");
    return user ? JSON.parse(user) : null;
  });

  useEffect(() => {
    if (currentUser)
      startConnection();
  }, [currentUser]);

  useEffect(() => {
    function onAuthExpired() {
      setCurrentUser(null);
      stopConnection();
    }

    window.addEventListener("auth-expired", onAuthExpired);

    return () => window.removeEventListener("auth-expired", onAuthExpired);
  }, []);

  async function handleLogout() {
    setCurrentUser(null);
    await logout();
  }

  return (
    <BrowserRouter>
      <GlobalErrorBanner />
      <Header currentUser={currentUser} onLogout={handleLogout} />
      <Routes>
        <Route path="/" element={currentUser ? <Navigate to="/chats" replace />
          : <LoginPage onSetCurrentUser={(user: User) => setCurrentUser(user)} />} />

        <Route path="/chats" element={currentUser ? <ChatPage currentUser={currentUser} />
          : <Navigate to="/" replace />} />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;