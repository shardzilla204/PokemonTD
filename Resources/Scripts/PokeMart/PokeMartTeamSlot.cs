using Godot;
using System;

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
        _healthBar.MaxValue = pokemon == null ? 100 : pokemon.Stats.MaxHP;
        _healthBar.Value = pokemon == null ? 100 : pokemon.Stats.HP;

        _pokemonSprite.Texture = pokemon == null ? null : pokemon.Sprite;
        _pokemonLevel.Text = pokemon == null ? "" : $"LVL {pokemon.Level}";
    }

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
        if (_pokemon == null) return false;
        
        PokeMartItem pokeMartItem = data.As<PokeMartItem>();

        if (pokeMartItem.Category == PokeMartItemCategory.Candy)
        {
            return _pokemon.Level < PokemonTD.MaxPokemonLevel;
        }
        else if (pokeMartItem.Category == PokeMartItemCategory.Medicine)
        {
            int healAmount = GetHealAmount(pokeMartItem.Name);
            int desiredHealth = _pokemon.Stats.HP + healAmount;
            int healthThreshold = Mathf.RoundToInt(_pokemon.Stats.MaxHP + healAmount / 2);

            return desiredHealth < healthThreshold;
        }
        else if (pokeMartItem.Category == PokeMartItemCategory.EvolutionStone && !_pokemon.HasCanceledEvolution)
        {
            return PokemonEvolution.Instance.CanEvolveWithStone(_pokemon);
        }
        return false;
    }

    public override async void _DropData(Vector2 atPosition, Variant data)
    {
        PokeMartItem pokeMartItem = data.As<PokeMartItem>();
        pokeMartItem.Quantity--;

        if (pokeMartItem.Category == PokeMartItemCategory.Candy)
        {
            int pokemonTeamIndex = PokemonTeam.Instance.Pokemon.IndexOf(_pokemon);
            PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonLeveledUp, 1, pokemonTeamIndex);

            string rareCandyMessage = $"Gave {_pokemon.Name} Rare Candy";
            PrintRich.PrintLine(TextColor.Orange, rareCandyMessage);
        }
        else if (pokeMartItem.Category == PokeMartItemCategory.Medicine)
        {
            int healAmount = GetHealAmount(pokeMartItem.Name);
            _pokemon.Stats.HP = Mathf.Clamp(_pokemon.Stats.HP + healAmount, 0, _pokemon.Stats.MaxHP);

            SetPokemon(_pokemon);

            int pokemonTeamIndex = PokemonTeam.Instance.Pokemon.IndexOf(_pokemon);
            PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonHealed, healAmount, pokemonTeamIndex);

            string healMessage = $"Healed {_pokemon.Name} For {healAmount} HP";
            PrintRich.PrintLine(TextColor.Orange, healMessage);
        }
        else if (pokeMartItem.Category == PokeMartItemCategory.EvolutionStone)
        {
            int pokemonTeamIndex = PokemonTeam.Instance.Pokemon.IndexOf(_pokemon);
            string evolutionStoneName = pokeMartItem.Name.TrimSuffix("Stone").Trim();
            EvolutionStone evolutionStone = Enum.Parse<EvolutionStone>(evolutionStoneName);

            string evolutionStoneMessage = $"Gave {evolutionStoneName} Stone To {_pokemon.Name}";
            PrintRich.PrintLine(TextColor.Orange, evolutionStoneMessage);

            Pokemon pokemonResult = await PokemonManager.Instance.PokemonEvolving(_pokemon, evolutionStone, pokemonTeamIndex);
            if (pokemonResult != _pokemon)
            {
                _pokemon = PokemonEvolution.Instance.EvolvePokemonWithStone(_pokemon, evolutionStone);
            }
            else
            {
                pokeMartItem.Quantity++; // Give back stone if canceled evolution
                PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.ItemReceived);
            }

            PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonEvolved, _pokemon, pokemonTeamIndex);
        }
    }

    private int GetHealAmount(string potionName) => potionName switch
    {
        "Potion" => 20,
        "Super Potion" => 50,
        "Hyper Potion" => 200,
        "Max Potion" => _pokemon.Stats.MaxHP,
        _ => 20
    };
}

