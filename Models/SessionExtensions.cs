using System.Reflection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ServerAPI.Models
{
	public static class SessionExtensions
	{
		public static void SetValue<T>(this ISession session, string key, T value)
		{
			session.SetString(key, JsonConvert.SerializeObject(value));
		}
		public static T GetValue<T>(this ISession session, string key)
		{
			var value = session.GetString(key);
			return value == null ? default(T) : JsonConvert.DeserializeObject<T>(value);
		}
	}
}
