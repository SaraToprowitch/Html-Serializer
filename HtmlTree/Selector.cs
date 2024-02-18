using HtmlTree;
using System.Text.RegularExpressions;

public class Selector
{
    public string TagName { get; set; }

    public string Id { get; set; }

    public List<string> Classes { get; set; }
    
    public Selector Parent { get; set; }
    
    public Selector Child { get; set; }

    public Selector()
    {
        Classes = new List<string>();
    }

    public bool Matches(HtmlElement element)
    {
        if (TagName != null && TagName != element.Name)
        {
            return false;
        }

        if (Id != null && Id != element.Id)
        {
            return false;
        }

        if (Classes != null)
        {
            return element.Classes.All(classes => Classes.Contains(classes));
        }

        return true;
    }

    public static Selector ConvertQueryStringToSelector(string queryString)
    {
        Selector rootSelector = new Selector();
        Selector currentSelector = rootSelector;

        string[] parts = queryString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (string part in parts)
        {
            if (part.StartsWith("#"))
            {
                currentSelector.Id = part.Substring(1);
                continue;
            }
            else if (part.StartsWith("."))
            {
                currentSelector.Classes.Add(part.Substring(1));
                continue;
            }
            else
            {
                if (!IsValidTagName(part))
                {
                    Console.WriteLine($"Invalid tag name: {part}");
                }

                Selector childSelector = new Selector { TagName = part };
                currentSelector.Child = childSelector;
                currentSelector = childSelector;
            }
        }

        return rootSelector;
    }

    private static bool IsValidTagName(string tagName)
    {
        const string pattern = "^[a-zA-Z][a-zA-Z0-9/-_:]{0,}$";
        return Regex.IsMatch(tagName, pattern);
    }
}
