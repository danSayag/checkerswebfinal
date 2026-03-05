using System.Collections.Concurrent;

namespace CheckersWeb.Services
{
    // connectionId -> username
    public static class ConnectedUsers
    {
        public static ConcurrentDictionary<string, string> Map = new();

        public static IReadOnlyCollection<string> GetOnlineUsernames()
            => Map.Values.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}