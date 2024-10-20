﻿using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace CachelessMigrationTests
{
	public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
    }
}
