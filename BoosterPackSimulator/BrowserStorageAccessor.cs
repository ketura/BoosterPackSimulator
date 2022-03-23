using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Newtonsoft.Json;

namespace BoosterPackSimulator
{
    public class BrowserStorageAccessor : ComponentBase
    {
        private readonly IJSRuntime JS;

        public BrowserStorageAccessor(IJSRuntime js)
        {
            this.JS = js;
        }

        public async Task SaveBS(string key, object value)
        {
            await JS.InvokeVoidAsync("localStorage.setItem", key, JsonConvert.SerializeObject(value));
        }

        public async Task<bool> HasBS(string key)
        {
            return await ReadBS<object>(key) != null;
        }

        public async Task<string?> ReadBS(string key)
        {
            return await ReadBS<string>(key);
        }

        public async Task<T?> ReadBS<T>(string? key)
        {
            string? json = await JS.InvokeAsync<string>("localStorage.getItem", key);

            return JsonConvert.DeserializeObject<T>(json ?? "");
        }

    }
}
