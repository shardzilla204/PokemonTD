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

    public void ApplyHealthRecoveryMove(GodotObject attacking, GodotObject defending, PokemonMove pokemonMove)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);

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
        pokemonStageSlot.Pokemon.Effects.HasMoveSkipped = pokemonMove.Name == "Rest";

        PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.RareCandyUpdated);
    }

    public async void LeechSeed(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        int iterationCount = 3;
        float drainPercentage = 0.1f;
        SceneTree sceneTree = new SceneTree();

        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            sceneTree = pokemonStageSlot.GetTree();
            pokemonStageSlot.Retrieved += (pokemonStageSlot) => { return; };
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            sceneTree = pokemonEnemy.GetTree();
            pokemonEnemy.Fainted += (pokemonEnemy) => { return; };
        }

        for (int i = 0; i < iterationCount; i++)
        {
            if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, PokemonSignals.SignalName.PressedPlay);

            if (!IsInstanceValid(defending)) return;

            int damage = Mathf.RoundToInt(defendingPokemon.Stats.HP * drainPercentage);
            PokemonCombat.Instance.HealPokemon(attacking, damage);
            PokemonCombat.Instance.DealDamage(defending, damage);

            // Print message to console
            string healthRecoveryMessage = $"{attackingPokemon.Name} Healed {damage} HP";
            PrintRich.PrintLine(TextColor.Purple, healthRecoveryMessage);   

            await ToSignal(sceneTree.CreateTimer(1 / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);
        }
    }
}
