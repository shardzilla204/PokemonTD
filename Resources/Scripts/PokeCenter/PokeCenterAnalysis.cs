using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokeCenterAnalysis : NinePatchRect
{
    [Export]
    private Label _pokemonName;

    [Export]
    private Label _pokemonLevel;

    [Export]
    private TextureRect _genderIcon;

    [Export]
    private Label _pokemonNumber;

    [Export]
    private Label _pokemonHeight;

    [Export]
    private Label _pokemonWeight;

    [Export]
    private Label _pokemonStats;

    [Export]
    private TextureRect _pokemonSprite;

    [Export]
    private Label _pokemonDescription;

    [Export]
    private CustomButton _releaseButton;

    public Pokemon Pokemon;

    public override void _ExitTree()
    {
        if (Pokemon is not null) PokeCenter.Instance.Pokemon.Insert(0, Pokemon);
    }

    public override void _Ready()
    {
        _releaseButton.Pressed += () => 
        {
            if (PokeCenter.Instance.Pokemon.Count == 0) return;
            
            SetPokemon(null);
        };

        SetPokemon(null);
    }

    public override Variant _GetDragData(Vector2 atPosition)
    {
		SetDragPreview(GetDragPreview());
		Dictionary<string, Variant> dataDictionary = new Dictionary<string, Variant>()
		{
			{ "FromAnalysisSlot", true },
			{ "PokeCenterAnalysis", this }
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

    public override bool _CanDropData(Vector2 atPosition, Variant data)
	{
		Dictionary<string, Variant> dataDictionary = data.As<Dictionary<string, Variant>>();
		bool fromAnalysisSlot = dataDictionary["FromAnalysisSlot"].As<bool>();

        if (fromAnalysisSlot) return false;

		bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
        if (fromTeamSlot)
        {
            PokeCenterTeamSlot pokeCenterTeamSlot = dataDictionary["Slot"].As<PokeCenterTeamSlot>();
            if (pokeCenterTeamSlot.Pokemon is not null) return true;
        }
        else
        {
            PokeCenterSlot pokeCenterSlot = dataDictionary["Slot"].As<PokeCenterSlot>();
            if (pokeCenterSlot.Pokemon is not null) return true;
        }

		return false;
	}

    public override void _DropData(Vector2 atPosition, Variant data)
    {
		Dictionary<string, Variant> dataDictionary = data.As<Dictionary<string, Variant>>();
        bool fromTeamSlot = dataDictionary["FromTeamSlot"].As<bool>();
        Pokemon pokemon = null;
        if (fromTeamSlot)
        {
            PokeCenterTeamSlot pokeCenterTeamSlot = dataDictionary["Slot"].As<PokeCenterTeamSlot>();
            if (pokeCenterTeamSlot.Pokemon is not null) pokemon = pokeCenterTeamSlot.Pokemon;
            PokemonTeam.Instance.Pokemon.Remove(pokemon);
        }
        else
        {
            PokeCenterSlot pokeCenterSlot = dataDictionary["Slot"].As<PokeCenterSlot>();
            if (pokeCenterSlot.Pokemon is not null) pokemon = pokeCenterSlot.Pokemon;
            PokeCenter.Instance.Pokemon.Remove(pokemon);
            pokeCenterSlot.QueueFree();
        }

		SetPokemon(pokemon);
    }

    public void SetPokemon(Pokemon pokemon)
    {
        string pokemonName = pokemon == null ? "" : pokemon.Name;
        if (pokemon != null) pokemonName = pokemon.Name.Contains("Nidoran") ? "Nidoran" : pokemon.Name;

        // Check if a pokemon is going in and there is already a pokemon in the slot, remove and add the pokemon to the inventory
        if (Pokemon != null)
        {
            if (pokemon != null) 
            {
                PokeCenter.Instance.Pokemon.Insert(0, Pokemon);
            }
        }

        PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamUpdated);

        Pokemon = pokemon;
        _pokemonName.Text = pokemonName;
        _pokemonLevel.Text = pokemon == null ? "" : $"LVL. {pokemon.Level}";
        _genderIcon.Texture = pokemon == null ? null : PokemonTD.GetGenderSprite(pokemon);
        _pokemonNumber.Text = pokemon == null ? "" : $"#{pokemon.NationalNumber}";
        _pokemonHeight.Text = pokemon == null ? "" : $"{pokemon.Height} m";
        _pokemonWeight.Text = pokemon == null ? "" : $"{pokemon.Weight} kg";
        _pokemonStats.Text = pokemon == null ? "" : GetStatsString(pokemon);
        _pokemonSprite.Texture = pokemon == null ? null : pokemon.Sprite;
        _pokemonDescription.Text = pokemon == null ? "" : pokemon.Description;

        PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonAnalyzed, pokemon);
    }

    private string GetStatsString(Pokemon pokemon)
    {
        string healthString = $"HP: {pokemon.HP}";
        string attackString = $"Attack: {pokemon.Attack}";
        string defenseString = $"Defense: {pokemon.Defense}";
        string specialAttackString = $"Sp. Attack: {pokemon.SpecialAttack}";
        string specialDefenseString = $"Sp. Defense: {pokemon.SpecialDefense}";
        string speedString = $"Speed: {pokemon.Speed}";

        string statsString = $"{healthString}\n{attackString}\n{defenseString}\n{specialAttackString}\n{specialDefenseString}\n{speedString}";
        return statsString;
    }
}
