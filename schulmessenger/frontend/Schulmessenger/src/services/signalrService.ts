import * as signalR from "@microsoft/signalr";

let connection: signalR.HubConnection | null = null;

export function getSignalRConnection() {
  if (!connection) {
    const apiUrl = import.meta.env.VITE_API_URL;
    connection = new signalR.HubConnectionBuilder()
      .withUrl(apiUrl + "/hubs/schulmessenger", {
        accessTokenFactory: () => localStorage.getItem("token") || "",
      })
      .withAutomaticReconnect()
      .build();
  }
  return connection;
}

export async function startConnection() {
  const conn: signalR.HubConnection | null = getSignalRConnection();
  if (conn.state === signalR.HubConnectionState.Disconnected) {
    try {
      await conn.start();
    } catch (err) {
      console.error("Fehler", err);
    }
  }
}

export async function stopConnection() {
  if (connection) {
    try {
      await connection.stop();
    } catch {
      //
    } finally {
      connection = null;
    }
  }
}

export async function sendMessage(
  chatId: string,
  content: string,
  tempId: string,
) {
  const conn = getSignalRConnection();

  try {
    await conn.invoke("SendMessage", chatId, content, tempId);
  } catch (err) {
    console.error("Fehler beim Senden der Nachricht!", err);
  }
}

export async function joinChat(chatId: string) {
  const conn = getSignalRConnection();

  try {
    await conn.invoke("JoinChat", chatId);
  } catch (err) {
    console.error("Fehler beim Beitreten des Chats!", err);
  }
}

export async function leaveChat(chatId: string) {
  const conn = getSignalRConnection();

  try {
    await conn.invoke("LeaveChat", chatId);
  } catch (err) {
    console.error("Fehler beim Verlassen des Chats!", err);
  }
}
