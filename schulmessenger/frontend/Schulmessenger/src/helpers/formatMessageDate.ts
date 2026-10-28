export function formatMessageDate(date: Date | null): string | null {
    if (!date) return null;

    return date.toLocaleDateString("de-DE", {
        weekday: "long",
        day: "2-digit",
        month: "2-digit",
        year: "numeric"
    });
}