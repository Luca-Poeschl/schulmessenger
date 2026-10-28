import { useEffect, useRef, useState, type KeyboardEvent } from "react";
import "../styles/MessageInput.css";

function MessageInput({ onSendMessage }: { onSendMessage: (content: string) => void; }) {
    const [content, setContent] = useState<string>("");
    const [error, setError] = useState<string | null>(null);
    const textAreaRef = useRef<HTMLTextAreaElement>(null);

    useEffect(() => {
        if (!error) return;

        function handleClick() {
            setError(null);
        }

        document.addEventListener("pointerdown", handleClick);

        return () => document.removeEventListener("pointerdown", handleClick);
    }, [error]);

    function onSubmitMessage() {
        const trimmedContent = content.trim();
        if (trimmedContent === "") {
            setError("Die Eingabe darf nicht leer sein!");
            return;
        }

        if (trimmedContent.length > 4000) {
            setError("Die Eingabe darf maximal 4000 Zeichen betragen!");
            return;
        }

        setError(null);
        onSendMessage(trimmedContent);
        setContent("");

        if (textAreaRef.current)
            textAreaRef.current.style.height = "auto";
    }

    function handleKeyDown(e: KeyboardEvent<HTMLTextAreaElement>) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            onSubmitMessage();
        }
    }

    function handleSubmit(e: React.SubmitEvent<HTMLFormElement>) {
        e.preventDefault();
        onSubmitMessage();
    }

    function handleInput(e: React.ChangeEvent<HTMLTextAreaElement>) {
        setContent(e.target.value);
        setError(null);

        if (textAreaRef.current) {
            textAreaRef.current.style.height = "auto";
            textAreaRef.current.style.height = `${Math.min(textAreaRef.current.scrollHeight, 120)}px`;
        }
    }

    return (
        <form className="messageInputForm" onSubmit={handleSubmit}>
            {error && <p className="errorMessage">{error}</p>}
            <textarea ref={textAreaRef} value={content}
                onChange={handleInput}
                onKeyDown={handleKeyDown}
                className="messageInputField" placeholder="Deine Nachricht..." ></textarea>
            <button type="submit" className="messageSendButton">Senden</button>
        </form>
    );
};

export default MessageInput;