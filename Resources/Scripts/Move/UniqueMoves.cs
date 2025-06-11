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

    public void ApplyUniqueMove(GodotObject attacking, GodotObject defending, PokemonMove pokemonMove)
    {
        switch (pokemonMove.Name)
        {
            case "Dragon Rage":
                DragonRage(defending);
                break;
            case "Low Kick":
                LowKick(defending);
                break;
            case "Seismic Toss":
                SeismicToss(defending);
                break;
            case "Night Shade":
                MirrorMove(attacking, defending);
                break;
            case "Pay Day":
                if (attacking is PokemonEnemy) return;
                PayDay();
                break;
            case "Sonic Boom":
                SonicBoom(attacking);
                break;
            case "Super Fang":
                SuperFang(attacking);
                break;
            case "Psywave":
                Psywave(attacking);
                break;
            case "Teleport":
                if (attacking is PokemonEnemy) return;
                Teleport(attacking);
                break;
            case "Counter":
                Counter(attacking);
                break;
            case "Disable":
                Disable(defending);
                break;
            case "Conversion":
                Conversion(attacking, defending);
                break;
            case "Roar":
                Roar(defending);
                break;
            case "Whirlwind":
                Whirlwind(defending);
                break;
            case "Transform":
                Transform(attacking, defending);
                break;
            case "Thrash":
                Thrash(attacking);
                break;
            case "Petal Dance":
                PetalDance(attacking);
                break;
            case "Substitute":
                Substitute(attacking);
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
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        float multiplier = 1.5f;
        int damage = Mathf.RoundToInt((1 + defendingPokemon.Weight) * multiplier);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // Inflicts damage equal to user's level
    public void SeismicToss(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        float multiplier = 1.5f;
        int damage = Mathf.RoundToInt(defendingPokemon.Level * multiplier);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // User performs the opponent's last move
    public void MirrorMove(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        PokemonMove pokemonMove = attackingPokemon.Move;
        if (attacking is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attacking as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defending as PokemonEnemy;
            pokemonStageSlot.AttackPokemonEnemy(pokemonEnemy, pokemonMove);
        }
        else if (attacking is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attacking as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defending as PokemonStageSlot;
            pokemonEnemy.AttackPokemon(pokemonStageSlot, pokemonMove);
        }
        string copiedMoveMessage = $"{attackingPokemon.Name} Has Copied {defendingPokemon.Name}'s Move And Used {pokemonMove.Name}";
        PrintRich.PrintLine(TextColor.Orange, copiedMoveMessage);
    }

    // Inflicts damage equal to user's level
    public void NightShade(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        float multiplier = 1.5f;
        int damage = Mathf.RoundToInt(defendingPokemon.Level * multiplier);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // User performs the opponent's last move
    public void Mimic(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        PokemonMove pokemonMove = attackingPokemon.Move;
        if (attacking is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attacking as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defending as PokemonEnemy;
            pokemonStageSlot.AttackPokemonEnemy(pokemonEnemy, pokemonMove);
        }
        else if (attacking is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attacking as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defending as PokemonStageSlot;
            pokemonEnemy.AttackPokemon(pokemonStageSlot, pokemonMove);
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
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        int damage = Mathf.RoundToInt(defendingPokemon.Stats.HP / 2);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // Inflicts damage 50-150% of user's level
    public void Psywave(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float percentage = RNG.RandfRange(0.5f, 1.5f);
        int damage = Mathf.RoundToInt(defendingPokemon.Level * percentage);
        PokemonCombat.Instance.DealDamage(defending, damage);
    }

    // ? Allow to enemy to teleport between Enemy positions
    // ? Used by Roar & Whirlwind
    // Allows user to move between areas on the map
    public void Teleport(GodotObject attacking)
    {
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            Pokemon pokemon = pokemonStageSlot.Pokemon;
            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Control>().GetOwnerOrNull<PokemonStage>();
            PokemonStageSlot randomPokemonStageSlot = pokemonStage.GetRandomPokemonStageSlot();
            if (randomPokemonStageSlot.Pokemon == null)
            {
                randomPokemonStageSlot.UpdateSlot(pokemon);
                randomPokemonStageSlot.PokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;
                randomPokemonStageSlot.Pokemon.Effects = pokemon.Effects;
                pokemonStageSlot.UpdateSlot(null);
            }
            else
            {
                randomPokemonStageSlot.SwapPokemon(pokemonStageSlot);
            }
            randomPokemonStageSlot.RefreshStatusConditions(pokemon);
        }
    }

    // When hit by a Physical Attack, user strikes back with 2x power.
    public void Counter(GodotObject attacking)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        attackingPokemon.Effects.HasCounter = true;
    }

    // ? Rework
    // Opponent can't attack for one turn.
    public void Disable(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        defendingPokemon.Effects.HasMoveSkipped = true;
    }

    // Changes user's type to that of it's target.
    public void Conversion(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        PokemonManager.Instance.ChangeTypes(attackingPokemon, defendingPokemon);
    }

    // In battles, the opponent switches areas. In the wild, the Pokemon runs.
    public void Roar(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        if (defendingPokemon == null) return;
        
        Pokemon defendingPokemonData = PokemonManager.Instance.GetPokemon(defendingPokemon.Name, defendingPokemon.Level);
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            Teleport(pokemonStageSlot);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            defendingPokemon.Stats.Speed = -Mathf.RoundToInt(defendingPokemonData.Stats.Speed * 1.25f);
            pokemonEnemy.HasRewards = false;
        }
    }

    // In battles, the opponent switches areas. In the wild, the Pokemon runs.
    public void Whirlwind(GodotObject defending)
    {
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);
        if (defendingPokemon == null) return;

        Pokemon defendingPokemonData = PokemonManager.Instance.GetPokemon(defendingPokemon.Name, defendingPokemon.Level);
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            Teleport(pokemonStageSlot);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            defendingPokemon.Stats.Speed = -Mathf.RoundToInt(defendingPokemonData.Stats.Speed * 1.25f);
            pokemonEnemy.HasRewards = false;
        }
    }

    // User takes on the form and attacks of the opponent.
    public void Transform(GodotObject attacking, GodotObject defending)
    {
        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        Pokemon defendingPokemon = PokemonCombat.Instance.GetPokemon(defending);

        attackingPokemon.Effects.PokemonTransform = PokemonManager.Instance.GetPokemonCopy(attackingPokemon);
        PokemonManager.Instance.ChangePokemon(attackingPokemon, defendingPokemon);

        if (attacking is not PokemonStageSlot pokemonStageSlot) return;

        PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonTransformed, attackingPokemon, pokemonStageSlot.PokemonTeamIndex);
    }

    // User attacks, but is then inactive for 2 seconds.
    public void Thrash(GodotObject attacking)
    {
        PokemonStatusCondition.Instance.FreezePokemon(attacking, StatusCondition.None, 2);

        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        string inactiveMessage = $"{attackingPokemon.Name} Is Now Inactive";
        PrintRich.PrintLine(TextColor.Orange, inactiveMessage);
    }

    // User attacks, but is then inactive for 2 seconds.
    public void PetalDance(GodotObject attacking)
    {
        PokemonStatusCondition.Instance.FreezePokemon(attacking, StatusCondition.None, 2);

        Pokemon attackingPokemon = PokemonCombat.Instance.GetPokemon(attacking);
        string inactiveMessage = $"{attackingPokemon.Name} Is Now Inactive";
        PrintRich.PrintLine(TextColor.Orange, inactiveMessage);
    }

    // Creates a decoy that takes hits.
    public async void Substitute(GodotObject attacking)
    {
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            int substituteIndex = pokemonStageSlot.Pokemon.Moves.FindIndex(move => move.Name == "Substitute");

            pokemonStageSlot.Pokemon.Effects.HasSubstitute = true;
            pokemonStageSlot.Pokemon.GetNextMove();

            await ToSignal(GetTree().CreateTimer(2.5f / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);

            pokemonStageSlot.Pokemon.Effects.HasSubstitute = false;
            pokemonStageSlot.Pokemon.Move = pokemonStageSlot.Pokemon.Moves[substituteIndex];
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Pokemon.Effects.HasSubstitute = true;

            StagePath stagePath = pokemonEnemy.GetParentOrNull<PathFollow2D>().GetOwnerOrNull<StagePath>();
            PokemonStage pokemonStage = stagePath.GetOwnerOrNull<PokemonStage>();

            PokemonEnemy pokemonClone = pokemonStage.GetPokemonClone(pokemonEnemy);
            pokemonClone.Fainted += (enemy) => pokemonEnemy.Pokemon.Effects.HasSubstitute = false;

            pokemonStage.SpawnClone(pokemonEnemy, pokemonClone);
        }
    }
}
