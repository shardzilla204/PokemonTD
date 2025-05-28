using Godot;
using System.Collections.Generic;

namespace PokemonTD;

// Swift, Metronome, Mist, Quick Attack already handled
public partial class UniqueMoves : Node
{
    private List<string> _uniqueMoveNames = new List<string>()
    {
        "Dragon Rage",
        "Low Kick",
        "Seismic Toss",
        "Night Shade",
        "Pay Day",
        "Sonic Boom",
        "Super Fang",
        "Psywave",
        "Surf",
        "Teleport",
        "Counter",
        "Disable",
        "Conversion",
        "Roar",
        "Whirlwind",
        "Transform",
        "Petal Dance",
        "Thrash",
        "Substitute"
    };

    public bool IsUniqueMove(PokemonMove pokemonMove)
    {
        string pokemonMoveName = _uniqueMoveNames.Find(pokemonMoveName => pokemonMoveName == pokemonMove.Name);
        return pokemonMoveName != null;
    }

    public void ApplyUniqueMove(GodotObject attackingPokemon, GodotObject defendingPokemon, PokemonMove pokemonMove)
    {
        switch (pokemonMove.Name)
        {
            case "Dragon Rage":
                DragonRage(defendingPokemon);
                break;
            case "Low Kick":
                LowKick(defendingPokemon);
                break;
            case "Seismic Toss":
                SeismicToss(defendingPokemon);
                break;
            case "Night Shade":
                MirrorMove(attackingPokemon, defendingPokemon);
                break;
            case "Pay Day":
                if (attackingPokemon is PokemonEnemy) return;
                PayDay();
                break;
            case "Sonic Boom":
                SonicBoom(attackingPokemon);
                break;
            case "Super Fang":
                SuperFang(attackingPokemon);
                break;
            case "Psywave":
                Psywave(attackingPokemon);
                break;
            case "Teleport":
                if (attackingPokemon is PokemonEnemy) return;
                Teleport(attackingPokemon);
                break;
            case "Counter":
                Counter(attackingPokemon);
                break;
            case "Disable":
                Disable(defendingPokemon);
                break;
            case "Conversion":
                Conversion(attackingPokemon, defendingPokemon);
                break;
            case "Roar":
                Roar(defendingPokemon);
                break;
            case "Whirlwind":
                Whirlwind(defendingPokemon);
                break;
            case "Transform":
                Transform(attackingPokemon, defendingPokemon);
                break;
            case "Thrash":
                Thrash(attackingPokemon);
                break;
            case "Petal Dance":
                PetalDance(attackingPokemon);
                break;
            case "Substitute":
                Substitute(attackingPokemon);
                break;
        }
    }

