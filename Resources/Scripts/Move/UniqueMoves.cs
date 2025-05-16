using Godot;
using System.Collections.Generic;

namespace PokemonTD;

// Swift && Metronome already handled
public partial class UniqueMoves : Node
{
    private List<string> _uniqueMoveNames = new List<string>()
    {
        "Dragon Rage",
        "Low Kick",
        "Seismic Toss",
        "Mirror Move",
        "Night Shade",
        "Mimic",
        "Pay Day",
        "Sonic Boom",
        "Super Fang",
        "Psywave",
        "Surf"
    };

    public bool IsUniqueMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _uniqueMoveNames.Find(move => move == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyUniqueMove<Defending>(Defending defendingPokemon, PokemonMove pokemonMove)
    {
        UniqueMoves uniqueMoves = PokemonMoveEffect.Instance.UniqueMoves;
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            switch (pokemonMove.Name)
            {
                case "Dragon Rage":
                    uniqueMoves.DragonRage(pokemonStageSlot);
                break;
                case "Low Kick":
                    uniqueMoves.LowKick(pokemonStageSlot);
                break;
                case "Seismic Toss":
                    uniqueMoves.SeismicToss(pokemonStageSlot);
                break;
                case "Mirror Move":
                    uniqueMoves.MirrorMove(this, pokemonStageSlot);
                break;
                case "Night Shade":
                    uniqueMoves.MirrorMove(this, pokemonStageSlot);
                break;
                case "Mimic":
                    uniqueMoves.Mimic(this, pokemonStageSlot);
                 break;
                case "Pay Day":
                    uniqueMoves.PayDay();
                break;
                case "Sonic Boom":
                    uniqueMoves.SonicBoom(pokemonStageSlot);
                break;
                case "Super Fang":
                    uniqueMoves.SuperFang(pokemonStageSlot);
                break;
                case "Psywave":
                    uniqueMoves.Psywave(pokemonStageSlot);
                break;
                case "Surf":
                    uniqueMoves.Surf(this, pokemonMove, pokemonStageSlot);
                break;
                case "Teleport":
                    uniqueMoves.Teleport();
                break;
            }
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            switch (pokemonMove.Name)
            {
                case "Dragon Rage":
                    uniqueMoves.DragonRage(pokemonEnemy);
                break;
                case "Low Kick":
                    uniqueMoves.LowKick(pokemonEnemy);
                break;
                case "Seismic Toss":
                    uniqueMoves.SeismicToss(pokemonEnemy);
                break;
                case "Mirror Move":
                    uniqueMoves.MirrorMove(this, pokemonEnemy);
                break;
                case "Night Shade":
                    uniqueMoves.MirrorMove(this, pokemonEnemy);
                break;
                case "Mimic":
                    uniqueMoves.Mimic(this, pokemonEnemy);
                 break;
                case "Sonic Boom":
                    uniqueMoves.SonicBoom(pokemonEnemy);
                break;
                case "Super Fang":
                    uniqueMoves.SuperFang(pokemonEnemy);
                break;
                case "Psywave":
                    uniqueMoves.Psywave(pokemonEnemy);
                break;
                case "Surf":
                    uniqueMoves.Surf(this, pokemonMove, pokemonEnemy);
                break;
            }
        }
    }

    // Always inflicts 40 HP
    public void DragonRage<Defending>(Defending defendingPokemon)
    {
        int damage = 40;
        PokemonCombat.Instance.ApplyDamage(defendingPokemon, damage);
    }

    // The heavier the opponent, the stronger the attack
    public void LowKick<Defending>(Defending defendingPokemon)
    {
        float multiplier = 1.5f;
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int damage = Mathf.RoundToInt((1 + pokemonStageSlot.Pokemon.Weight) * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = Mathf.RoundToInt((1 + pokemonEnemy.Pokemon.Weight) * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    //Inflicts damage equal to user's level
    public void SeismicToss<Defending>(Defending defendingPokemon)
    {
        float multiplier = 1.5f;
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    // User performs the opponent's last move
    public void MirrorMove<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        if (defendingPokemon is StageSlot)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;

            PokemonMove pokemonMove = pokemonStageSlot.Pokemon.Move;
            pokemonEnemy.AttackPokemon(pokemonMove, pokemonStageSlot);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            StageSlot pokemonStageSlot = attackingPokemon as StageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            PokemonMove pokemonMove = pokemonEnemy.Pokemon.Move;
            pokemonStageSlot.AttackPokemonEnemy(pokemonMove, pokemonEnemy);
        }
    }

    //Inflicts damage equal to user's level
    public void NightShade<Defending>(Defending defendingPokemon)
    {
        float multiplier = 1.5f;
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * multiplier);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    // User performs the opponent's last move
    public void Mimic<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        if (attackingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = attackingPokemon as StageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            PokemonMove pokemonMove = pokemonStageSlot.Pokemon.Move;
            pokemonEnemy.AttackPokemon(pokemonMove, pokemonStageSlot);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;

            PokemonMove pokemonMove = pokemonEnemy.Pokemon.Move;
            pokemonStageSlot.AttackPokemonEnemy(pokemonMove, pokemonEnemy);
        }
    }

    // Money is earned.
    public void PayDay()
    {
        PokemonTD.PokeDollars += 5;
        PokemonTD.Signals.EmitSignal(Signals.SignalName.PokeDollarsUpdated);
    }

    // Always inflicts 20 HP
    public void SonicBoom<Defending>(Defending defendingPokemon)
    {
        int damage = 20;
        PokemonCombat.Instance.ApplyDamage(defendingPokemon, damage);
    }

    // Always takes off half of the opponent's HP
    public void SuperFang<Defending>(Defending defendingPokemon)
    {
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.HP / 2);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.HP / 2);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    // Inflicts damage 50-150% of user's level
    public void Psywave<Defending>(Defending defendingPokemon)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float percentage = RNG.RandfRange(0.5f, 1.5f);
        if (defendingPokemon is StageSlot pokemonStageSlot)
        {
            int damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * percentage);
            PokemonCombat.Instance.ApplyDamage(pokemonStageSlot, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * percentage);
            PokemonCombat.Instance.ApplyDamage(pokemonEnemy, damage);
        }
    }

    // Hits all adjacent Pokemon
    public void Surf<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        if (attackingPokemon is StageSlot)
        {
            StageSlot pokemonStageSlot = attackingPokemon as StageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            int damage = PokemonManager.Instance.GetDamage(pokemonStageSlot.Pokemon, pokemonMove, pokemonEnemy.Pokemon);
            PokemonCombat.Instance.AttackAllPokemon(pokemonStageSlot.PokemonEnemyQueue, damage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            StageSlot pokemonStageSlot = defendingPokemon as StageSlot;

            int damage = PokemonManager.Instance.GetDamage(pokemonEnemy.Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
            PokemonCombat.Instance.AttackAllPokemon(pokemonEnemy.PokemonQueue, damage);
        }
    }

    // Halves damage from Special attacks for 5 turns
    public void LightScreen<T>(T parameter)
    {
        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.LightScreenCount = 5;
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.LightScreenCount = 5;
        }
    }

    // Halves damage from Physical attacks for 5 turns
    public void Reflect<T>(T parameter)
    {
        if (parameter is StageSlot pokemonStageSlot)
        {
            pokemonStageSlot.ReflectCount = 5;
        }
        else if (parameter is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.ReflectCount = 5;
        }
    }

    // Allows user to move between areas on the map
    public void Teleport()
    {

    }
}