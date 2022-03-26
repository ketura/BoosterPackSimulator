﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using Newtonsoft.Json;

using static System.Net.WebRequestMethods;

namespace BoosterPackSimulator
{
    public class BrowserStorageAccessor : ComponentBase
    {
        private readonly IJSRuntime JS;
        private readonly HttpClient Http;

        public BrowserStorageAccessor(IJSRuntime js, HttpClient http)
        {
            this.JS = js;
            this.Http = http;
        }

        public async Task SaveBS(string key, object value)
        {
            await JS.InvokeVoidAsync("localStorage.setItem", key, JsonConvert.SerializeObject(value));
        }

        public async Task SaveBS(object value)
        {
            await SaveBS(value.GetType().Name, value);
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

        public async Task<T?> ReadBS<T>()
        {
            return await ReadBS<T>(typeof(T).Name);
        }

        public async Task<T> ReadOrCreateBS<T>()
            where T : class, new()
        {
            T? item = await ReadBS<T>();

            if (item == null)
            {
                if(typeof(T) == typeof(GameDefinition))
                {
                    (_, _, var gamedef) = await LoadGameDefinitions();
                    item = gamedef as T;
                }
                else
                {
                    item = new T();
                }
                
                await SaveBS(item);
            }

            return item;
        }

        //Default list of GameDefinition json files that are loaded each startup.
        private List<string> Definitions = new List<string>
        {
            "data/set01.json"
        };

        public async Task<(bool error, string result, GameDefinition gamedef)> LoadGameDefinitions()
        {
            return await LoadGameDefinitions(DateTime.MinValue);
        }
        public async Task<(bool error, string result, GameDefinition gamedef)> LoadGameDefinitions(DateTime LatestUpdate)
        {
            Console.WriteLine("LoadGameDefinition request.");
            bool error = false;
            string result = "Loading Complete";
            GameDefinition gamedef;

            gamedef = await ReadBS<GameDefinition>() ?? new GameDefinition();
            if (gamedef == null || gamedef.LastUpdated == DateTime.MinValue || gamedef.LastUpdated < LatestUpdate)
            {
                Console.WriteLine("LoadGameDefinition full load.");
                gamedef = new GameDefinition();
                try
                {
                    Http.DefaultRequestHeaders.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue() { NoCache = true };

                    foreach (var filename in Definitions)
                    {
                        var str = await Http.GetStringAsync(filename);
                        var def = JsonConvert.DeserializeObject<GameDefinition>(str);

                        if (def == null)
                        {
                            result = "ERROR loading definition from server!";
                            break;
                        }

                        gamedef.InsertDefinition(def);
                    }

                    ProcessGameDefinition(gamedef);

                    await SaveBS(gamedef);
                }
                catch (Exception ex)
                {
                    result = $"ERROR processing game definitions!\n\n{ex}";
                }
            }
            else
            {
                result = "No new updates!  Game definitions are current.";
            }

            return (error, result, gamedef);
        }

        private void ProcessGameDefinition(GameDefinition gamedef)
        {
            foreach(var set in gamedef.Sets)
            {
                var Case = gamedef.CaseDefinitions[set.SetNum];
                Case.SetName = set.Name;
                Case.DisplayBox.SetName = set.Name;
                Case.DisplayBox.BoosterDefinition.SetName = set.Name;
            }
        }

    }
}
