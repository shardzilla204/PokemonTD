using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokeCenterSlot : NinePatchRect
{
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
			if (PokemonTD.PokemonTeam.Pokemon.Count >= PokemonTD.MaxTeamSize) return;
			
			if (!isLeftClick || !isDoubleClick) return;
			
			PokemonTD.PokeCenter.RemovePokemon(Pokemon);
			QueueFree();
		};
	}

    public override Variant _GetDragData(Vector2 atPosition)
    {
		SetDragPreview(GetDragPreview());
		Dictionary<string, Variant> dataDictionary = new Dictionary<string, Variant>()
		{
			{ "FromTeamSlot", false },
			{ "FromAnalysisSlot", false },
			{ "Slot", this }
		};
		return dataDictionary;
    }

	public Control GetDragPreview()
	{
		Control control = new Control();

		if (Pokemon is null) return control;

		PokeCenterSlot pokeCenterSlot = PokemonTD.PackedScenes.GetPokeCenterSlot();
		pokeCenterSlot.Position = -pokeCenterSlot.Size / 2;
		pokeCenterSlot.UpdateSlot(Pokemon);

		control.AddChild(pokeCenterSlot);

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
