using System;
using Godot;

namespace PokemonTD;

public partial class PokeMartTeamSlot : NinePatchRect
{
    [Export]
    private TextureRect _pokemonSprite;

    [Export]
    private TextureProgressBar _healthBar;

    [Export]
    private Label _pokemonLevel;

    private Pokemon _pokemon;

    public override void _Ready()
    {
        SetPokemon(null);
    }

    public void SetPokemon(Pokemon pokemon)
    {
        _pokemon = pokemon;

        _healthBar.Visible = pokemon != null;
        _healthBar.MaxValue = pokemon == null ? 100 : pokemon.MaxHP;
        _healthBar.Value = pokemon == null ? 100 : pokemon.HP;

        _pokemonSprite.Texture = pokemon == null ? null : pokemon.Sprite;
        _pokemonLevel.Text = pokemon == null ? "" : $"LVL {pokemon.Level}";
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        PokeMartItem pokeMartItem = data.As<PokeMartItem>();

        if (pokeMartItem.Category == PokeMartItemCategory.EvolutionStone) return PokemonEvolution.Instance.CanEvolveWithStone(_pokemon);
        return pokeMartItem != null;
    }

    public override async void _DropData(Vector2 atPosition, Variant data)
    {
        PokeMartItem pokeMartItem = data.As<PokeMartItem>();
        pokeMartItem.Quantity--;

        if (pokeMartItem.Category == PokeMartItemCategory.Candy)
        {
            int pokemonTeamIndex = PokemonTeam.Instance.Pokemon.IndexOf(_pokemon);
            PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonLeveledUp, 1, pokemonTeamIndex);

            string rareCandyMessage = $"Gave {_pokemon.Name} Rare Candy";
            PrintRich.PrintLine(TextColor.Orange, rareCandyMessage);
        }
        else if (pokeMartItem.Category == PokeMartItemCategory.Medicine)
        {
            int healAmount = GetHealAmount(pokeMartItem.Name);
            _pokemon.HP = Mathf.Clamp(_pokemon.HP + healAmount, 0, _pokemon.MaxHP);

            SetPokemon(_pokemon);

            int pokemonTeamIndex = PokemonTeam.Instance.Pokemon.IndexOf(_pokemon);
            PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonHealed, healAmount, pokemonTeamIndex);

            string healMessage = $"Healed {_pokemon.Name} For {healAmount} HP";
            PrintRich.PrintLine(TextColor.Orange, healMessage);
        }
        else if (pokeMartItem.Category == PokeMartItemCategory.EvolutionStone)
        {
            int pokemonTeamIndex = PokemonTeam.Instance.Pokemon.IndexOf(_pokemon);
            string evolutionStoneName = pokeMartItem.Name.TrimSuffix("Stone").Trim();
            EvolutionStone evolutionStone = Enum.Parse<EvolutionStone>(evolutionStoneName);
            PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEvolving, _pokemon, (int) evolutionStone, pokemonTeamIndex);

            await ToSignal(PokemonTD.Signals, Signals.SignalName.EvolutionFinished);

            _pokemon = PokemonEvolution.Instance.EvolvePokemonWithStone(_pokemon, evolutionStone);
            PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonEvolved, _pokemon, pokemonTeamIndex);
        }
    }

    private int GetHealAmount(string potionName) => potionName switch
    {
        "Potion" => 20,
        "Super Potion" => 50,
        "Hyper Potion" => 200,
        "Max Potion" => _pokemon.MaxHP,
        _ => 20
    };
}

