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
        Pokemon attackingPokemon = GetPokemon(attacking);
        Pokemon defendingPokemon = GetPokemon(defending);

        if (pokemonMove.Power == 0)
        {
            string usedMessage = PrintRich.GetPokemonMoveUsedMessage(attackingPokemon, defendingPokemon, pokemonMove);
            PrintRich.PrintLine(TextColor.Orange, usedMessage);
            return;
        }

        if (pokemonMove.Name == "Dream Eater" && !defendingPokemon.HasStatusCondition(StatusCondition.Sleep)) return;

        DamagePokemon(attacking, defending, pokemonMove);
    }

    public void DealDamage(GodotObject defending, int damage)
    {
        if (defending is PokemonStageSlot pokemonStageSlot)
        {
            if (!pokemonStageSlot.IsRecovering) pokemonStageSlot.DamagePokemon(damage);
        }
        else if (defending is PokemonEnemy pokemonEnemy)
        {
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.DamagePokemon(damage);
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
            if (IsInstanceValid(pokemonEnemy)) pokemonEnemy.HealPokemon(health);
        }
    }

    private void DamagePokemon(GodotObject attacking, GodotObject defending, PokemonMove pokemonMove)
    {
        Pokemon attackingPokemon = GetPokemon(attacking);
        Pokemon defendingPokemon = GetPokemon(defending);

        int randomHitCount = PokemonMoveEffect.Instance.GetRandomHitCount(pokemonMove);
        for (int i = 0; i < randomHitCount; i++)
        {
            int damage = GetDamage(attacking, pokemonMove, defending);
            DealDamage(defending, damage);

            bool canPokemonCounter = CanPokemonCounter(defendingPokemon.Effects.HasCounter, pokemonMove);
            if (canPokemonCounter) DealCounterDamage(attacking, damage, defending);

            string usedMessage = PrintRich.GetPokemonMoveUsedMessage(attackingPokemon, defendingPokemon, pokemonMove);
            string damageMessage = PrintRich.GetPokemonMoveDamageMessage(defendingPokemon, pokemonMove, damage);
            PrintRich.PrintLine(TextColor.Orange, usedMessage + damageMessage);
        }
    }

    public async void DamagePokemonOverTime(GodotObject defending, int iterations, StatusCondition statusCondition)
    {
        float percentage = statusCondition == StatusCondition.None ? .125f /* 1/8 */ : .0625f; /* 1/16 */ 

        int pokemonTeamIndex = 0;
        PokemonStage pokemonStage = null;
        Pokemon pokemon = GetPokemon(defending);

        if (defending is PokemonStageSlot)
        {
            PokemonStageSlot pokemonStageSlot = defending as PokemonStageSlot;
            pokemonTeamIndex = pokemonStageSlot.PokemonTeamIndex;
            pokemonStage = pokemonStageSlot.GetParentOrNull<Node>().GetOwnerOrNull<PokemonStage>();
        }

        for (int i = 0; i < iterations; i++)
        {
            if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, PokemonSignals.SignalName.PressedPlay);

            if (defending is PokemonStageSlot) defending = pokemonStage.FindPokemonStageSlot(pokemonTeamIndex); // Gets the Pokemon wherever it is on the stage

            Pokemon defendingPokemon = GetPokemon(defending);
            if (defendingPokemon == null) return;

            int damage = GetDamage(defendingPokemon, percentage);

            DealDamage(defending, damage);
            await ToSignal(GetTree().CreateTimer(1 / PokemonTD.GameSpeed), SceneTreeTimer.SignalName.Timeout);

            percentage = statusCondition == StatusCondition.BadlyPoisoned ? percentage *= 2 : percentage; // Badly Poisoned damage increases
        }

        PokemonStatusCondition.Instance.ApplyStatusColor(defending, StatusCondition.None);
        PokemonStatusCondition.Instance.RemoveStatusCondition(defending, statusCondition);
        pokemon.RemoveStatusCondition(statusCondition);
    }

    // If pokemon move has a status condition that the pokemon already has, get the next pokemon in the queue
    public GodotObject GetNextTarget<T>(List<T> targets, PokemonMove pokemonMove)
    {
        List<GodotObject> targetObjects = GetTargetObjects(targets);
        GodotObject nextTargetObject = targetObjects[0];
        foreach (GodotObject targetObject in targetObjects)
        {
            Pokemon pokemon = GetPokemon(targetObject);
            if (pokemon == null) continue;

            GodotObject result = CheckStats(targetObject, pokemon, pokemonMove);
            result = CheckStatusConditions(targetObject, pokemon, pokemonMove);
            
            if (result != null) nextTargetObject = result;
        }
        return nextTargetObject;
    }

    // Converts the list 
    private List<GodotObject> GetTargetObjects<T>(List<T> targets)
    {
        List<GodotObject> targetObjects = new List<GodotObject>();
        if (targets is List<PokemonStageSlot> pokemonStageSlots)
        {
            targetObjects.AddRange(pokemonStageSlots);
        }
        else if (targets is List<PokemonEnemy> pokemonEnemies)
        {
            targetObjects.AddRange(pokemonEnemies);
        }
        return targetObjects;
    }

    // See if the pokemon from the object has any stats already debuffed. If it doesn't have any, return the object.
    private GodotObject CheckStats(GodotObject attacking, Pokemon pokemon, PokemonMove pokemonMove)
    {
        List<StatMove> decreasingStatMoves = PokemonStatMoves.Instance.FindDecreasingStatMoves(pokemonMove);
        foreach (StatMove decreasingStatMove in decreasingStatMoves)
        {
            bool pokemonHasDebuff = pokemon.Debuffs.Contains(decreasingStatMove.PokemonStat);
            if (pokemonHasDebuff) continue;

            return attacking;
        }
        return null;
    }

    // See if the pokemon from the object has any of the status condition the pokemon move gives. If it doesn't have any, return the object.
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
        Pokemon attackingPokemon = GetPokemon(attacking);
        Pokemon defendingPokemon = GetPokemon(defending);

        int power = GetPokemonMovePower(defending, pokemonMove);
        float criticalDamageMultiplier = GetCriticalDamageMultiplier(attackingPokemon, pokemonMove);
        float attackDefenseRatio = GetAttackDefenseRatio(attackingPokemon, pokemonMove, defendingPokemon);

        float damage = (((5 * attackingPokemon.Level * criticalDamageMultiplier) + 4) * power * attackDefenseRatio / 25) + 4;
        damage = ApplyTypeMultipliers(defendingPokemon, pokemonMove, damage);
        damage = HalveDamage(defending, pokemonMove, damage);

        if (defendingPokemon.Effects.UsedDig && pokemonMove.Name != "Earthquake") return 0;

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
        Pokemon attackingPokemon = GetPokemon(attacking);
        Pokemon defendingPokemon = GetPokemon(defending);

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

    public Pokemon GetPokemon(GodotObject godotObject)
    {
        Pokemon pokemon = null;
        if (godotObject is PokemonStageSlot pokemonStageSlot)
        {
            pokemon = pokemonStageSlot.Pokemon;
        }
        else if (godotObject is PokemonEnemy pokemonEnemy)
        {
            pokemon = pokemonEnemy.Pokemon;
        }
        return pokemon;
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
        if (!hasPassedAccuracy)
        {
            ApplyMissDamage(attackingPokemon, pokemonMove);

            if (pokemonMove.Accuracy == 0)
            {
                // Print message to console
                string missedMessage = $"{attackingPokemon.Name}'s {pokemonMove.Name} Missed";
                PrintRich.PrintLine(TextColor.Purple, missedMessage);
            }
        }

        return hasPassedAccuracy;
    }

    private void ApplyMissDamage(GodotObject attacking, PokemonMove pokemonMove)
    {
        bool isMissMove = PokemonMoveEffect.Instance.MissMoves.IsMissMove(pokemonMove);
        if (!isMissMove) return;

        Pokemon attackingPokemon = GetPokemon(attacking);
        int damage = attackingPokemon.Stats.HP / 2;
        DealDamage(attacking, damage);

        string missedMessage = $"{attackingPokemon.Name} Lost Half Of It's HP";
        PrintRich.PrintLine(TextColor.Orange, missedMessage);
    }

    private int GetAccuracy(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        int accuracy = Mathf.RoundToInt((attackingPokemon.Stats.Accuracy - defendingPokemon.Stats.Evasion) * pokemonMove.Accuracy);
        return accuracy;
    }

    // Earthquake doubles it's power if opposing Pokemon used dig
    private int GetPokemonMovePower(GodotObject defending, PokemonMove pokemonMove)
    {
        Pokemon defendingPokemon = GetPokemon(defending);

        int pokemonMovePower = pokemonMove.Power;
        if (pokemonMove.Name != "Earthquake") return pokemonMovePower;

        if (defendingPokemon.Effects.UsedDig) pokemonMovePower *= 2;
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
        Pokemon defendingPokemon = GetPokemon(defending);

        if (defendingPokemon.Effects.HasLightScreen && pokemonMove.Category == MoveCategory.Special)
        {
            damage /= 2;

            // Print message to console
            string usedLightScreenMessage = $"{defendingPokemon.Name} Halved The Damage With Light Screen";
            PrintRich.PrintLine(TextColor.Orange, usedLightScreenMessage);
        }
        else if (defendingPokemon.Effects.HasReflect && pokemonMove.Category == MoveCategory.Physical)
        {
            damage /= 2;

            // Print message to console
            string usedReflectMessage = $"{defendingPokemon.Name} Halved The Damage With Reflect";
            PrintRich.PrintLine(TextColor.Orange, usedReflectMessage);
        }
        return damage;
    }

    private float GetAttackDefenseRatio(Pokemon attackingPokemon, PokemonMove pokemonMove, Pokemon defendingPokemon)
    {
        float specialRatio = (float)attackingPokemon.Stats.SpecialAttack / defendingPokemon.Stats.SpecialDefense;
        float normalRatio = (float)attackingPokemon.Stats.Attack / defendingPokemon.Stats.Defense;

        float attackDefenseRatio = pokemonMove.Category == MoveCategory.Special ? specialRatio : normalRatio;

        return (float)Math.Round(attackDefenseRatio, 2) / 50;
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

    // Gets a move that's going to be used in combat
    public PokemonMove GetCombatMove(Pokemon attackingPokemon, Pokemon defendingPokemon, PokemonMove pokemonMove, int pokemonTeamIndex)
    {
        PokemonMove combatMove = pokemonMove;
        if (combatMove.Name == "Mirror Move" || combatMove.Name == "Mimic")
        {
            combatMove = PokemonMoveEffect.Instance.CopyMoves.GetCopiedPokemonMove(attackingPokemon, defendingPokemon);
            if (pokemonTeamIndex != -1) PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonCopiedMove, combatMove, pokemonTeamIndex);
        }

        combatMove = combatMove.Name == "Metronome" ? PokemonMoves.Instance.GetRandomPokemonMove() : combatMove;
        return combatMove;
    }

    public void ApplyEffects(Pokemon pokemon)
	{
		foreach (PokemonMove pokemonMove in pokemon.Moves)
		{
			switch (pokemonMove.Name)
			{
				case "Light Screen":
					pokemon.Effects.HasLightScreen = true;
					break;
				case "Reflect":
					pokemon.Effects.HasReflect = true;
					break;
				case "Mist":
					pokemon.Effects.HasMist = true;
					break;
				case "Conversion":
					pokemon.Effects.HasConversion = true;
					break;
			}
		}
	}
}