    // Always inflicts 40 HP
    public void DragonRage(GodotObject defending)
    {
        int damage = 40;
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // The heavier the opponent, the stronger the attack
    public void LowKick(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        float multiplier = 1.5f;
        int damage = Mathf.RoundToInt((1 + defendingPokemon.Weight) * multiplier);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // Inflicts damage equal to user's level
    public void SeismicToss(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        float multiplier = 1.5f;
        int damage = Mathf.RoundToInt(defendingPokemon.Level * multiplier);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // User performs the opponent's last move
    public void MirrorMove(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetAttackingPokemon(defending);

        PokemonMove pokemonMove = attackingPokemon.Move;
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.AttackPokemonEnemy(pokemonMove);
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.AttackPokemon(pokemonMove);
        }
        string copiedMoveMessage = $"{attackingPokemon.Name} Has Copied {defendingPokemon.Name}'s Move And Used {pokemonMove.Name}";
        PrintRich.PrintLine(TextColor.Orange, copiedMoveMessage);
    }

    // Inflicts damage equal to user's level
    public void NightShade(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        float multiplier = 1.5f;
        int damage = Mathf.RoundToInt(defendingPokemon.Level * multiplier);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // User performs the opponent's last move
    public void Mimic(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetAttackingPokemon(defending);

        PokemonMove pokemonMove = attackingPokemon.Move;
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.AttackPokemonEnemy(pokemonMove);
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.AttackPokemon(pokemonMove);
        }
        string copiedMoveMessage = $"{attackingPokemon.Name} Has Copied {defendingPokemon.Name}'s Move And Used {pokemonMove.Name}";
        PrintRich.PrintLine(TextColor.Orange, copiedMoveMessage);
    }

    // Money is earned.
    public void PayDay()
    {
        PokemonTD.AddPokeDollars(5);
    }

    // Always inflicts 20 HP
    public void SonicBoom(GodotObject defending)
    {
        int damage = 20;
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // Always takes off half of the opponent's HP
    public void SuperFang(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        int damage = Mathf.RoundToInt(defendingPokemon.Stats.HP / 2);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // Inflicts damage 50-150% of user's level
    public void Psywave(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float percentage = RNG.RandfRange(0.5f, 1.5f);
        int damage = Mathf.RoundToInt(defendingPokemon.Level * percentage);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // Halves damage from Special attacks for 5 turns
    public void LightScreen(GodotObject attacking)
    {
        PokemonEffects attackingPokemonEffects = PokemonCombat.Instance.GetAttackingPokemonEffects(attacking);
        attackingPokemonEffects.LightScreenCount = 5;
    }

    // Halves damage from Physical attacks for 5 turns
    public void Reflect(GodotObject attacking)
    {
        PokemonEffects attackingPokemonEffects = PokemonCombat.Instance.GetAttackingPokemonEffects(attacking);
        attackingPokemonEffects.ReflectCount = 5;
    }

    // ? Allow to enemy to teleport between Enemy positions
    // ? Used by Roar & Whirlwind
    // Allows user to move between areas on the map
    public void Teleport(GodotObject attacking)
    {
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Control>().GetOwnerOrNull<PokemonStage>();
            PokemonStageSlot randomPokemonStageSlot = pokemonStage.GetRandomPokemonStageSlot();
            if (randomPokemonStageSlot.Pokemon == null)
            {
                randomPokemonStageSlot.UpdateSlot(pokemonStageSlot.Pokemon);
                randomPokemonStageSlot.PokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;
                pokemonStageSlot.UpdateSlot(null);
            }
            else
            {
                randomPokemonStageSlot.SwapPokemon(pokemonStageSlot);
            }
        }
    }

    // When hit by a Physical Attack, user strikes back with 2x power.
    public void Counter(GodotObject attacking)
    {
        PokemonEffects attackingPokemonEffects = PokemonCombat.Instance.GetAttackingPokemonEffects(attacking);
        attackingPokemonEffects.HasCounter = true;
    }

    // Opponent can't attack for one turn.
    public void Disable(GodotObject defending)
    {
        PokemonEffects defendingPokemonEffects = PokemonCombat.Instance.GetDefendingPokemonEffects(defending);
        defendingPokemonEffects.HasMoveSkipped = true;
    }

    // Changes user's type to that of its first move.
    public void Conversion(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        PokemonManager.Instance.ChangeTypes(attackingPokemon, defendingPokemon);
    }

    // In battles, the opponent switches areas. In the wild, the Pokemon runs.
    public void Roar(GodotObject defending)
    {
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            Teleport(pokemonStageSlot);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.IsMovingForward = false;
        }
    }

    // In battles, the opponent switches areas. In the wild, the Pokemon runs.
    public void Whirlwind(GodotObject defending)
    {
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            Teleport(pokemonStageSlot);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.IsMovingForward = false;
        }
    }

    // User takes on the form and attacks of the opponent.
    public void Transform(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetDefendingPokemon(defending);

        PokemonEffects attackingPokemonEffects = PokemonCombat.Instance.GetAttackingPokemonEffects(attacking);
        attackingPokemonEffects.PokemonTransform = PokemonManager.Instance.GetPokemonCopy(attackingPokemon);
        PokemonManager.Instance.ChangePokemon(attackingPokemon, defendingPokemon);

        if (attacking is not PokemonStageSlot pokemonStageSlot) return;

        PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTransformed, attackingPokemon, pokemonStageSlot.PokemonTeamIndex);
    }

    // User attacks, but is then inactive for 4 seconds.
    public void Thrash(GodotObject attacking)
    {
        PokemonStatusCondition.Instance.FreezePokemon(attacking, StatusCondition.None, 4);
    }

    // User attacks, but is then inactive for 4 seconds.
    public void PetalDance(GodotObject attacking)
    {
        PokemonStatusCondition.Instance.FreezePokemon(attacking, StatusCondition.None,  4);
    }

    // Creates a decoy that takes hits.
    public async void Substitute(GodotObject attacking)
    {
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            int substituteIndex = pokemonStageSlot.Pokemon.Moves.FindIndex(move => move.Name == "Substitute");

            pokemonStageSlot.Effects.HasSubstitute = true;
            pokemonStageSlot.Pokemon.GetNextMove();

            await ToSignal(GetTree().CreateTimer(2.5f / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);

            pokemonStageSlot.Effects.HasSubstitute = false;
            pokemonStageSlot.Pokemon.Move = pokemonStageSlot.Pokemon.Moves[substituteIndex];
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Effects.HasSubstitute = true;

            StagePath stagePath = pokemonEnemy.GetParentOrNull<PathFollow2D>().GetOwnerOrNull<StagePath>();
            PokemonStage pokemonStage = stagePath.GetOwnerOrNull<PokemonStage>();

            PokemonEnemy pokemonClone = pokemonStage.GetPokemonClone(pokemonEnemy);
            pokemonClone.Fainted += (enemy) => pokemonEnemy.Effects.HasSubstitute = false;

            pokemonStage.SpawnClone(pokemonEnemy, pokemonClone);
        }
    }
}
