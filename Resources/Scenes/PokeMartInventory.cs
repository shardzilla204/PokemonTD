using Godot;

namespace PokemonTD;

public partial class PokeMartInventory : Container
{
    [Export]
    private Container _pokeMartSlots;

    public override void _ExitTree()
    {
        base._ExitTree();
    }

    public override void _Ready()
    {
        UpdateSlots();
    }

    private void ClearSlots()
    {
        foreach (Node child in _pokeMartSlots.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void AddSlots()
    {
        foreach (PokeMartItem pokeMartItem in PokeMart.Instance.Items)
        {
            if (pokeMartItem.Quantity == 0) continue;

            PokeMartSlot pokeMartSlot = PokemonTD.PackedScenes.GetPokeMartSlot(pokeMartItem);
            pokeMartItem.Used += UpdateSlots;
            pokeMartSlot.PokeMartItem = pokeMartItem;
            _pokeMartSlots.AddChild(pokeMartSlot);
        }
    }

    public void UpdateSlots()
    {
        ClearSlots();
        AddSlots();
    }
}