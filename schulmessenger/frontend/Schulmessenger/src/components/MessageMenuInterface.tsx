import { useEffect, useRef } from "react";
import "../styles/MessageMenuInterface.css";

export function MessageMenuInterface({ onDelete, onClose }: { onDelete: () => void, onClose: () => void; }) {
    const menuWrapperRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        function handleClick(event: MouseEvent) {
            if (menuWrapperRef.current && !menuWrapperRef.current.contains(event.target as Node))
                onClose();
        }

        document.addEventListener("pointerdown", handleClick);

        return () => document.removeEventListener("pointerdown", handleClick);
    }, [onClose]);

    return (
        <div ref={menuWrapperRef} className="messageMenuWrapper">
            <button className="messageMenuDeleteButton" type="button" onClick={onDelete}>Löschen</button>
        </div>
    );
}

export default MessageMenuInterface;