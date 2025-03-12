using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Models
{
    public static class CustomSessionManager
    {
        // In-memory session storage (for demonstration purposes)
        private static readonly Dictionary<string, Dictionary<string, string>> SessionStore = new();

        // Create or get session ID from cookies (private method)
        private static string GetOrCreateSessionId(HttpContext context)
        {
            string sessionId = context.Request.Cookies["sessionId"];

            if (string.IsNullOrEmpty(sessionId) || !SessionStore.ContainsKey(sessionId))
            {
                sessionId = Guid.NewGuid().ToString();
                SessionStore[sessionId] = new Dictionary<string, string>();

                // Set sessionId as a cookie
                context.Response.Cookies.Append("sessionId", sessionId, new CookieOptions
                {
                    HttpOnly = true, // Prevents JavaScript access
                    Secure = true,  // Ensure it's only sent over HTTPS
                    SameSite = SameSiteMode.Lax,
                    Path = "/"
                });
            }

            return sessionId;
        }

        // Store a value in session (extension method)
        public static void SetValue<T>(this ISession session, HttpContext context, string key, T value)
        {
            string sessionId = GetOrCreateSessionId(context);

            if (!SessionStore.ContainsKey(sessionId))
            {
                SessionStore[sessionId] = new Dictionary<string, string>();
            }

            SessionStore[sessionId][key] = JsonSerializer.Serialize(value);
        }

        // Retrieve a value from session (extension method)
        public static T? GetValue<T>(this ISession session, HttpContext context, string key)
        {
            string sessionId = GetOrCreateSessionId(context);

            if (SessionStore.ContainsKey(sessionId) && SessionStore[sessionId].TryGetValue(key, out var jsonValue))
            {
                return JsonSerializer.Deserialize<T>(jsonValue);
            }

            return default;
        }
    }
}
