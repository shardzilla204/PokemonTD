using Godot;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonInventory : Node
{
    private static PokemonInventory _instance;

    public static PokemonInventory Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    public List<PokeMartItem> _items = new List<PokeMartItem>();

    public void AddItem(PokeMartItem pokeMartItem)
    {
        _items.Add(pokeMartItem);
    }

    public void RemoveItem(PokeMartItem pokeMartItem)
    {
        _items.Remove(pokeMartItem);
    }

    public int GetItemQuantity(string itemName)
    {
        return _items.Where(item => item.Name == itemName).Count();
    }
}