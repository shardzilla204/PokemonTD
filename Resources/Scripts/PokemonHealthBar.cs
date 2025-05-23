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
        _pokemonHealthLabel.Text = pokemon != null ? $"{pokemon.HP} HP" : null;

        _healthBar.MaxValue = pokemon != null ? pokemon.MaximumHP : 100;
        _healthBar.Value = pokemon != null ? pokemon.HP : 100;
    }

    public void AddHealth(Pokemon pokemon, int health)
    {
        SetHealth(pokemon, health);
    }

    public void SubtractHealth(Pokemon pokemon, int damage)
    {
        SetHealth(pokemon, -damage);
        CheckHealth();
    }

    private void SetHealth(Pokemon pokemon, int value)
    {
        pokemon.HP += value;
        pokemon.HP = Math.Clamp(pokemon.HP, 0, pokemon.MaximumHP);
        _pokemonHealthLabel.Text = pokemon != null ? $"{pokemon.HP} HP" : null;
        _healthBar.Value = pokemon.HP;
    }

    public void ResetHealth(Pokemon pokemon)
    {
        pokemon.HP = pokemon.MaximumHP;
        Update(pokemon);
    }

    private void CheckHealth()
    {
        if (_healthBar.Value > _healthBar.MinValue) return;

        EmitSignal(SignalName.Fainted);
    }
}
