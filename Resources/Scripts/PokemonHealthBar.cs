using System;
using Godot;

namespace PokemonTD;

public partial class PokemonHealthBar : Container
{
    [Signal]
    public delegate void FaintedEventHandler();

    [Export]
    private Label _pokemonHealthLabel;

    [Export]
    private TextureProgressBar _healthBar;

    public void Update(Pokemon pokemon)
    {
        _pokemonHealthLabel.Text = pokemon != null ? $"{pokemon.Stats.HP} HP" : null;

        _healthBar.MaxValue = pokemon != null ? pokemon.Stats.MaxHP : 100;
        _healthBar.Value = pokemon != null ? pokemon.Stats.HP : 100;
    }

    public void AddHealth(Pokemon pokemon, int health)
    {
        SetHealth(pokemon, health);
    }

    public void SubtractHealth(Pokemon pokemon, int damage)
    {
        SetHealth(pokemon, -damage);
        CheckHealth(pokemon);
    }

    private void SetHealth(Pokemon pokemon, int value)
    {
        pokemon.Stats.HP += value;
        pokemon.Stats.HP = Math.Clamp(pokemon.Stats.HP, 0, pokemon.Stats.MaxHP);
        _pokemonHealthLabel.Text = pokemon != null ? $"{pokemon.Stats.HP} HP" : null;
        _healthBar.Value = pokemon.Stats.HP;
    }

    public void ResetHealth(Pokemon pokemon)
    {
        pokemon.Stats.HP = pokemon.Stats.MaxHP;
        Update(pokemon);
    }

    private void CheckHealth(Pokemon pokemon)
    {
        if (_healthBar.Value > _healthBar.MinValue) return;

        PokemonTD.SubtractPokeDollars(pokemon);
        EmitSignal(SignalName.Fainted);
    }
}
