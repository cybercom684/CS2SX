using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

// JsonSerializer without reflection: the build writes a writer and a reader for
// every type that is serialized. Expected output in comments.
public enum Rank { Bronze, Silver, Gold }

public class Item
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public class SaveGame
{
    public string Player { get; set; } = "";
    public int Level { get; set; }
    public double Time { get; set; }
    public bool Hard { get; set; }
    public Rank Rank { get; set; }
    public int? Best { get; set; }
    public List<Item> Items { get; set; } = new List<Item>();
    public int[] Scores { get; set; } = new int[0];
    public Dictionary<string, int> Flags { get; set; } = new Dictionary<string, int>();
    [JsonPropertyName("hp")] public float Health { get; set; } = 100;
    [JsonIgnore] public string Secret { get; set; } = "keep";
    public int Lives = 3;               // a field: only with IncludeFields
    public string Note { get; set; } = "default";
}

public record Point(int X, int Y);

public class Card
{
    [JsonConverter(typeof(JsonStringEnumConverter))] public Rank Rank { get; set; } = Rank.Silver;
    public Rank Plain { get; set; } = Rank.Gold;
}

public struct Size
{
    public int W { get; set; }
    public int H { get; set; }
}

public class JsonApp : SwitchApp
{
    public override void OnInit()
    {
        var g = new SaveGame { Player = "Mia \"M\"", Level = 7, Time = 12.5, Hard = true, Rank = Rank.Gold };
        g.Items.Add(new Item { Name = "Schwert", Count = 1 });
        g.Items.Add(new Item { Name = "Trank", Count = 3 });
        g.Scores = new[] { 10, 20 };
        g.Flags["door"] = 1;
        var text = JsonSerializer.Serialize(g);
        Console.WriteLine(text);
        // {"Player":"Mia \"M\"","Level":7,"Time":12.5,"Hard":true,"Rank":2,"Best":null,"Items":[{"Name":"Schwert","Count":1},{"Name":"Trank","Count":3}],"Scores":[10,20],"Flags":{"door":1},"hp":100,"Note":"default"}

        var back = JsonSerializer.Deserialize<SaveGame>(text)!;
        Console.WriteLine(back.Player + " " + back.Level + " " + back.Time + " " + back.Hard + " " + back.Rank
            + " " + back.Items.Count + " " + back.Items[1].Name + " " + back.Scores[1] + " " + back.Flags["door"]
            + " " + back.Health + " " + back.Secret + " " + (back.Best == null));          // Mia "M" 7 12.5 True Gold 2 Trank 20 1 100 keep True

        // missing members keep their defaults, unknown ones are skipped
        var part = JsonSerializer.Deserialize<SaveGame>("{ \"Level\": 3, \"Unknown\": [1, {\"a\": null}], \"Best\": 9 }")!;
        Console.WriteLine("part " + part.Level + " " + part.Note + " " + part.Best + " " + part.Lives);   // part 3 default 9 3

        // options: indented, camelCase, case-insensitive, fields
        var opts = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase, IncludeFields = true };
        var item = new Item { Name = "Ring", Count = 2 };
        Console.WriteLine(JsonSerializer.Serialize(item, opts));
        // {
        //   "name": "Ring",
        //   "count": 2
        // }
        var withFields = JsonSerializer.Serialize(new SaveGame { Lives = 5 }, new JsonSerializerOptions { IncludeFields = true });
        Console.WriteLine("fields " + withFields.Contains("\"Lives\":5"));                  // fields True
        var ci = JsonSerializer.Deserialize<Item>("{\"NAME\":\"Axt\",\"count\":4}", new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        Console.WriteLine("ci " + ci.Name + " " + ci.Count);                               // ci Axt 4

        // records, structs, lists and plain values
        var p = JsonSerializer.Deserialize<Point>("{\"X\":4,\"Y\":-2}")!;
        Console.WriteLine("point " + JsonSerializer.Serialize(p) + " " + p.Y);           // point {"X":4,"Y":-2} -2
        var s = JsonSerializer.Deserialize<Size>("{\"W\":3,\"H\":5}");
        Console.WriteLine("size " + (s.W * s.H));                                          // size 15
        var list = JsonSerializer.Deserialize<List<string>>("[\"a\\nb\", \"\\u00e4\\u20ac\", null]")!;
        Console.WriteLine("list " + list.Count + " " + list[0].Length + " " + (list[1] == "ä€") + " " + (list[2] == null));   // list 3 3 True True
        Console.WriteLine("plain " + JsonSerializer.Serialize(42) + " " + JsonSerializer.Serialize("x") + " "
            + JsonSerializer.Serialize(new List<double> { 1.5, -2 }) + " " + JsonSerializer.Deserialize<int>(" 17 "));   // plain 42 "x" [1.5,-2] 17

        // enums as names: JsonStringEnumConverter on a member or in the options
        Console.WriteLine("enum " + JsonSerializer.Serialize(new Card()));                  // enum {"Rank":"Silver","Plain":2}
        var named = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        Console.WriteLine("enum2 " + JsonSerializer.Serialize(new Card(), named));          // enum2 {"Rank":"Silver","Plain":"Gold"}
        var card = JsonSerializer.Deserialize<Card>("{\"Rank\":\"gold\",\"Plain\":0}", named)!;
        Console.WriteLine("enum3 " + card.Rank + " " + card.Plain);                         // enum3 Gold Bronze

        // errors
        try { JsonSerializer.Deserialize<Item>("{\"Count\":\"x\"}"); }
        catch (JsonException) { Console.WriteLine("error type"); }                           // error type
        try { JsonSerializer.Deserialize<Item>("{\"Count\":1"); }
        catch (JsonException) { Console.WriteLine("error end"); }                            // error end
        try { JsonSerializer.Deserialize<byte>("300"); }
        catch (JsonException) { Console.WriteLine("error range"); }                          // error range
        Console.WriteLine("done");
    }

    public override void OnFrame() { }
}
