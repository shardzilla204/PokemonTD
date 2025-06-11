using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokeCenterTeamSlot : NinePatchRect
{
	[Export]
	private InteractComponent _interactComponent;

	[Export]
	private TextureProgressBar _healthBar;

	[Export]
	private TextureRect _pokemonSprite;

	[Export]
	private Label _pokemonLevel;

	public Pokemon Pokemon;

	public override void _Ready()
	{
		_healthBar.Visible = Pokemon != null;
		_healthBar.MaxValue = Pokemon == null ? 100 : Pokemon.Stats.MaxHP;
		_healthBar.Value = Pokemon == null ? 100 : Pokemon.Stats.HP;

		_pokemonSprite.Texture = Pokemon == null ? null : Pokemon.Sprite;
		_pokemonLevel.Text = Pokemon == null ? "" : $"LVL {Pokemon.Level}";

		_interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) =>
		{
			if (PokemonTeam.Instance.Pokemon.Count == 0 || Pokemon == null) return;
			
			if (!isLeftClick || !isDoubleClick) return;

			PokeCenter.Instance.AddPokemon(Pokemon);
		};
	}

	public override Variant _GetDragData(Vector2 atPosition)
    {
		Control dragPreview = GetDragPreview();
		SetDragPreview(dragPreview);
		Dictionary<string, Variant> dataDictionary = new Dictionary<string, Variant>()
		{
			{ "FromTeamSlot", true },
			{ "FromAnalysisSlot", false },
			{ "Slot", this }
		};
		return dataDictionary;
    }

	private Control GetDragPreview()
	{
		Control control = new Control();

		if (Pokemon is null) return control;

		PokeCenterSlot pokeCenterSlot = PokemonTD.PackedScenes.GetPokeCenterSlot();
		pokeCenterSlot.Position = -pokeCenterSlot.Size / 2;
		pokeCenterSlot.UpdateSlot(Pokemon);

		control.AddChild(pokeCenterSlot);

		return control;
	}

	public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		Dictionary<string, Variant> dataDictionary = data.As<Dictionary<string, Variant>>();
		// {
		// 	{ "FromTeamSlot", false },
		// 	{ "FromAnalysisSlot", false },
		// 	{ "Slot", this }
		// };
		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
		if (fromTeamSlot) return false;

		Node slot = dataDictionary["Slot"].As<Node>();
		if (slot is PokeCenterSlot)
		{
			return Pokemon != null;
		}
		return false;
    }

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		// Swap Pokemon 
		Dictionary<string, Variant> dataDictionary = data.As<Dictionary<string, Variant>>();
		Node slot = dataDictionary["Slot"].As<Node>();
		if (slot is PokeCenterSlot pokeCenterSlot)
		{
			Pokemon newPokemon = pokeCenterSlot.Pokemon;
			PokeCenter.Instance.AddPokemon(Pokemon);
			PokeCenter.Instance.RemovePokemon(newPokemon);
		}
    }

	public void UpdateSlot(Pokemon pokemon)
	{
		Pokemon = pokemon;

		_healthBar.Visible = Pokemon != null;
		_healthBar.MaxValue = Pokemon == null ? 100 : Pokemon.Stats.MaxHP;
		_healthBar.Value = Pokemon == null ? 100 : Pokemon.Stats.HP;

		_pokemonSprite.Texture = pokemon == null ? null : pokemon.Sprite;
		_pokemonLevel.Text = pokemon == null ? "" : $"LVL {pokemon.Level}";
	}
}
