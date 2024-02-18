using HtmlTree;
using System.Text.Json;
using System.Text.RegularExpressions;

public class HtmlHelper
{
    private readonly static HtmlHelper _instance = new HtmlHelper();
    
    public static HtmlHelper Instance => _instance;
    
    public string[] AllHtmlTags { get;  }
    
    public string[] SelfClosingHtmlTags { get;  }

    private HtmlHelper()
    {
        AllHtmlTags = LoadTagsFromJson("seed/AllTags.json");
        SelfClosingHtmlTags = LoadTagsFromJson("seed/SelfClosingTags.json");
    }

    private string[] LoadTagsFromJson(string jsonFilePath)
    {
        try
        {
            using (StreamReader reader = new StreamReader(jsonFilePath))
            {
                string jsonString = reader.ReadToEnd();
                return JsonSerializer.Deserialize<string[]>(jsonString);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading: {ex.Message}");
            return null;
        }
    }

    public string CleanHtml(string html)
    {
        return Regex.Replace(html, "\\s", "");
    }

    public List<string> ExtractHtmlLines(string cleanHtml)
    {
        return new Regex("<(.*?)>").Split(cleanHtml).Where(s => s.Length > 0).ToList();
    }

    public HtmlElement ParseHtml(List<string> htmlLines)
    {
        HtmlElement rootElement = new HtmlElement("root");
        HtmlElement currentElement = rootElement;

        foreach (var line in htmlLines)
        {
            var parts = line.Split(new char[] { ' ' }, 2);
            var tag = parts[0];

            if (tag.StartsWith("html/"))
            {
                break;
            }

            if (currentElement != null && tag.StartsWith("/"))
            {
                currentElement = currentElement.Parent;
            }
            else if (AllHtmlTags.Contains(tag))
            {
                var newElement = new HtmlElement(tag, currentElement);

                var attributes = parts.Length > 1 ? parts[1].Split(' ') : Array.Empty<string>();
                foreach (var attribute in attributes)
                {
                    var parts1 = attribute.Split('=');

                    if (parts1.Length == 2)
                    {
                        var name = parts1[0].Trim();
                        var value = parts1[1].Trim();

                        if (name.Equals("class"))
                        {
                            newElement.Classes.Add(value);
                        }
                        else
                        {
                            newElement.Attributes.Add(name);
                        }
                    }
                }

                if (tag.EndsWith("/") || SelfClosingHtmlTags.Contains(tag) || line.EndsWith("/>"))
                {
                    newElement.InnerHtml = "";
                    currentElement?.Children.Add(newElement);
                }
                else
                {
                    newElement.InnerHtml = parts.Length > 1 ? parts[1] : "";
                    currentElement?.Children.Add(newElement);
                    currentElement = newElement;
                }
            }
            else
            {
                if (currentElement?.Children.Count > 0)
                {
                    currentElement.Children.Last().InnerHtml += line;
                }

            }
        }

        return rootElement;
    }
}
