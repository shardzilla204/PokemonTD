using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokemonCenterTeamSlot : NinePatchRect
{
	[Signal]
	public delegate void RemovedEventHandler(Pokemon pokemon);

	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private TextureRect _pokemonSprite;

	[Export]
	private Label _pokemonLevel;

	public int ID;
	public Pokemon Pokemon;
	public bool IsFilled;

	public override void _Ready()
	{
		_pokemonSprite.Texture = Pokemon == null ? null : Pokemon.Sprite;
		_pokemonLevel.Text = Pokemon == null ? "" : $"LVL {Pokemon.Level}";

		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) =>
		{
			if (PokemonTD.PokemonTeam.Pokemon.Count == 0) return;
			
			if (!isLeftClick || !isDoubleClick) return;
			
			EmitSignal(SignalName.Removed, Pokemon);
			UpdateSlot(null);
		};
	}

	public override Variant _GetDragData(Vector2 atPosition)
    {
		SetDragPreview(GetDragPreview());
		Dictionary<string, Variant> dataDictionary = new Dictionary<string, Variant>()
		{
			{ "FromTeamSlot", true },
			{ "Slot", this }
		};
		return dataDictionary;
    }

	public Control GetDragPreview()
	{
		Control control = new Control();

		if (Pokemon is null) return control;

		PokemonCenterSlot pokemonCenterSlot = PokemonTD.PackedScenes.GetPokemonCenterSlot();
		pokemonCenterSlot.Position = -pokemonCenterSlot.Size / 2;
		pokemonCenterSlot.UpdateSlot(Pokemon);

		control.AddChild(pokemonCenterSlot);

		return control;
	}

	public void UpdateSlot(Pokemon pokemon)
	{
		Pokemon = pokemon;

		_pokemonSprite.Texture = pokemon == null ? null : pokemon.Sprite;
		_pokemonLevel.Text = pokemon == null ? "" : $"LVL {pokemon.Level}";
		IsFilled = pokemon == null ? false : true;
	}
}
