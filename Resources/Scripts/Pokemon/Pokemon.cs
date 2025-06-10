using Godot;
using GC = Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class Pokemon : Node
{
	public new string Name;
	public string NationalNumber;
	public string Species;
	public string Description;
	public float Height;
	public float Weight;
	public Texture2D Sprite;

	public PokemonStats Stats = new PokemonStats();

	public List<PokemonType> Types = new List<PokemonType>();
	public List<PokemonMove> Moves = new List<PokemonMove>();
	public PokemonMove Move;

	public int Level = PokemonTD.MinPokemonLevel;
	public PokemonExperience Experience;

	public Gender Gender;
	public bool HasCanceledEvolution;

	public PokemonEffects Effects = new PokemonEffects();
	public List<PokemonStat> Debuffs = new List<PokemonStat>();
	public List<StatusCondition> StatusConditions = new List<StatusCondition>();

	public Pokemon() { }

	public Pokemon(string pokemonName, GC.Dictionary<string, Variant> pokemonDictionary, GC.Array<string> pokemonTypes, GC.Dictionary<string, Variant> pokemonStats)
	{
		Name = pokemonName;
		NationalNumber = pokemonDictionary["National Number"].As<string>();
		Species = pokemonDictionary["Species"].As<string>();
		Height = pokemonDictionary["Height"].As<float>();
		Weight = pokemonDictionary["Weight"].As<float>();
		Description = pokemonDictionary["Description"].As<string>();
		Sprite = PokemonManager.Instance.GetPokemonSprite(pokemonName);

		foreach (string pokemonType in pokemonTypes)
		{
			Types.Add(Enum.Parse<PokemonType>(pokemonType));
		}

		int experienceYield = pokemonDictionary["Base Experience Yield"].As<int>();
		Experience = new PokemonExperience(experienceYield);

		Stats = new PokemonStats(pokemonStats);
		Gender = PokemonManager.Instance.GetGender(pokemonName);
	}

	public void SetLevel(int level)
	{
		// Default to the minimum level if below the threshold
		level = level < PokemonTD.MinPokemonLevel ? PokemonTD.MinPokemonLevel : level;
		Level = PokemonTD.Debug.AreLevelsRandomized ? PokemonTD.GetRandomLevel() : level;
	}

	public void SetMoves(List<PokemonMove> pokemonMoves)
	{
		if (Moves.Count >= PokemonTD.MaxMoveCount) return;

		Moves.AddRange(pokemonMoves);
		Move = Moves[0];

		GetNextMove();
	}

	public void AddStatusCondition(StatusCondition statusCondition)
	{
		StatusConditions.Add(statusCondition);
	}

	public void RemoveStatusCondition(StatusCondition statusCondition)
	{
		StatusConditions.Remove(statusCondition);
	}

	public void ClearStatusConditions()
	{
		StatusConditions.Clear();
	}

	public bool HasStatusCondition(StatusCondition statusCondition)
	{
		return StatusConditions.Contains(statusCondition);
	}

	// Set Current Move
	public void GetNextMove()
	{
		// Filter out most pokemon moves that give stat increases 
		for (int i = 0; i < Moves.Count; i++)
		{
			// Skip to current move
			PokemonMove nextPokemonMove = Moves.SkipWhile(move => move != Moves[i]).FirstOrDefault();
			if (PokemonMoves.Instance.IsAutomaticMove(nextPokemonMove)) continue;

			Move = nextPokemonMove;
			break;
		}

		ApplyMoveEffect(Move);
	}

	public void ResetEffects(int pokemonTeamIndex)
	{
		Effects.ResetPokemon(this, pokemonTeamIndex);
	}

	public void IncreaseLevel(int levels)
	{
		Level += levels;
		Level = Mathf.Clamp(Level, 1, PokemonTD.MaxPokemonLevel);

		Stats.MaxHP = PokemonManager.Instance.GetPokemonHP(this);
		Stats.HP = PokemonManager.Instance.GetPokemonHP(this);

		PokemonManager.Instance.SetPokemonStats(this);
	}

	public void IsEnraged()
	{
		if (!Effects.HasRage) return;

		Effects.UseRage(this);
	}

	public bool IsMoveSkipped()
	{
		if (!Effects.HasMoveSkipped) return false;
		Effects.HasMoveSkipped = false;

		string skippedMessage = $"{Name} Had It's Turn Skipped";
		PrintRich.PrintLine(TextColor.Red, skippedMessage);

		return true;
	}

	public void ApplyEffects()
	{
		foreach (PokemonMove pokemonMove in Moves)
		{
			switch (pokemonMove.Name)
			{
				case "Light Screen":
					Effects.HasLightScreen = true;
					break;
				case "Reflect":
					Effects.HasReflect = true;
					break;
				case "Mist":
					Effects.HasMist = true;
					break;
				case "Conversion":
					Effects.HasConversion = true;
					break;
			}
		}
	}

	public void ApplyMoveEffect(PokemonMove pokemonMove)
	{
		Effects.ClearMoveEffects(this);
		switch (pokemonMove.Name)
		{
			case "Quick Attack":
				Effects.HasQuickAttack = true;
				break;
			case "Counter":
				Effects.HasCounter = true;
				break;
			case "Rage":
				Effects.HasRage = true;
				break;
			case "Hyper Beam":
				Effects.HasHyperBeam = true;
				break;
		}
	}
}

public partial class PokemonStats : Node
{
	public PokemonStats() { }
	
	public PokemonStats(GC.Dictionary<string, Variant> pokemonStats)
	{
		HP = pokemonStats["HP"].As<int>();
		MaxHP = pokemonStats["HP"].As<int>();
		Attack = pokemonStats["Attack"].As<int>();
		Defense = pokemonStats["Defense"].As<int>();
		SpecialAttack = pokemonStats["Special Attack"].As<int>();
		SpecialDefense = pokemonStats["Special Defense"].As<int>();
		Speed = pokemonStats["Speed"].As<int>();
	}

	public int HP;
	public int MaxHP;
	public int Attack;
	public int Defense;
	public int SpecialAttack;
	public int SpecialDefense;
	public int Speed;
	public float Accuracy = 1;
	public float Evasion = 0;
}

public partial class PokemonExperience : Node
{
	public PokemonExperience(int experienceYield)
	{
		Yield = experienceYield;
	}

	public int Yield;
	public int Min;
	public int Max = 100;
}
