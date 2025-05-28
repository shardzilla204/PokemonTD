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

    public void DealDamage(GodotObject attacking, GodotObject defending, PokemonMove pokemonMove)
    {
        Pokemon attackingPokemon = GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = GetDefendingPokemon(defending);

        if (pokemonMove.Power == 0)
        {
            string usedMessage =  PrintRich.GetPokemonMoveUsedMessage(attackingPokemon, defendingPokemon, pokemonMove);
            PrintRich.PrintLine(TextColor.Orange, usedMessage);
            return;
        }

        if (pokemonMove.Name == "Dream Eater" && !defendingPokemon.HasStatusCondition(StatusCondition.Sleep)) return;

        DamagePokemon(attacking, pokemonMove, defending);
    }

    // Mainly for moves like Dragon Rage and Sonic Boom
    public void DealDamage(GodotObject defending, int damage)
    {
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.DamagePokemon(damage);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.DamagePokemon(damage);
        }
    }

    public void HealPokemon(GodotObject attacking, int health)
    {
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.HealPokemon(health);
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.HealPokemon(health);
        }
    }

    private void DamagePokemon(GodotObject attacking, PokemonMove pokemonMove, GodotObject defending)
    {
        Pokemon attackingPokemon = GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = GetDefendingPokemon(defending);
        PokemonEffects defendingPokemonEffects = GetDefendingPokemonEffects(defending);
        
        int randomHitCount = PokemonMoveEffect.Instance.GetRandomHitCount(pokemonMove);
        for (int i = 0; i < randomHitCount; i++)
        {
            int damage = GetDamage(attacking, pokemonMove, defending);
            DealDamage(defending, damage);

            bool canPokemonCounter = CanPokemonCounter(defendingPokemonEffects.HasCounter, pokemonMove);
            if (canPokemonCounter) DealCounterDamage(attacking, damage, defending);

            string usedMessage = PrintRich.GetPokemonMoveUsedMessage(attackingPokemon, defendingPokemon, pokemonMove);
            string damageMessage = PrintRich.GetPokemonMoveDamageMessage(defendingPokemon, pokemonMove, damage);
            PrintRich.PrintLine(TextColor.Orange, usedMessage + damageMessage);
        }
    }

    private void DamagePokemon(GodotObject defending, int damage)
    {
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot.IsActive) pokemonStageSlot.DamagePokemon(damage);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.DamagePokemon(damage);
        }
    }

    // For badly poisoned status condition
    public void DamagePokemon(GodotObject defending, float percentage)
    {
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            if (pokemonStageSlot == null) return;

            int damage = GetDamage(pokemonStageSlot.Pokemon, percentage);
            if (pokemonStageSlot.IsActive) pokemonStageSlot.DamagePokemon(damage);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            if (pokemonEnemy == null) return;

            int damage = GetDamage(pokemonEnemy.Pokemon, percentage);
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.DamagePokemon(damage);
        }
    }

    public async void DamagePokemonOverTime(GodotObject attacking, GodotObject defending, int damage, int iterations, StatusCondition statusCondition)
    {
        for (int i = 0; i < iterations; i++)
        {
            if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, Signals.SignalName.PressedPlay);

            CustomTimer timer = PokemonStatusCondition.Instance.GetDamageTimer(defending, 1);
            timer.Timeout += () => DamagePokemon(defending, damage);

            if (attacking is PokemonStageSlot)
            {
                PokemonEnemy pokemonEnemy = defending as PokemonEnemy;

                timer.TreeExiting += () =>
                {
                    if (!IsInstanceValid(pokemonEnemy)) return;

                    PokemonStatusCondition.Instance.ApplyStatusColor(pokemonEnemy, StatusCondition.None);
                    PokemonStatusCondition.Instance.RemoveEnemyStatusCondition(pokemonEnemy, statusCondition);
                };
            }
            else if (attacking is PokemonEnemy)
            {
                PokemonEnemy pokemonEnemy = attacking as PokemonEnemy;
                PokemonStageSlot pokemonStageSlot = defending as PokemonStageSlot;

                PokemonStage pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
                int pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;

                if (!IsInstanceValid(pokemonEnemy)) return;

                timer.TreeExiting += () =>
                {
                    pokemonStageSlot = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex);
                    if (!IsInstanceValid(pokemonStageSlot) || pokemonStageSlot == null) return;

                    PokemonStatusCondition.Instance.ApplyStatusColor(pokemonStageSlot, StatusCondition.None);
                    PokemonStatusCondition.Instance.RemoveStageSlotStatusCondition(pokemonStageSlot, statusCondition);
                };

                pokemonStageSlot.Retrieved += (pokemonStageSlot) => timer.QueueFree();
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
            Pokemon pokemon = pokemonStageSlot.Pokemon;
            if (pokemon == null) continue;

            PokemonStageSlot result = (PokemonStageSlot)CheckStatusConditions(pokemonStageSlot, pokemon, pokemonMove);
            if (result != null) nextPokemonStageSlot = result;
        }
        return nextPokemonStageSlot;
    }

    public PokemonEnemy GetNextPokemonEnemy(List<PokemonEnemy> pokemonEnemies, PokemonMove pokemonMove)
    {
        PokemonEnemy nextPokemonEnemy = pokemonEnemies[0];
        foreach (PokemonEnemy pokemonEnemy in pokemonEnemies)
        {
            Pokemon pokemon = pokemonEnemy.Pokemon;
            if (pokemon == null) continue;

            PokemonEnemy result = (PokemonEnemy)CheckStatusConditions(pokemonEnemy, pokemon, pokemonMove);
            if (result != null) nextPokemonEnemy = result;
        }
        return nextPokemonEnemy;
    }

    // See if the pokemon from the pokemon stage slot has any of the status condition the pokemon move gives. If it doesn't have any, return that pokemon stage slot.
    private GodotObject CheckStatusConditions(GodotObject attacking, Pokemon pokemon, PokemonMove pokemonMove)
    {
        foreach (string statusConditionName in pokemonMove.StatusCondition.Keys)
        {
            StatusCondition statusCondition = Enum.Parse<StatusCondition>(statusConditionName);
            bool pokemonHasStatusCondition = pokemon.HasStatusCondition(statusCondition);
            if (pokemonHasStatusCondition) continue;

            return attacking;
        }
        return null;
    }

    public int GetDamage(GodotObject attacking, PokemonMove pokemonMove, GodotObject defending)
    {
        Pokemon attackingPokemon = GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = GetDefendingPokemon(defending);
        PokemonEffects defendingPokemonEffects = GetDefendingPokemonEffects(defending);

        int power = GetPokemonMovePower(defending, pokemonMove);
        float criticalDamageMultiplier = GetCriticalDamageMultiplier(attackingPokemon, pokemonMove);
        float attackDefenseRatio = GetAttackDefenseRatio(attackingPokemon, pokemonMove, defendingPokemon);

        float damage = (((5 * attackingPokemon.Level * criticalDamageMultiplier) + 4) * power * attackDefenseRatio / 25) + 4;
        damage = ApplyTypeMultipliers(defendingPokemon, pokemonMove, damage);
        damage = HalveDamage(defending, pokemonMove, damage);

        if (defendingPokemonEffects.UsedDig && pokemonMove.Name != "Earthquake") return 0;

        float damageMultiplier = 1;
        if (attacking is PokemonStageSlot)
        {
            damageMultiplier = 1.85f;
        }
        else if (attacking is PokemonEnemy)
        {
            damageMultiplier = 0.65f;
        }

        return Mathf.RoundToInt(damage * damageMultiplier);
    }

    private void DealCounterDamage(GodotObject attacking, int pokemonMoveDamage, GodotObject defending)
    {
        Pokemon attackingPokemon = GetAttackingPokemon(attacking);
        Pokemon defendingPokemon = GetDefendingPokemon(defending);

        int counterDamage = GetCounterDamage(pokemonMoveDamage);
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            pokemonStageSlot.DamagePokemon(counterDamage);
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            pokemonEnemy.DamagePokemon(counterDamage);
        }

        string counteredMessage = $"{defendingPokemon.Name} Has Countered {attackingPokemon.Name} For {pokemonMoveDamage} HP";
        PrintRich.PrintLine(TextColor.Orange, counteredMessage);
    }

    // Get amount based off the Pokemon's HP
    public int GetDamage(Pokemon pokemon, float percentage)
    {
        int damageAmount = Mathf.RoundToInt(pokemon.Stats.HP * percentage);
        return damageAmount;
    }

    private bool CanPokemonCounter(bool hasCounter, PokemonMove pokemonMove)
    {
        return hasCounter && pokemonMove.Category == MoveCategory.Physical;
    }

    private int GetCounterDamage(int damage)
    {
        float counterMultiplier = 1.5f;
        return Mathf.RoundToInt(damage * counterMultiplier);
    }

    private float AppendDamage(GodotObject defending, PokemonMove pokemonMove, float damageAmount)
    {
        Pokemon defendingPokemon = GetDefendingPokemon(defending);
        damageAmount = ApplyTypeMultipliers(defendingPokemon, pokemonMove, damageAmount);
        damageAmount = HalveDamage(defending, pokemonMove, damageAmount);
        return damageAmount;
    }

    public bool HasPokemonMoveHit(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        try
        {
            bool isPokemonMoveHitGuarenteed = IsPokemonMoveHitGuarenteed(attackingPokemon, pokemonMove, defendingPokemon);
            if (isPokemonMoveHitGuarenteed) return true;

            bool hasPassedAccuracy = CheckAccuracy(attackingPokemon, pokemonMove, defendingPokemon);
            return hasPassedAccuracy;
        }
        catch (NullReferenceException)
        {
            return false;
        }
    }

    public Pokemon GetAttackingPokemon(GodotObject attacking)
    {
        Pokemon attackingPokemon = null;
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            attackingPokemon = pokemonStageSlot.Pokemon;
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            attackingPokemon = pokemonEnemy.Pokemon;
        }
        return attackingPokemon;
    }

    public Pokemon GetDefendingPokemon(GodotObject defending)
    {
        Pokemon defendingPokemon = null;
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            defendingPokemon = pokemonStageSlot.Pokemon;
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            defendingPokemon = pokemonEnemy.Pokemon;
        }
        return defendingPokemon;
    }

    public PokemonEffects GetAttackingPokemonEffects(GodotObject attacking)
    {
        PokemonEffects attackingPokemonEffects = null;
        if (attacking is PokemonStageSlot pokemonStageSlot)
        {
            attackingPokemonEffects = pokemonStageSlot.Effects;
        }
        else if (attacking is PokemonEnemy pokemonEnemy)
        {
            attackingPokemonEffects = pokemonEnemy.Effects;
        }
        return attackingPokemonEffects;
    }

    public PokemonEffects GetDefendingPokemonEffects(GodotObject defending)
    {
        PokemonEffects defendingPokemonEffects = null;
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            defendingPokemonEffects = pokemonStageSlot.Effects;
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            defendingPokemonEffects = pokemonEnemy.Effects;
        }
        return defendingPokemonEffects;
    }

    private bool IsPokemonMoveHitGuarenteed(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        if (pokemonMove.Name != "Swift" &&
            pokemonMove.Name != "Transform") return false;

        if (pokemonMove.Name == "Swift")
        {
            string usedSwiftMessage = $"{attackingPokemon.Name} Has Bypassed {defendingPokemon.Name}'s Evasion";
            PrintRich.PrintLine(TextColor.Orange, usedSwiftMessage);
        }
        return true;
    }

    private bool CheckAccuracy(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        int accuracy = GetAccuracy(attackingPokemon, pokemonMove, defendingPokemon);

        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int randomThreshold = Mathf.RoundToInt(RNG.RandfRange(0, 100));
        randomThreshold -= accuracy;

        bool hasPassedAccuracy = randomThreshold <= 0;
        if (!hasPassedAccuracy) ApplyMissDamage(attackingPokemon, pokemonMove);

        if (pokemonMove.Accuracy != 0 && !hasPassedAccuracy)
        {
            // Print message to console
            string missedMessage = $"{attackingPokemon.Name}'s {pokemonMove.Name} Missed";
            PrintRich.PrintLine(TextColor.Purple, missedMessage);
        }

        return hasPassedAccuracy;
    }

    private void ApplyMissDamage(GodotObject attacking, PokemonMove pokemonMove)
    {
        bool isMissMove = PokemonMoveEffect.Instance.MissMoves.IsMissMove(pokemonMove);
        if (!isMissMove) return;

        Pokemon attackingPokemon = GetAttackingPokemon(attacking);
        int damage = attackingPokemon.Stats.HP / 2;
        DealDamage(attacking, damage);
    }

    private int GetAccuracy(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        int accuracy = Mathf.RoundToInt((attackingPokemon.Stats.Accuracy - defendingPokemon.Stats.Evasion) * pokemonMove.Accuracy);
        return accuracy;
    }

    // Earthquake doubles it's power if opposing Pokemon used dig
    private int GetPokemonMovePower(GodotObject defending, PokemonMove pokemonMove)
    {
        PokemonEffects defendingPokemonEffects = GetDefendingPokemonEffects(defending);

        int pokemonMovePower = pokemonMove.Power;
        if (pokemonMove.Name != "Earthquake") return pokemonMovePower;

        if (defendingPokemonEffects.UsedDig) pokemonMovePower *= 2;
        return pokemonMovePower;
    }

    // Get damage based off type effectiveness
    private float ApplyTypeMultipliers(Pokemon defendingPokemon, PokemonMove pokemonMove, float damage)
    {
        List<float> typeMultipliers = PokemonTypes.Instance.GetTypeMultipliers(pokemonMove.Type, defendingPokemon.Types);
        foreach (float typeMultiplier in typeMultipliers)
        {
            damage *= typeMultiplier;
        }
        return damage;
    }

    // Light screen & reflect Pokemon moves halve damage 
    private float HalveDamage(GodotObject defending, PokemonMove pokemonMove, float damage)
    {
        Pokemon defendingPokemon = GetDefendingPokemon(defending);
        PokemonEffects defendingPokemonEffects = GetDefendingPokemonEffects(defending);

        if (defendingPokemonEffects.LightScreenCount > 0 && pokemonMove.Category == MoveCategory.Special)
        {
            damage /= 2;
            defendingPokemonEffects.LightScreenCount--;

            // Print message to console
            string usedLightScreenMessage = $"{defendingPokemon.Name} Halved The Damage With Light Screen";
            PrintRich.PrintLine(TextColor.Orange, usedLightScreenMessage);
        }
        else if (defendingPokemonEffects.ReflectCount > 0 && pokemonMove.Category == MoveCategory.Physical)
        {
            damage /= 2;
            defendingPokemonEffects.ReflectCount--;

            // Print message to console
            string usedReflectMessage = $"{defendingPokemon.Name} Halved The Damage With Reflect";
            PrintRich.PrintLine(TextColor.Orange, usedReflectMessage);
        }
        return damage;
    }

    private float GetAttackDefenseRatio(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        float specialRatio = (float) attackingPokemon.Stats.SpecialAttack / defendingPokemon.Stats.SpecialDefense;
        float normalRatio = (float) attackingPokemon.Stats.Attack / defendingPokemon.Stats.Defense;

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
        int criticalHitThresold = GetCriticalHitThreshold(attackingPokemon);

        bool IsCriticalHit = CheckCriticalHit(criticalHitRatio, criticalHitThresold);
        if (IsCriticalHit)
        {
            // Print message to console
            string criticalHitMessage = $"{attackingPokemon.Name} Has Landed A Critical Hit";
            PrintRich.PrintLine(TextColor.Purple, criticalHitMessage);
        }
        return IsCriticalHit;
    }

    private bool CheckCriticalHit(float criticalHitRatio, int criticalHitThresold)
    {
        RandomNumberGenerator RNG = new RandomNumberGenerator();
        int criticalValue = Mathf.RoundToInt(RNG.RandfRange(criticalHitRatio, criticalHitThresold));
        int randomThreshold = Mathf.RoundToInt(RNG.RandfRange(0, criticalHitThresold));
        randomThreshold -= criticalValue;

        return randomThreshold <= 0;
    }

    private int GetCriticalHitThreshold(Pokemon attackingPokemon)
    {
        PokemonMove focusEnergy = attackingPokemon.Moves.Find(move => move.Name == "Focus Energy");
        if (focusEnergy != null)
        {
            string usedFocusEnergyMessage = $"{attackingPokemon.Name} Has Increased It's Critical Hit Ratio With Focus Energy";
            PrintRich.PrintLine(TextColor.Orange, usedFocusEnergyMessage);
        }
        return focusEnergy != null ? 255 / 2 : 255;
    }
}