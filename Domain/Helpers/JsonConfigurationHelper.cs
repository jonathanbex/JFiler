using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace JFiler.Domain.Helpers
{
  public static class JsonConfigurationHelper
  {
    public static void UpdateAppSettings(string key, object value)
    {
      const string filePath = "appsettings.json";
      IDictionary<string, object> jsonObject;

      // Ensure the configuration file exists or create a new object
      if (!File.Exists(filePath))
      {
        Console.WriteLine($"Configuration file '{filePath}' does not exist. Creating a new one.");
        jsonObject = new Dictionary<string, object>();
      }
      else
      {
        jsonObject = LoadJsonFile(filePath);
      }

      // Update only the specified section or key of the JSON object
      UpdateJsonValue(jsonObject, key.Split(':'), value);

      // Save updated JSON back to the file
      SaveJsonToFile(filePath, jsonObject);
    }


    public static void UpdateAppSettings(string key, string value)
    {
      const string filePath = "appsettings.json";
      IDictionary<string, object> jsonObject;

      // Ensure the configuration file exists or create a new object
      if (!File.Exists(filePath))
      {
        Console.WriteLine($"Configuration file '{filePath}' does not exist. Creating a new one.");
        jsonObject = new Dictionary<string, object>();
      }
      else
      {
        jsonObject = LoadJsonFile(filePath);
      }

      // Update the JSON object with the provided key and value
      UpdateJsonValue(jsonObject, key.Split(':'), value);

      // Save updated JSON back to the file
      SaveJsonToFile(filePath, jsonObject);
    }

    private static IDictionary<string, object> LoadJsonFile(string filePath)
    {
      try
      {
        var json = File.ReadAllText(filePath);

        // Remove comments and parse the JSON
        var processedJson = RemoveCommentsFromJson(json);
        using var jsonDocument = JsonDocument.Parse(processedJson);

        return JsonDocumentToJsonObject(jsonDocument);
      }
      catch (JsonException)
      {
        Console.WriteLine($"Invalid JSON format in '{filePath}'. Overwriting with a new configuration.");
        return new Dictionary<string, object>();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred while reading the configuration file: {ex.Message}");
        throw;
      }
    }

    private static IDictionary<string, object> JsonDocumentToJsonObject(JsonDocument document)
    {
      var jsonObject = new Dictionary<string, object>();

      foreach (var prop in document.RootElement.EnumerateObject())
      {
        jsonObject[prop.Name] = ConvertJsonElement(prop.Value);
      }

      return jsonObject;
    }

    private static object ConvertJsonElement(JsonElement element)
    {
      return element.ValueKind switch
      {
        JsonValueKind.Object => JsonDocumentToJsonObject(JsonDocument.Parse(element.GetRawText())), // Recursively parse objects
        JsonValueKind.Array => element.EnumerateArray().Select(ConvertJsonElement).ToList(),        // Recursively parse arrays
        JsonValueKind.String => element.GetString(),                                               // Handle strings
        JsonValueKind.Number => element.TryGetInt64(out var l) ? l : element.GetDouble(),          // Handle numbers
        JsonValueKind.True => true,                                                                // Handle boolean true
        JsonValueKind.False => false,                                                              // Handle boolean false
        JsonValueKind.Null => null,                                                                // Handle null values
        _ => element.GetRawText()                                                                  // Fallback for anything unexpected
      };
    }

    private static void UpdateJsonValue(IDictionary<string, object> jsonObject, string[] keys, object value)
    {
      var current = jsonObject;

      for (int i = 0; i < keys.Length - 1; i++)
      {
        if (!current.ContainsKey(keys[i]))
        {
          current[keys[i]] = new Dictionary<string, object>();
        }

        if (current[keys[i]] is IDictionary<string, object> nestedObject)
        {
          current = nestedObject;
        }
        else
        {
          throw new InvalidOperationException($"Cannot update key '{keys[i]}'. It is not an object.");
        }
      }

      // Update the targeted key or section with the new value
      if (value is IDictionary<string, object> dictionaryValue)
      {
        // If the value is a nested object (e.g., for sections)
        current[keys[^1]] = dictionaryValue;
      }
      else if (value is IEnumerable<object> listValue)
      {
        // If the value is an array or list
        current[keys[^1]] = listValue.ToList();
      }
      else
      {
        // For single values (e.g., strings, numbers)
        current[keys[^1]] = value?.ToString();
      }
    }

    private static void SaveJsonToFile(string filePath, IDictionary<string, object> jsonObject)
    {
      try
      {
        var updatedJson = JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, updatedJson);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"An error occurred while writing to the configuration file: {ex.Message}");
        throw;
      }
    }

    public static string RemoveCommentsFromJson(string json)
    {
      var lines = json.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
      return string.Join(Environment.NewLine,
          lines.Where(line => !line.TrimStart().StartsWith("//") && !string.IsNullOrWhiteSpace(line)));
    }
  }


}
