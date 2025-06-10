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
        AddStartingItems();
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
            AddPokeMartItem(pokeMartItemName, pokeMartItemsDictionaries);
        }
    }

    public void AddStartingItems()
    {
        List<string> pokeMartItemNames = PokemonTD.StartingInventory.Keys.ToList();
        for (int i = 0; i < pokeMartItemNames.Count; i++)
        {
            string itemName = pokeMartItemNames[i];
            int itemQuantity = PokemonTD.StartingInventory[itemName];
            PokeMartItem pokeMartItem = FindItem(itemName);
            pokeMartItem.Quantity += itemQuantity;
        }
    }

    private void AddPokeMartItem(string pokeMartItemName, GC.Dictionary<string, Variant> pokeMartItemsDictionaries)
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

    public List<PokeMartItem> FindItems(PokeMartItemCategory targetCategory)
    {
        return Items.FindAll(item => item.Category == targetCategory);
    }

    public PokeMartItem FindItem(string pokeMartItemName)
    {
        return Items.Find(item => item.Name == pokeMartItemName);
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

    public int GetHealAmount(Pokemon pokemon, PokeMartItem potion) => potion.Name switch
    {
        "Potion" => 20,
        "Super Potion" => 50,
        "Hyper Potion" => 200,
        "Max Potion" => pokemon.Stats.MaxHP,
        _ => 0
    };

    public PokeMartItem GetBestPotion(Pokemon pokemon)
    {
        List<PokeMartItem> potions = FindItems(PokeMartItemCategory.Medicine);
        List<PokeMartItem> stockedPotions = potions.FindAll(potion => potion.Quantity != 0);
        stockedPotions.Reverse(); // Get the potion that heals the most first

        PokeMartItem bestPotion = FindBestPotion(pokemon, stockedPotions);
        return bestPotion;
    }

    // Get highest healing potion ! that doesn't go over the max HP
    private PokeMartItem FindBestPotion(Pokemon pokemon, List<PokeMartItem> stockedPotions)
    {
        if (stockedPotions.Count == 0) return null;

        int index = 0;
        PokeMartItem bestPotion = stockedPotions[index];
        int highestHealAmount = GetHealAmount(pokemon, bestPotion);

        while (highestHealAmount > pokemon.Stats.MaxHP && bestPotion.Name != "Max Potion")
        {
            index++;
            if (index > stockedPotions.Count - 1) break;

            PokeMartItem potion = stockedPotions[index];
            int healAmount = GetHealAmount(pokemon, potion);

            bestPotion = potion;
            highestHealAmount = healAmount;
        }

        if (highestHealAmount + pokemon.Stats.HP > pokemon.Stats.MaxHP)
        {
            return null;
        }
        else
        {
            return bestPotion;
        }
    }
}