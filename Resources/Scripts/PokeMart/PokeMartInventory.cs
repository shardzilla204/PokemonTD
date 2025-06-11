using Godot;

namespace PokemonTD;

public partial class PokeMartInventory : Container
{
    [Export]
    private Container _pokeMartSlots;

    public override void _ExitTree()
    {
        PokemonTD.Signals.ItemReceived -= UpdateSlots;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.ItemReceived += UpdateSlots;
        UpdateSlots();
    }

    private void ClearSlots()
    {
        foreach (PokeMartSlot pokeMartSlot in _pokeMartSlots.GetChildren())
        {
            pokeMartSlot.Used -= UpdateSlots;
            pokeMartSlot.QueueFree();
        }
    }

    private void AddSlots()
    {
        foreach (PokeMartItem pokeMartItem in PokeMart.Instance.Items)
        {
            if (pokeMartItem.Quantity == 0) continue;

            PokeMartSlot pokeMartSlot = PokemonTD.PackedScenes.GetPokeMartSlot(pokeMartItem);
            pokeMartSlot.Used += UpdateSlots;
            _pokeMartSlots.AddChild(pokeMartSlot);
        }
    }

    public void UpdateSlots()
    {
        ClearSlots();
        AddSlots();
    }
}