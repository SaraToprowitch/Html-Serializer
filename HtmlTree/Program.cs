using HtmlAgilityPack;
using HtmlTree;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var html = await Load("https://touchreality.co.il/");
        var cleanHtml = CleanHtml(html);
        var htmlLines = ExtractHtmlLines(cleanHtml);
        Console.WriteLine();

        var content = File.ReadAllText("seed/AllTags.json");
        Console.WriteLine(content);
        string[] allTags = JsonSerializer.Deserialize<string[]>(content);

        foreach (var tag in allTags)
        {
            Console.WriteLine(tag);
        }

        foreach (var item in HtmlHelper.Instance.AllHtmlTags)
        {
            Console.WriteLine(item);
        }

        var rootElement = await ParseHtmlFromUrl("https://touchreality.co.il/");
        if (rootElement != null)
        {
            PrintHtmlTree(rootElement, 0);
        }

        string htmlContent = await GetHtmlContentAsync("https://touchreality.co.il/");

        string queryString = "div #container .content p #content";
        PrintSelectorHierarchy(Selector.ConvertQueryStringToSelector(queryString));

        queryString = "input #username";
        PrintSelectorHierarchy(Selector.ConvertQueryStringToSelector(queryString));

        queryString = "h1 .title";
        PrintSelectorHierarchy(Selector.ConvertQueryStringToSelector(queryString));

        var root = new HtmlElement { Name = "div" };
        var child1 = new HtmlElement { Name = "span", Parent = root };
        var child2 = new HtmlElement { Name = "p", Parent = root };
        var grandchild = new HtmlElement { Name = "a", Parent = child1 };

        root.Children.Add(child1);
        root.Children.Add(child2);
        child1.Children.Add(grandchild);

        Console.WriteLine("Descendants:");
        foreach (var element in root.Descendants())
        {
            Console.WriteLine(element.Name);
        }

        Console.WriteLine("\nAncestors:");
        foreach (var element in grandchild.Ancestors())
        {
            Console.WriteLine(element.Name);
        }

        Console.WriteLine("\nFindElements:");
        var elements = HtmlElement.FindElements(root, e => e.Name == "a");
        foreach (var element in elements)
        {
            Console.WriteLine(element.Name);
        }
    }

    static async Task<string> Load(string url)
    {
        HttpClient client = new HttpClient();
        var response = await client.GetAsync(url);
        var html = await response.Content.ReadAsStringAsync();
        return html;
    }

    static string CleanHtml(string html)
    {
        return Regex.Replace(html, "\\s", "");
    }

    static List<string> ExtractHtmlLines(string cleanHtml)
    {
        return new Regex("<(.*?)>").Split(cleanHtml).Where(s => s.Length > 0).ToList();
    }

    static async Task<HtmlElement> ParseHtmlFromUrl(string url)
    {
        var html = await Load(url);
        var cleanHtml = CleanHtml(html);
        var htmlLines = ExtractHtmlLines(cleanHtml);
        return HtmlHelper.Instance.ParseHtml(htmlLines);
    }

    static void PrintHtmlTree(HtmlElement element, int depth)
    {
        if (element == null)
        {
            return;
        }

        Console.WriteLine($"{new string(' ', depth * 2)}{element.Name}");

        foreach (var child in element.Children)
        {
            PrintHtmlTree(child, depth + 1);
        }
    }

    static async Task<string> GetHtmlContentAsync(string url)
    {
        using (var httpClient = new HttpClient())
        {
            return await httpClient.GetStringAsync(url);
        }
    }

    static void PrintSelectorHierarchy(Selector selector)
    {
        Console.WriteLine("Selector Hierarchy:");
        Selector current = selector;
        while (current != null)
        {
            Console.WriteLine($"Tag: {current.TagName}, Id: {current.Id}, Classes: {string.Join(", ", current.Classes)}");
            current = current.Child;
        }
        Console.WriteLine();
    }
}
