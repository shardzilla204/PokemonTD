using Godot;
using Godot.Collections;

namespace PokémonTD;

public partial class InventorySlot : NinePatchRect
{
	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private TextureRect _pokémonSprite;

	[Export]
	private Label _pokémonLevel;

	public Pokémon Pokémon;
	public bool IsFilled;

	public int ID;

	public override void _Ready()
	{
		_pokémonSprite.Texture = Pokémon == null ? null : Pokémon.Sprite;
		_pokémonLevel.Text = Pokémon == null ? "" : $"LVL {Pokémon.Level}";

		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) =>
		{
			if (!isLeftClick || !isDoubleClick) return;
			
			PokémonTD.Signals.EmitSignal(Signals.SignalName.InventorySlotRemoved, Pokémon);
		};
	}

    public override Variant _GetDragData(Vector2 atPosition)
    {
		SetDragPreview(GetDragPreview());

		Dictionary<string, Variant> dragDictionary = new Dictionary<string, Variant>()
		{
			{ "FromTeamSlot", false },
			{ "Slot", this }
		};

		return dragDictionary;
    }

	private Control GetDragPreview()
	{
		Control control = new Control();

		if (Pokémon is null) return control;

		InventorySlot inventorySlot = PokémonTD.PackedScenes.GetInventorySlot();
		inventorySlot.Position = -inventorySlot.Size / 2;
		inventorySlot.UpdatePokémon(Pokémon);

		control.AddChild(inventorySlot);

		return control;
	}

	public void UpdatePokémon(Pokémon pokémon)
	{
		Pokémon = pokémon;

		_pokémonSprite.Texture = pokémon == null ? null : pokémon.Sprite;
		_pokémonLevel.Text = pokémon == null ? "" : $"LVL {pokémon.Level}";
		IsFilled = pokémon == null ? false : true;
	}
}
