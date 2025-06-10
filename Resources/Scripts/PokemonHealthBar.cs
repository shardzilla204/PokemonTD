using Godot;
using System;

namespace PokemonTD;

public partial class PokemonHealthBar : Container
{
    [Signal]
    public delegate void FaintedEventHandler();

    [Export]
    private Label _pokemonHealthLabel;

    [Export]
    private TextureProgressBar _healthBar;

    private Pokemon _pokemon;

    public void Update(Pokemon pokemon)
    {
        _pokemon = pokemon;
        _pokemonHealthLabel.Text = pokemon != null ? $"{pokemon.Stats.HP} HP" : null;

        _healthBar.MaxValue = pokemon != null ? pokemon.Stats.MaxHP : 100;
        _healthBar.Value = pokemon != null ? pokemon.Stats.HP : 100;
    }

    public void AddHealth(int health)
    {
        SetHealth(health);
    }

    public void SubtractHealth(int damage)
    {
        SetHealth(-damage);
        AutoHeal();
        CheckHealth();
    }

    private void SetHealth(int value)
    {
        _pokemon.Stats.HP += value;
        _pokemon.Stats.HP = Math.Clamp(_pokemon.Stats.HP, 0, _pokemon.Stats.MaxHP);
        _pokemonHealthLabel.Text = _pokemon != null ? $"{_pokemon.Stats.HP} HP" : null;
        _healthBar.Value = _pokemon.Stats.HP;
    }

    public void ResetHealth()
    {
        _pokemon.Stats.HP = _pokemon.Stats.MaxHP;
        Update(_pokemon);
    }

    private void CheckHealth()
    {
        if (_healthBar.Value > _healthBar.MinValue) return;

        PokemonTD.SubtractPokeDollars(_pokemon);
        EmitSignal(SignalName.Fainted);
    }

    private void AutoHeal()
    {
        if (!PokemonSettings.Instance.AutoHealEnabled) return;

        PokeMartItem bestPotion = PokeMart.Instance.GetBestPotion(_pokemon);
        if (bestPotion == null) return;

        bestPotion.Quantity--;
        
        int healAmount = PokeMart.Instance.GetHealAmount(_pokemon, bestPotion);
        AddHealth(healAmount);
        PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.AutoHealed);

        string healedMessage = $"{_pokemon.Name} Was Healed For {healAmount} HP";
        PrintRich.PrintLine(TextColor.Yellow, healedMessage);
    }
}
