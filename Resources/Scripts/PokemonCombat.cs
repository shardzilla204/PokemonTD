using Godot;
using System;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonCombat : Node
{
    private static PokemonCombat _instance;

    public static PokemonCombat Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public void DealDamage<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        int randomHitCount = PokemonMoveEffect.Instance.GetRandomHitCount(pokemonMove);
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            string usedMessage = $"{pokemonStageSlot.Pokemon.Name} Used {pokemonMove.Name} On {pokemonEnemy.Pokemon.Name} ";
            if (pokemonMove.Power == 0)
            {
                PrintRich.PrintLine(TextColor.Orange, usedMessage);
                return;
            }

            if (pokemonMove.Name == "Dream Eater" && !pokemonEnemy.Pokemon.HasStatusCondition(StatusCondition.Sleep)) return;
            
            DamagePokemonEnemy(pokemonStageSlot, pokemonMove, pokemonEnemy, randomHitCount, usedMessage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;
            string usedMessage = $"{pokemonEnemy.Pokemon.Name} Used {pokemonMove.Name} On {pokemonStageSlot.Pokemon.Name} ";
            if (pokemonMove.Power == 0)
            {
                PrintRich.PrintLine(TextColor.Red, usedMessage);
                return;
            }

            if (pokemonMove.Name == "Dream Eater" && !pokemonStageSlot.Pokemon.HasStatusCondition(StatusCondition.Sleep)) return;

            DamagePokemonStageSlot(pokemonEnemy, pokemonMove, pokemonStageSlot, randomHitCount, usedMessage);
        }
    }

    // For moves like Dragon Rage and Sonic Boom
    public void DealDamage<Defending>(Defending defendingPokemon, int damage)
    {
        if (defendingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;
            pokemonStageSlot.DamagePokemon(damage);
        }
        else if (defendingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
            pokemonEnemy.DamagePokemon(damage);
        }
    }

    // ? Comment In Value For Higher Damage
    private void DamagePokemonEnemy(PokemonStageSlot pokemonStageSlot, PokemonMove pokemonMove, PokemonEnemy pokemonEnemy, int hitCount, string usedMessage)
    {
        for (int i = 0; i < hitCount; i++)
        {
            int pokemonMoveDamage = GetPokemonMoveDamage(pokemonStageSlot, pokemonMove, pokemonEnemy) /* * 100 */;
            pokemonEnemy.DamagePokemon(pokemonMoveDamage);

            string damageMessage = PrintRich.GetDamageMessage(pokemonMoveDamage, pokemonEnemy.Pokemon, pokemonMove);
            PrintRich.PrintLine(TextColor.Orange, usedMessage + damageMessage);
        }
    }

    private void DamagePokemonStageSlot(PokemonEnemy pokemonEnemy, PokemonMove pokemonMove, PokemonStageSlot pokemonStageSlot, int hitCount, string usedMessage)
    {
        float damageReduction = 0.75f;
        for (int i = 0; i < hitCount; i++)
        {
            int pokemonMoveDamage = GetPokemonMoveDamage(pokemonEnemy, pokemonMove, pokemonStageSlot);
            pokemonMoveDamage = Mathf.RoundToInt(pokemonMoveDamage * damageReduction);
            pokemonStageSlot.DamagePokemon(pokemonMoveDamage);

            string damageMessage = PrintRich.GetDamageMessage(pokemonMoveDamage, pokemonStageSlot.Pokemon, pokemonMove);
            PrintRich.PrintLine(TextColor.Red, usedMessage + damageMessage);
        }
    }

    private void DamagePokemon<Defending>(Defending defendingPokemon, int damage)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.IsActive) pokemonStageSlot.DamagePokemon(damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.DamagePokemon(damage);
        }
    }

    public void DamagePokemon<Defending>(Defending defendingPokemon, float percentage)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot == null) return;

            int damageAmount = GetDamageAmount(pokemonStageSlot.Pokemon, percentage);
            if (pokemonStageSlot.IsActive) pokemonStageSlot.DamagePokemon(damageAmount);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy == null) return;

            int damageAmount = GetDamageAmount(pokemonEnemy.Pokemon, percentage);
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.DamagePokemon(damageAmount);
        }
    }

    public void DamagePokemonOverTime<Attacking, Defending>(Attacking attackingPokemon, Defending defendingPokemon, int healthAmount, int iterations, StatusCondition statusCondition)
    {
        for (int i = 0; i < iterations; i++)
        {
            CustomTimer timer = PokemonStatusCondition.Instance.GetTimer(1);
            timer.Timeout += () =>
            {
                DamagePokemon(defendingPokemon, healthAmount);
                timer.QueueFree();
            };

            if (attackingPokemon is PokemonStageSlot)
            {
                PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
                PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

                timer.TreeExiting += () =>
                {
                    if (!IsInstanceValid(pokemonEnemy)) return;
                    
                    PokemonStatusCondition.Instance.ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                    PokemonStatusCondition.Instance.RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
                };
            }
            else if (attackingPokemon is PokemonEnemy)
            {
                PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
                PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

                PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
                int teamSlotIndex = pokemonStageSlot.TeamSlotIndex;

                if (!IsInstanceValid(pokemonEnemy)) return;

                timer.TreeExiting += () =>
                {
                    pokemonStageSlot = pokemonStage.FindPokemonStageSlot(teamSlotIndex);
                    if (pokemonStageSlot == null) return;

                    PokemonStatusCondition.Instance.ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                    PokemonStatusCondition.Instance.RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
                };

                pokemonStageSlot.Retrieved += timer.QueueFree;
                pokemonStageSlot.Fainted += (pokemonStageSlot) => timer.QueueFree();
            }
            AddChild(timer);
        }
    }

    // If pokemon move has a status condition that the pokemon already has, get the next pokemon in the queue
    public PokemonStageSlot GetNextPokemonStageSlot(List<PokemonStageSlot> pokemonStageSlots, PokemonMove pokemonMove)
    {
        PokemonStageSlot nextPokemonStageSlot = pokemonStageSlots[0];
        foreach (PokemonStageSlot pokemonStageSlot in pokemonStageSlots)
        {
            foreach (string statusConditionName in pokemonMove.StatusCondition.Keys)
            {
                StatusCondition statusCondition = Enum.Parse<StatusCondition>(statusConditionName);
                bool pokemonHasStatusCondition = pokemonStageSlot.Pokemon.HasStatusCondition(statusCondition);
                if (!pokemonHasStatusCondition)
                {
                    nextPokemonStageSlot = pokemonStageSlot;
                    break;
                }
            }
        }
        return nextPokemonStageSlot;
    }

    public PokemonEnemy GetNextPokemonEnemy(List<PokemonEnemy> pokemonEnemies, PokemonMove pokemonMove)
    {
        PokemonEnemy nextPokemonEnemy = pokemonEnemies[0];
        foreach (PokemonEnemy pokemonEnemy in pokemonEnemies)
        {
            foreach (string statusConditionName in pokemonMove.StatusCondition.Keys)
            {
                StatusCondition statusCondition = Enum.Parse<StatusCondition>(statusConditionName);
                bool pokemonHasStatusCondition = pokemonEnemy.Pokemon.HasStatusCondition(statusCondition);
                if (!pokemonHasStatusCondition)
                {
                    nextPokemonEnemy = pokemonEnemy;
                    break;
                }
            }
        }
        return nextPokemonEnemy;
    }

    public int GetPokemonMoveDamage<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        int pokemonMoveDamage = 0;
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            if (pokemonEnemy.Effects.UsedDig && pokemonMove.Name != "Earthquake") return 0;

            pokemonMoveDamage = GetDamage(pokemonStageSlot, pokemonMove, pokemonEnemy);

            bool canCounter = CanCounter(pokemonEnemy.Effects.HasCounter, pokemonMove);
            if (!canCounter) return pokemonMoveDamage;

            int counterDamage = GetCounterDamage(pokemonMoveDamage);
            pokemonStageSlot.DamagePokemon(counterDamage);
        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            if (pokemonStageSlot.Effects.UsedDig && pokemonMove.Name != "Earthquake") return 0;

            pokemonMoveDamage = GetDamage(pokemonEnemy, pokemonMove, pokemonStageSlot);

            bool canCounter = CanCounter(pokemonEnemy.Effects.HasCounter, pokemonMove);
            if (!canCounter) return pokemonMoveDamage;

            int counterDamage = GetCounterDamage(pokemonMoveDamage);
            pokemonEnemy.DamagePokemon(counterDamage);
        }
        return pokemonMoveDamage;
    }

    // Get amount based off the Pokemon's HP
    public int GetDamageAmount(Pokemon pokemon, float percentage)
    {
        int damageAmount = Mathf.RoundToInt(pokemon.HP * percentage);
        return damageAmount;
    }

    private bool CanCounter(bool hasCounter, PokemonMove pokemonMove)
    {
        return hasCounter && pokemonMove.Category == MoveCategory.Physical;
    }

    private int GetCounterDamage(int damage)
    {
        float counterMultiplier = 1.5f;
        return Mathf.RoundToInt(damage * counterMultiplier);
    }

    private int GetDamage<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        int power = 0;
        Pokemon attacking = null;
        Pokemon defending = null;
        if (attackingPokemon is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
            PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;

            power = GetPower(pokemonEnemy, pokemonMove);
            attacking = pokemonStageSlot.Pokemon;
            defending = pokemonEnemy.Pokemon;

        }
        else if (attackingPokemon is PokemonEnemy)
        {
            PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
            PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

            power = GetPower(pokemonStageSlot, pokemonMove);
            attacking = pokemonEnemy.Pokemon;
            defending = pokemonStageSlot.Pokemon;
        }

        float criticalDamageMultiplier = GetCriticalDamageMultiplier(attacking, pokemonMove);
        float attackDefenseRatio = GetAttackDefenseRatio(attacking, pokemonMove, defending);

        float damage = (((5 * attacking.Level * criticalDamageMultiplier) + 4) * power * attackDefenseRatio / 25) + 4;
        damage = AppendDamage(defendingPokemon, pokemonMove, damage);

        return Mathf.RoundToInt(damage);
    }

    private float AppendDamage<Defending>(Defending defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            damage = ApplyTypeMultiplers(pokemonStageSlot.Pokemon, pokemonMove, damage);
            damage = HalveDamage(pokemonStageSlot, pokemonMove, damage);
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            damage = ApplyTypeMultiplers(pokemonEnemy.Pokemon, pokemonMove, damage);
            damage = HalveDamage(pokemonEnemy, pokemonMove, damage);
        }
        return damage;
    }

    public bool HasPokemonMoveHit<Attacking, Defending>(Attacking attackingPokemon, PokemonMove pokemonMove, Defending defendingPokemon)
    {
        try
        {
            if (attackingPokemon is PokemonStageSlot)
            {
                PokemonStageSlot pokemonStageSlot = attackingPokemon as PokemonStageSlot;
                PokemonEnemy pokemonEnemy = defendingPokemon as PokemonEnemy;
                
                if (pokemonMove.Name == "Swift" || pokemonMove.Name == "Transform")
                {
                    if (pokemonMove.Name == "Swift")
                    {
                        string usedSwiftMessage = $"{pokemonStageSlot.Pokemon.Name} Has Bypassed {pokemonEnemy.Pokemon.Name}'s Evasion";
                        PrintRich.PrintLine(TextColor.Orange, usedSwiftMessage);
                    }
                    return true;
                }

                bool hasPassedAccuracy = HasPassedAccuracy(pokemonStageSlot.Pokemon, pokemonMove, pokemonEnemy.Pokemon);
                if (!hasPassedAccuracy) ApplyMissDamage(pokemonStageSlot, pokemonMove);

                if (pokemonMove.Accuracy != 0 && !hasPassedAccuracy)
                {
                    // Print Message To Console
                    string missedMessage = $"{pokemonStageSlot.Pokemon.Name}'s {pokemonMove.Name} Missed";
                    PrintRich.PrintLine(TextColor.Purple, missedMessage);
                }

                return hasPassedAccuracy;
            }
            else if (attackingPokemon is PokemonEnemy)
            {
                PokemonEnemy pokemonEnemy = attackingPokemon as PokemonEnemy;
                PokemonStageSlot pokemonStageSlot = defendingPokemon as PokemonStageSlot;

                if (pokemonMove.Name == "Swift" || pokemonMove.Name == "Transform")
                {
                    if (pokemonMove.Name == "Swift")
                    {
                        string usedSwiftMessage = $"{pokemonEnemy.Pokemon.Name} Has Bypassed {pokemonStageSlot.Pokemon.Name}'s Evasion";
                        PrintRich.PrintLine(TextColor.Orange, usedSwiftMessage);
                    }
                    return true;
                }

                bool hasPassedAccuracy = HasPassedAccuracy(pokemonEnemy.Pokemon, pokemonMove, pokemonStageSlot.Pokemon);
                if (!hasPassedAccuracy) ApplyMissDamage(pokemonEnemy, pokemonMove);

                return hasPassedAccuracy;
            }

            return false;
        }
        catch (NullReferenceException error)
        {
            GD.PrintErr(error);
            return false;
        }
    }

    private bool HasPassedAccuracy(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        int accuracy = GetAccuracy(attackingPokemon, pokemonMove, defendingPokemon);

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomThreshold = Mathf.RoundToInt(RNG.RandfRange(0, 100));
        randomThreshold -= accuracy;

        return randomThreshold <= 0;
    }

    private void ApplyMissDamage<Attacking>(Attacking attackingPokemon, PokemonMove pokemonMove)
    {
        bool isMissMove = PokemonMoveEffect.Instance.MissMoves.IsMissMove(pokemonMove);
        if (!isMissMove) return;

        if (attackingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            int damage = pokemonStageSlot.Pokemon.HP / 2;
            pokemonStageSlot.DamagePokemon(damage);

        }
        else if (attackingPokemon is PokemonEnemy pokemonEnemy)
        {
            int damage = pokemonEnemy.Pokemon.HP / 2;
            pokemonEnemy.DamagePokemon(damage);
        }
    }

    private int GetAccuracy(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        return Mathf.RoundToInt((attackingPokemon.Accuracy - defendingPokemon.Evasion) * pokemonMove.Accuracy);
    }

    // Earthquake Doubles It's Power If Opposing Pokemon Used Dig
    private int GetPower<Defending>(Defending defendingPokemon, PokemonMove pokemonMove)
    {
        int power = pokemonMove.Power;
        if (pokemonMove.Name != "Earthquake") return power;

        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.Effects.UsedDig) power *= 2;
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.Effects.UsedDig) power *= 2;
        }
        return power;
    }

    private float ApplyTypeMultiplers(Pokemon defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        List<float> typeMultipliers = PokemonTypes.Instance.GetTypeMultipliers(pokemonMove.Type, defendingPokemon.Types);
        foreach (float typeMultiplier in typeMultipliers)
        {
            damage *= typeMultiplier;
        }
        return damage;
    }

    // Light Screen & Reflect Pokemon Moves Halve Damage 
    private float HalveDamage<Defending>(Defending defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        if (defendingPokemon is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.Effects.LightScreenCount > 0 && pokemonMove.Category == MoveCategory.Special)
            {
                damage /= 2;
                pokemonStageSlot.Effects.LightScreenCount--;

                string usedLightScreenMessage = $"{pokemonStageSlot.Pokemon.Name} Halved The Damage With Light Screen";
                PrintRich.PrintLine(TextColor.Orange, usedLightScreenMessage);
            }
            else if (pokemonStageSlot.Effects.ReflectCount > 0 && pokemonMove.Category == MoveCategory.Physical)
            {
                damage /= 2;
                pokemonStageSlot.Effects.ReflectCount--;

                string usedReflectMessage = $"{pokemonStageSlot.Pokemon.Name} Halved The Damage With Reflect";
                PrintRich.PrintLine(TextColor.Orange, usedReflectMessage);
            }
        }
        else if (defendingPokemon is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy.Effects.LightScreenCount > 0 && pokemonMove.Category == MoveCategory.Special)
            {
                damage /= 2;
                pokemonEnemy.Effects.LightScreenCount--;

                string usedLightScreenMessage = $"{pokemonEnemy.Pokemon.Name} Halved The Damage With Light Screen";
                PrintRich.PrintLine(TextColor.Orange, usedLightScreenMessage);
            }
            else if (pokemonEnemy.Effects.ReflectCount > 0 && pokemonMove.Category == MoveCategory.Physical)
            {
                damage /= 2;
                pokemonEnemy.Effects.ReflectCount--;

                string usedReflectMessage = $"{pokemonEnemy.Pokemon.Name} Halved The Damage With Reflect";
                PrintRich.PrintLine(TextColor.Orange, usedReflectMessage);
            }
        }
        return damage;
    }

    private float GetAttackDefenseRatio(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        float specialRatio = (float) attackingPokemon.SpecialAttack / defendingPokemon.SpecialDefense;
        float normalRatio = (float) attackingPokemon.Attack / defendingPokemon.Defense;

        float attackDefenseRatio = pokemonMove.Category == MoveCategory.Special ? specialRatio : normalRatio;

        return (float) Math.Round(attackDefenseRatio, 2) / 50;
    }

    private float GetCriticalDamageMultiplier(Pokemon pokemon, PokemonMove pokemonMove)
    {
        return IsCriticalHit(pokemon, pokemonMove) ? 2 : 1;
    }

    private bool IsCriticalHit(Pokemon attackingPokemon, PokemonMove pokemonMove)
    {
        float criticalHitRatio = PokemonMoveEffect.Instance.GetCriticalHitRatio(attackingPokemon, pokemonMove);
        PokemonMove focusEnergy = attackingPokemon.Moves.Find(move => move.Name == "Focus Energy");
        if (focusEnergy != null)
        {
            string usedFocusEnergyMessage = $"{attackingPokemon.Name} Has Increased It's Critical Hit Ratio With Focus Energy";
            PrintRich.PrintLine(TextColor.Orange, usedFocusEnergyMessage);
        }
        
        int maxThreshold = focusEnergy != null ? 255 / 2 : 255;

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int criticalValue = Mathf.RoundToInt(RNG.RandfRange(criticalHitRatio, maxThreshold));
        int randomThreshold = Mathf.RoundToInt(RNG.RandfRange(0, maxThreshold));
        randomThreshold -= criticalValue;

        bool isCriticalHit = randomThreshold <= 0;
        if (isCriticalHit)
        {
            // Print Message To Console
            string criticalHitMessage = $"{attackingPokemon.Name} Has Landed A Critical Hit";
            PrintRich.PrintLine(TextColor.Purple, criticalHitMessage);
        }
        return isCriticalHit;
    }
}
