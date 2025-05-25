using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class RecoverMoves : Node
{
    private List<string> _healthRecoveryMoves = new List<string>()
    {
        "Leech Life",
        "Absorb",
        "Leech Seed",
        "Mega Drain",
        "Dream Eater"
    };

    private List<string> _rareCandyRecoveryMoves = new List<string>()
    {
        "Recover",
        "Soft-Boiled",
        "Rest"
    };

    public bool IsHealthRecoveryMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _healthRecoveryMoves.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public bool IsRareCandyRecoveryMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _rareCandyRecoveryMoves.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyHealthRecoveryMove(GodotObject attacking, PokemonMove pokemonMove, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetAttackingPokemon(attacking);

        int damage = PokemonCombat.Instance.GetDamage(attacking, pokemonMove, defending);
        int health = damage / 2;
        PokemonCombat.Instance.HealPokemon(attacking, health);

        // Print message to console
        string healthRecoveryMessage = $"{attackingPokemon.Name} Healed {health} HP";
        PrintRich.PrintLine(TextColor.Purple, healthRecoveryMessage);
    }

    public void ApplyRareCandyRecoveryMove(PokemonStageSlot pokemonStageSlot, PokemonMove pokemonMove)
    {
        int maxRareCandyCount = 200;
        PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<PokemonStage>();
        pokemonStage.RareCandy += pokemonMove.Name == "Rest" ? 5 : 2;
        pokemonStage.RareCandy = Mathf.Clamp(pokemonStage.RareCandy, 0, maxRareCandyCount);
        pokemonStageSlot.Effects.HasMoveSkipped = pokemonMove.Name == "Rest";

        PokemonTD.Signals.EmitSignal(Signals.SignalName.RareCandyUpdated);
    }

    public async void LeechSeed(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        int iterationCount = 3;
        float drainPercentage = 0.1f;
        float timeSeconds = 0.75f;

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Fainted += (pokemonEnemy) => { return; };
        }

        for (int i = 0; i < iterationCount; i++)
        {
            if (!IsInstanceValid(defending)) return;

            int damage = Mathf.RoundToInt(defendingPokemon.HP * drainPercentage);
            PokemonCombat.Instance.HealPokemon(attacking, damage);
            PokemonCombat.Instance.DealDamage(defending, damage);

            await ToSignal(GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);

            // Print message to console
            string healthRecoveryMessage = $"{attackingPokemon.Name} Healed {damage} HP";
            PrintRich.PrintLine(TextColor.Red, healthRecoveryMessage);
        }
    }
}
