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
        "Mirror Move",
        "Night Shade",
        "Mimic",
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

    public void ApplyUniqueMove<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
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
            case "Mirror Move":
                MirrorMove(attackingPokemon, defendingPokemon);
                break;
            case "Night Shade":
                MirrorMove(attackingPokemon, defendingPokemon);
                break;
            case "Mimic":
                Mimic(attackingPokemon, defendingPokemon);
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
            case "Surf":
                Surf(attackingPokemon, pokemonMove);
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
    public void DragonRage<Defending>(Defending defendingPokemon)
    {
        int damage = 40;
        PokemonCombat.Instance.DealDamage(defendingPokemon, damage);
    }

    // The heavier the opponent, the stronger the attack
    public void LowKick<Defending>(Defending defendingPokemon)
    {
        int damage = 0;
        float multiplier = 1.5f;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            damage = Mathf.RoundToInt((1 + pokemonStageSlot.Pokemon.Weight) * multiplier);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            damage = Mathf.RoundToInt((1 + pokemonEnemy.Pokemon.Weight) * multiplier);
        }
        PokemonCombat.Instance.DealDamage(defendingPokemon, damage);
    }

    // Inflicts damage equal to user's level
    public void SeismicToss<Defending>(Defending defendingPokemon)
    {
        int damage = 0;
        float multiplier = 1.5f;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * multiplier);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * multiplier);
        }
        PokemonCombat.Instance.DealDamage(defendingPokemon, damage);
    }

    // User performs the opponent's last move
    public void MirrorMove<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        if (defendingPokemon is PokemonStageSlot)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            PokemonMove pokemonMove = pokemonStageSlot.Pokemon.Move;
            pokemonEnemy.AttackPokemon(pokemonMove);

            string copiedMoveMessage = $"{pokemonStageSlot.Pokemon.Name} Has Copied {pokemonEnemy.Pokemon.Name}'s Move And Used {pokemonMove.Name}";
            PrintRich.PrintLine(TextColor.Orange, copiedMoveMessage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            PokemonMove pokemonMove = pokemonEnemy.Pokemon.Move;
            pokemonStageSlot.AttackPokemonEnemy(pokemonMove);

            string copiedMoveMessage = $"{pokemonEnemy.Pokemon.Name} Has Copied {pokemonStageSlot.Pokemon.Name}'s Move And Used {pokemonMove.Name}";
            PrintRich.PrintLine(TextColor.Orange, copiedMoveMessage);
        }
    }

    // Inflicts damage equal to user's level
    public void NightShade<Defending>(Defending defendingPokemon)
    {
        int damage = 0;
        float multiplier = 1.5f;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * multiplier);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * multiplier);
        }
        PokemonCombat.Instance.DealDamage(defendingPokemon, damage);
    }

    // User performs the opponent's last move
    public void Mimic<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            PokemonMove pokemonMove = pokemonStageSlot.Pokemon.Move;
            pokemonEnemy.AttackPokemon(pokemonMove);

            string copiedMoveMessage = $"{pokemonStageSlot.Pokemon.Name} Has Copied {pokemonEnemy.Pokemon.Name}'s Move And Used {pokemonMove.Name}";
            PrintRich.PrintLine(TextColor.Orange, copiedMoveMessage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            PokemonMove pokemonMove = pokemonEnemy.Pokemon.Move;
            pokemonStageSlot.AttackPokemonEnemy(pokemonMove);

            string copiedMoveMessage = $"{pokemonEnemy.Pokemon.Name} Has Copied {pokemonStageSlot.Pokemon.Name}'s Move And Used {pokemonMove.Name}";
            PrintRich.PrintLine(TextColor.Orange, copiedMoveMessage);
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
        PokemonCombat.Instance.DealDamage(defendingPokemon, damage);
    }

    // Always takes off half of the opponent's HP
    public void SuperFang<Defending>(Defending defendingPokemon)
    {
        int damage = 0;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.HP / 2);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.HP / 2);
        }
        PokemonCombat.Instance.DealDamage(defendingPokemon, damage);
    }

    // Inflicts damage 50-150% of user's level
    public void Psywave<Defending>(Defending defendingPokemon)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        float percentage = RNG.RandfRange(0.5f, 1.5f);
        int damage = 0;
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            damage = Mathf.RoundToInt(pokemonStageSlot.Pokemon.Level * percentage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            damage = Mathf.RoundToInt(pokemonEnemy.Pokemon.Level * percentage);
        }
        PokemonCombat.Instance.DealDamage(defendingPokemon, damage);
    }

    // ! No Pokemon Learns Surf On Level Up...
    // Hits all adjacent Pokemon
    public void Surf<Attacking>(Attacking attackingPokemon, PokemonMove pokemonMove)
    {
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            foreach (PokemonEnemy pokemonEnemy in pokemonStageSlot.PokemonEnemyQueue)
            {
                int pokemonMoveDamage = PokemonCombat.Instance.GetPokemonMoveDamage(pokemonStageSlot.Pokemon, pokemonMove, pokemonEnemy.Pokemon);
                pokemonEnemy.DamagePokemon(pokemonMoveDamage);
            }
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            foreach (PokemonStageSlot pokemonStageSlot in pokemonEnemy.PokemonQueue)
            {
                int pokemonMoveDamage = PokemonCombat.Instance.GetPokemonMoveDamage(pokemonEnemy.Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
                pokemonStageSlot.DamagePokemon(pokemonMoveDamage);
            }
        }
    }

    // Halves damage from Special attacks for 5 turns
    public void LightScreen<Attacking>(Attacking attackingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Effects.LightScreenCount = 5;
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Effects.LightScreenCount = 5;
        }
    }

    // Halves damage from Physical attacks for 5 turns
    public void Reflect<Attacking>(Attacking attackingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Effects.ReflectCount = 5;
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Effects.ReflectCount = 5;
        }
    }

    // ? Allow to enemy to teleport between Enemy positions
    // ! Used by Roar & Whirlwind
    // Allows user to move between areas on the map
    public void Teleport<Attacking>(Attacking attackingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Control>().GetOwnerOrNull<PokemonStage>();
            PokemonStageSlot randomPokemonStageSlot = pokemonStage.GetRandomPokemonStageSlot();
            if (randomPokemonStageSlot.Pokemon == null)
            {
                randomPokemonStageSlot.UpdateSlot(pokemonStageSlot.Pokemon);
                randomPokemonStageSlot.TeamSlotIndex = pokemonStageSlot.TeamSlotIndex;
                pokemonStageSlot.UpdateSlot(null);
            }
            else
            {
                randomPokemonStageSlot.SwapPokemon(pokemonStageSlot);
            }
        }


    }

    // PokemonCombat
    // When hit by a Physical Attack, user strikes back with 2x power.
    public void Counter<Attacking>(Attacking attackingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Effects.HasCounter = true;
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Effects.HasCounter = true;
        }
    }

    // Opponent can't attack for one turn.
    public void Disable<Defending>(Defending defendingPokemon)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.Effects.HasMoveSkipped = true;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.Effects.HasMoveSkipped = true;
        }
    }

    // Changes user's type to that of its first move.
    public void Conversion<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            PokemonManager.Instance.ChangeTypes(pokemonStageSlot.Pokemon, pokemonEnemy.Pokemon);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            PokemonManager.Instance.ChangeTypes(pokemonEnemy.Pokemon, pokemonStageSlot.Pokemon);
        }
    }

    // In battles, the opponent switches areas. In the wild, the Pokemon runs.
    public void Roar<Defending>(Defending defendingPokemon)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            Teleport(pokemonStageSlot);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.IsMovingForward = false;
        }
    }

    // In battles, the opponent switches areas. In the wild, the Pokemon runs.
    public void Whirlwind<Defending>(Defending defendingPokemon)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            Teleport(pokemonStageSlot);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.IsMovingForward = false;
        }
    }

    // User takes on the form and attacks of the opponent.
    public void Transform<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            pokemonStageSlot.Effects.PokemonTransform = PokemonManager.Instance.GetPokemonCopy(pokemonStageSlot.Pokemon);
            PokemonManager.Instance.ChangePokemon(pokemonStageSlot.Pokemon, pokemonEnemy.Pokemon);

            PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUpdated, pokemonStageSlot.Pokemon, pokemonStageSlot.TeamSlotIndex);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            pokemonEnemy.Effects.PokemonTransform = pokemonEnemy.Pokemon;
            PokemonManager.Instance.ChangePokemon(pokemonEnemy.Pokemon, pokemonStageSlot.Pokemon);
        }
    }

    // User attacks, but is then inactive for 4 seconds.
    public void Thrash<Attacking>(Attacking attackingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            PokemonStatusCondition.Instance.FreezePokemon(pokemonStageSlot, StatusCondition.None, 4);
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            PokemonStatusCondition.Instance.FreezePokemon(pokemonEnemy, StatusCondition.None,  4);
        }
    }

    // User attacks, but is then inactive for 4 seconds.
    public void PetalDance<Attacking>(Attacking attackingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            PokemonStatusCondition.Instance.FreezePokemon(pokemonStageSlot, StatusCondition.None,  4);
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            PokemonStatusCondition.Instance.FreezePokemon(pokemonEnemy, StatusCondition.None, 4);
        }
    }

    // Uses HP to creates a decoy that takes hits.
    public async void Substitute<Attacking>(Attacking attackingPokemon)
    {
        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int substituteIndex = pokemonStageSlot.Pokemon.Moves.FindIndex(move => move.Name == "Substitute");

            pokemonStageSlot.Effects.HasSubstitute = true;
            pokemonStageSlot.Pokemon.GetNextMove();

            await ToSignal(GetTree().CreateTimer(2.5f / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);

            pokemonStageSlot.Effects.HasSubstitute = false;
            pokemonStageSlot.Pokemon.Move = pokemonStageSlot.Pokemon.Moves[substituteIndex];
        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
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