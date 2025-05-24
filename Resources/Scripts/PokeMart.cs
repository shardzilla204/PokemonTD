using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public enum PokeMartItemCategory
{
    EvolutionStone,
    Medicine, 
    Candy,
}

public partial class PokeMart : Node
{
    private static PokeMart _instance;

    public static PokeMart Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    private GC.Dictionary<string, Variant> _pokeMartItem = new GC.Dictionary<string, Variant>();

    public List<PokeMartItem> Items = new List<PokeMartItem>();

    public override void _EnterTree()
    {
        Instance = this;

        PokemonTD.Signals.GameReset += () =>
        {
            foreach (PokeMartItem pokeMartItem in Items)
            {
                pokeMartItem.Quantity = 0;
            }
        };

        LoadPokeMartItems();
    }

    public void LoadPokeMartItems()
    {
        string filePath = $"res://JSON/PokeMartItems.json";

        using FileAccess pokeMartItemsFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        string jsonString = pokeMartItemsFile.GetAsText();

        Json json = new Json();

        if (json.Parse(jsonString) != Error.Ok) return;

        GC.Dictionary<string, Variant> pokeMartItemsDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary)json.Data);

        foreach (string pokeMartItemName in pokeMartItemsDictionaries.Keys)
        {
            AddPokeMartItems(pokeMartItemName, pokeMartItemsDictionaries);
        }
    }

    private void AddPokeMartItems(string pokeMartItemName, GC.Dictionary<string, Variant> pokeMartItemsDictionaries)
    {
        GC.Dictionary<string, Variant> pokeMartItemDictionary = pokeMartItemsDictionaries[pokeMartItemName].As<GC.Dictionary<string, Variant>>();
        PokeMartItem pokeMartItem = new PokeMartItem(pokeMartItemName, pokeMartItemDictionary);
        Items.Add(pokeMartItem);
    }

    public GC.Dictionary<string, int> GetData()
	{
		GC.Dictionary<string, int> pokemonInventoryData = new GC.Dictionary<string, int>();
		foreach (PokeMartItem pokeMartItem in Items)
		{
            pokemonInventoryData.Add(pokeMartItem.Name, pokeMartItem.Quantity);
		}
		return pokemonInventoryData;
	}

	public void SetData(GC.Dictionary<string, Variant> pokemonInventoryData)
    {
        List<string> pokeMartItemKeys = pokemonInventoryData.Keys.ToList();
        foreach (string pokeMartItemKey in pokeMartItemKeys)
        {
            int pokeMartItemQuantity = pokemonInventoryData[pokeMartItemKey].As<int>();

            PokeMartItem pokeMartItem = Items.Find(item => item.Name == pokeMartItemKey);
            pokeMartItem.Quantity = pokeMartItemQuantity;
        }
    }
}