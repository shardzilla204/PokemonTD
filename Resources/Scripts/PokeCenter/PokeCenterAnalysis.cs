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

    [Export]
    private NinePatchRect _pokeCenterInfo;

    [Export]
    private NinePatchRect _pokeCenterStats;

    [Export]
    private Container _pokemonTypeIcons;

    [Export]
    private InteractComponent _interactComponent;

    public Pokemon Pokemon;

    public override void _ExitTree()
    {
        if (Pokemon != null) PokeCenter.Instance.Pokemon.Insert(0, Pokemon);
    }

    public override void _Ready()
    {
        _releaseButton.Pressed += () =>
        {
            if (PokeCenter.Instance.Pokemon.Count != 0) SetPokemon(null);
        };
        _interactComponent.Interacted += (isLeftClick, isPressed, isDoubleClick) =>
        {
            if (!isDoubleClick || !isLeftClick || Pokemon == null) return;

            PokeCenter.Instance.AddPokemon(Pokemon);
            SetPokemon(null);
        };

        SetPokemon(null);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest && Pokemon != null) PokeCenter.Instance.Pokemon.Insert(0, Pokemon);
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
        _pokemonDescription.Visible = pokemon != null;
        _pokeCenterInfo.SizeFlagsVertical = pokemon != null ? SizeFlags.Fill : SizeFlags.ShrinkBegin;
        _releaseButton.Visible = pokemon != null;
        _pokeCenterStats.Visible = pokemon != null;

        PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonTeamUpdated);
        Pokemon = pokemon;
        string nationalNumber = pokemon == null ? "" : $"#{pokemon.NationalNumber}";

        _pokemonName.Text = pokemon == null ? "Poke Dex" : $"{nationalNumber}: {pokemonName}";
        _pokemonLevel.Text = pokemon == null ? "Place A Pokemon To Analyze" : $"LVL. {pokemon.Level}";
        _genderIcon.Texture = pokemon == null ? null : PokemonTD.GetGenderSprite(pokemon);
        _pokemonHeight.Text = pokemon == null ? "" : $"Height: {pokemon.Height} m";
        _pokemonWeight.Text = pokemon == null ? "" : $"Weight: {pokemon.Weight} kg";
        _pokemonStats.Text = pokemon == null ? "" : GetStatsString(pokemon);
        _pokemonSprite.Texture = pokemon == null ? null : pokemon.Sprite;
        _pokemonDescription.Text = pokemon == null ? "" : pokemon.Description;

        SetTypeIcons(pokemon);

        PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonAnalyzed, pokemon);
    }

    private string GetStatsString(Pokemon pokemon)
    {
        string healthString = $"HP: {pokemon.Stats.HP}";
        string attackString = $"Attack: {pokemon.Stats.Attack}";
        string defenseString = $"Defense: {pokemon.Stats.Defense}";
        string specialAttackString = $"Sp. Attack: {pokemon.Stats.SpecialAttack}";
        string specialDefenseString = $"Sp. Defense: {pokemon.Stats.SpecialDefense}";
        string speedString = $"Speed: {pokemon.Stats.Speed}";

        string statsString = $"{healthString}\n{attackString}\n{defenseString}\n{specialAttackString}\n{specialDefenseString}\n{speedString}";
        return statsString;
    }

    private void SetTypeIcons(Pokemon pokemon)
    {
        // Remove current children
        foreach (Node child in _pokemonTypeIcons.GetChildren())
        {
            child.QueueFree();
        }
        
        if (pokemon == null) return;

        // Add Pokemon's type icons
        foreach (PokemonType pokemonType in pokemon.Types)
        {
            TextureRect typeTexture = GetTypeTexture(pokemonType);
            _pokemonTypeIcons.AddChild(typeTexture);
        }
    }

    private TextureRect GetTypeTexture(PokemonType pokemonType)
    {
        int sizeValue = 25;
        Texture2D pokemonTypeIcon = PokemonTypes.Instance.GetTypeIcon(pokemonType);
        TextureRect typeTexture = new TextureRect()
        {
            Texture = pokemonTypeIcon,
            CustomMinimumSize = new Vector2(sizeValue, sizeValue),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };
        return typeTexture;
    }
}
