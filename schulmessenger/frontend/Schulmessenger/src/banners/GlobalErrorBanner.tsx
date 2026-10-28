import { useEffect, useState } from "react";
import "../styles/GlobalErrorBanner.css";

function GlobalErrorBanner() {
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    useEffect(() => {
        function handleErrorEvent(event: Event) {
            const customEvent = event as CustomEvent<string>;
            setErrorMessage(customEvent.detail);
        }

        window.addEventListener("app-error", handleErrorEvent);
        return () => window.removeEventListener("app-error", handleErrorEvent);
    }, []);

    useEffect(() => {
        if (!errorMessage)
            return;

        const timer = setTimeout(() => {
            setErrorMessage(null);
        }, 5000);

        return () => clearTimeout(timer);
    }, [errorMessage]);

    if (!errorMessage) return null;

    return (
        <div className="globalErrorBanner">
            <div className="errorContent">
                <p>{errorMessage}</p>
            </div>
            <button type="button" onClick={() => setErrorMessage(null)}>✕</button>
        </div>
    );
}

export default GlobalErrorBanner;