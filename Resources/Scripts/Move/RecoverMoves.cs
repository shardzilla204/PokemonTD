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

    public void ApplyHealthRecoveryMove<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            int pokemonMoveDamage = PokemonCombat.Instance.GetPokemonMoveDamage(pokemonStageSlot, pokemonMove, pokemonEnemy);
            int healthRecoverAmount = pokemonMoveDamage / 2;
            pokemonStageSlot.HealPokemon(healthRecoverAmount);

            // Print Message To Console
            string healthRecoveryMessage = $"{pokemonStageSlot.Pokemon.Name} Healed {healthRecoverAmount} HP";
            PrintRich.PrintLine(TextColor.Purple, healthRecoveryMessage);

        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            int pokemonMoveDamage = PokemonCombat.Instance.GetPokemonMoveDamage(pokemonEnemy, pokemonMove, pokemonStageSlot);
            int healthRecoverAmount = pokemonMoveDamage / 2;
            pokemonStageSlot.HealPokemon(healthRecoverAmount);

            // Print Message To Console
            string healthRecoveryMessage = $"{pokemonEnemy.Pokemon.Name} Healed {healthRecoverAmount} HP";
            PrintRich.PrintLine(TextColor.Red, healthRecoveryMessage);
        }
    }

    public void ApplyRareCandyRecoveryMove(PokemonStageSlot pokemonStageSlot, PokemonMove pokemonMove)
    {
        int MaxRareCandyCount = 200;
        PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<PokemonStage>();
        pokemonStage.RareCandy += pokemonMove.Name == "Rest" ? 5 : 2;
        pokemonStage.RareCandy = Mathf.Clamp(pokemonStage.RareCandy, 0, MaxRareCandyCount);
        pokemonStageSlot.Effects.HasMoveSkipped = pokemonMove.Name == "Rest";

        PokemonTD.Signals.EmitSignal(Signals.SignalName.RareCandyUpdated);
    }

    public async void LeechSeed<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        int iterationCount = 3;
        float drainPercentage = 0.1f;
        float timeSeconds = 0.75f;

        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            pokemonEnemy.Fainted += (pokemonEnemy) => { return; };
        
            for (int i = 0; i < iterationCount; i++)
            {
                if (!IsInstanceValid(pokemonEnemy)) return;

                int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.HP * drainPercentage);
                pokemonStageSlot.HealPokemon(damage);
                pokemonEnemy.DamagePokemon(damage);

                await ToSignal(pokemonStageSlot.GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);

                // Print Message To Console
                string healthRecoveryMessage = $"{pokemonStageSlot.Pokemon.Name} Healed {damage} HP";
                PrintRich.PrintLine(TextColor.Purple, healthRecoveryMessage);
            }
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;
            pokemonStageSlot.Fainted += (pokemonStageSlot) => { return; };

            for (int i = 0; i < iterationCount; i++)
            {
                if (!IsInstanceValid(pokemonEnemy)) return;
                
                int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.HP * drainPercentage);
                pokemonEnemy.HealPokemon(damage);
                pokemonStageSlot.DamagePokemon(damage);

                await ToSignal(pokemonEnemy.GetTree().CreateTimer(timeSeconds), SceneTreeTimer.SignalName.Timeout);

                // Print Message To Console
                string healthRecoveryMessage = $"{pokemonEnemy.Pokemon.Name} Healed {damage} HP";
                PrintRich.PrintLine(TextColor.Red, healthRecoveryMessage);
            }
        }
    }
}
