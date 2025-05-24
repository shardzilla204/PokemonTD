using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokeCenterSlot : NinePatchRect
{
	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private TextureProgressBar _healthBar;

	[Export]
	private TextureRect _pokemonSprite;

	[Export]
	private Label _pokemonLevel;

	public int ID;
	public Pokemon Pokemon;
	public bool IsFilled;

	public override void _Ready()
	{
		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) =>
		{
			if (PokemonTeam.Instance.Pokemon.Count >= PokemonTD.MaxTeamSize) return;
			
			if (!isLeftClick || !isDoubleClick) return;
			
			PokeCenter.Instance.RemovePokemon(Pokemon);
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

		_healthBar.Visible = pokemon != null;
		_healthBar.MaxValue = pokemon == null ? 100 : pokemon.MaxHP;
		_healthBar.Value = pokemon == null ? 100 : pokemon.HP;

		_pokemonSprite.Texture = pokemon == null ? null : pokemon.Sprite;
		_pokemonLevel.Text = pokemon == null ? "" : $"LVL {pokemon.Level}";
		IsFilled = pokemon != null;
	}
}
