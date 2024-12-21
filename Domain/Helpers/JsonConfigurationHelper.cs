using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace JFiler.Domain.Helpers
{
  public static class JsonConfigurationHelper
  {
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
        jsonObject[prop.Name] = prop.Value.ValueKind switch
        {
          JsonValueKind.Object => JsonDocumentToJsonObject(JsonDocument.Parse(prop.Value.GetRawText())),
          JsonValueKind.String => prop.Value.GetString(),
          _ => prop.Value.ToString()
        };
      }

      return jsonObject;
    }

    private static void UpdateJsonValue(IDictionary<string, object> jsonObject, string[] keys, string value)
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

      //^1 is last element of array ^2 is second to last etc
      current[keys[^1]] = value;
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
