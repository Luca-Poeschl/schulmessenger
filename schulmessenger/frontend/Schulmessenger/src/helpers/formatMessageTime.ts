export function formatMessageTime(dateString: string | null): string | null {
    if(!dateString) return null;

    const date: Date = new Date(dateString);

    return date.toLocaleTimeString("de-DE", {
        hour: "2-digit",
        minute: "2-digit"
    });
}