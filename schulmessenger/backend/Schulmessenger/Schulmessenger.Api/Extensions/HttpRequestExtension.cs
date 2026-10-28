namespace Schulmessenger.Api.Extensions;

public static class HttpRequestExtension
{
    public static string GetConnectionId(this HttpRequest request)
    {
        if (request.Headers.TryGetValue("X-Connection-Id", out var connectionId))
            return connectionId!;

        throw new BadHttpRequestException("Keine Websocket-Verbindung aktiv!");
    }
}