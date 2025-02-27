using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

public class LocalizationService
{

    public event Action? OnLanguageChanged;
    private readonly HttpClient _http;
    private readonly NavigationManager _navigationManager;
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private string _currentLanguage = "en";

    public LocalizationService(HttpClient http, NavigationManager navigationManager)
    {
        _http = http;
        _navigationManager = navigationManager;
        _ = LoadTranslationsAsync();
    }

    public async Task LoadTranslationsAsync()
    {
        var languages = new[] { "en", "es" };

        foreach (var lang in languages)
        {
            var filePath = $"{_navigationManager.BaseUri}locales/{lang}.json";
            try
            {
                // Console.WriteLine($"Loading translations for: {lang}");
                var json = await _http.GetStringAsync(filePath);
                var translations = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
                _translations[lang] = FlattenJson(translations);
                //Console.WriteLine($"Loaded {lang} translations: {string.Join(", ", _translations[lang].Keys)}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load {lang} translations: {ex.Message}");
            }
        }
    }

    private Dictionary<string, string> FlattenJson(Dictionary<string, object> json, string prefix = "")
    {
        var flatDictionary = new Dictionary<string, string>();

        foreach (var kvp in json)
        {
            var key = string.IsNullOrEmpty(prefix) ? kvp.Key : $"{prefix}.{kvp.Key}";

            if (kvp.Value is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                var nestedDict = JsonSerializer.Deserialize<Dictionary<string, object>>(element.GetRawText());
                var nestedFlat = FlattenJson(nestedDict, key);
                foreach (var nestedKvp in nestedFlat)
                {
                    flatDictionary[nestedKvp.Key] = nestedKvp.Value;
                }
            }
            else
            {
                flatDictionary[key] = kvp.Value.ToString();
            }
        }
        return flatDictionary;
    }

    public void SetLanguage(string language)
    {
        if (_translations.ContainsKey(language))
        {
            _currentLanguage = language;
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(language);
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(language);
            OnLanguageChanged?.Invoke();
        }
    }

    public string Translate(string key)
    {
        return _translations.ContainsKey(_currentLanguage) && _translations[_currentLanguage].ContainsKey(key)
        ? _translations[_currentLanguage][key]
        : key;
    }

    public string CurrentLanguage => _currentLanguage;

}

    // private void LoadTranslations()
    // {
    //     var languages = new[] { "en", "es" };

    //     foreach (var lang in languages)
    //     {
    //         var filePath = Path.Combine(_env.WebRootPath, "locales", $"{lang}.json");
    //         if (File.Exists(filePath))
    //         {
    //             var json = File.ReadAllText(filePath);
    //             var translations = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
    //             _translations[lang] = FlattenJson(translations);

    //             Console.WriteLine($"Loaded {lang} translations: " + string.Join(", ", _translations[lang].Keys));
    //         }
    //         else
    //         {
    //             Console.WriteLine($"Translation file not found: {filePath}");
    //         }
    //     }
    // }
