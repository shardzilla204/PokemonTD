using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class InventoryInterface : CanvasLayer
{
    [Export]
    private Container _stageItemContainer;

    public override void _ExitTree()
    {
        PokemonTD.Signals.ChangeMovesetPressed -= QueueFree;
		PokemonTD.Signals.Dragging -= Dragging;
        PokemonTD.Keybinds.ChangePokemonMove -= KeybindPressed;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.ChangeMovesetPressed += QueueFree;
		PokemonTD.Signals.Dragging += Dragging;
		PokemonTD.Keybinds.ChangePokemonMove += KeybindPressed;
        
        ClearContainer();
        AddItems();
    }

    private void ClearContainer()
    {
        foreach (Node child in _stageItemContainer.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void AddItems()
    {
        List<PokeMartItem> potions = PokeMart.Instance.FindItems(PokeMartItemCategory.Medicine);
        foreach (PokeMartItem potion in potions)
        {
            StageItem stageItem = PokemonTD.PackedScenes.GetStageItem(potion);
            _stageItemContainer.AddChild(stageItem);
        }
    }

    private void KeybindPressed(int pokemonTeamIndex)
	{
		QueueFree();
	}

	private void Dragging(bool isDragging)
	{
		QueueFree();
	}
}
