using System;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Models
{
    public static class SessionExtensions
    {
        // Trådsäker session-lagring
        private static readonly ConcurrentDictionary<string, ConcurrentDictionary<string, string>> SessionStore = new();

        // Skapa eller hämta sessionId från cookies
        private static string GetOrCreateSessionId(HttpContext context)
        {
            // Hämta befintligt sessionId från cookien
            string sessionId = context.Request.Cookies["sessionId"];

            if (string.IsNullOrEmpty(sessionId))
            {
                // Om inget sessionId finns, skapa ett nytt
                sessionId = Guid.NewGuid().ToString();
            }

            // Om sessionId inte finns i SessionStore, skapa en ny Dictionary
            SessionStore.GetOrAdd(sessionId, _ => new ConcurrentDictionary<string, string>());

            // Sätt sessionId som en cookie om det är nytt eller saknas i klienten
            if (string.IsNullOrEmpty(context.Request.Cookies["sessionId"]))
            {
                context.Response.Cookies.Append("sessionId", sessionId, new CookieOptions
                {
                    HttpOnly = true, // Förhindrar åtkomst från JavaScript
                    //Secure = true,  // Använd detta om du kör HTTPS
                    SameSite = SameSiteMode.None,
                    Path = "/"
                });
            }

            return sessionId;
        }

        // Lagra ett värde i sessionen (trådsäker)
        public static void SetValue<T>(this ISession session, HttpContext context, string key, T value)
        {
            string sessionId = GetOrCreateSessionId(context);

            // Hämta eller skapa en ny session för detta sessionId
            var sessionData = SessionStore.GetOrAdd(sessionId, _ => new ConcurrentDictionary<string, string>());

            // Spara värdet i sessionen
            sessionData[key] = JsonSerializer.Serialize(value);
        }

        // Hämta ett värde från sessionen (trådsäker)
        public static T? GetValue<T>(this ISession session, HttpContext context, string key)
        {
            string sessionId = GetOrCreateSessionId(context);

            // Hämta värdet om det finns
            if (SessionStore.TryGetValue(sessionId, out var sessionData) && sessionData.TryGetValue(key, out var jsonValue))
            {
                return JsonSerializer.Deserialize<T>(jsonValue);
            }

            return default;
        }
    }
}
