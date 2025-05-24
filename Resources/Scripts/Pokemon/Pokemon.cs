using Godot;
using GC = Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public enum EvolutionStone
{
	None,
	Fire,
	Water,
	Thunder,
	Leaf,
	Moon
}

public enum Gender
{
	Male,
	Female
}

public enum PokemonStat
{
	HP,
	Attack,
	Defense,
	SpecialAttack,
	SpecialDefense,
	Speed,
	Accuracy,
	Evasion
}

public partial class Pokemon : Node
{
	public new string Name;
	public string NationalNumber;
	public string Species;
   	public string Description;
	public float Height;
	public float Weight;
   	public Texture2D Sprite;
   	public List<PokemonType> Types = new List<PokemonType>();
	public List<PokemonMove> Moves = new List<PokemonMove>();

	public int HP;
	public int MaxHP;
	public int Attack;
	public int Defense;
	public int SpecialAttack;
	public int SpecialDefense;
	public int Speed;
	public float Accuracy = 1;
	public float Evasion = 0;

   	public int Level = PokemonTD.MinPokemonLevel;
	public PokemonExperience Experience;

	public Gender Gender;
	public PokemonMove Move;

	private List<StatusCondition> _statusConditions = new List<StatusCondition>();

	public Pokemon() {}

	public Pokemon(string pokemonName, GC.Dictionary<string, Variant> pokemonDictionary, GC.Array<string> pokemonTypes, GC.Dictionary<string, Variant> pokemonStats)
	{
		Name = pokemonName;
		NationalNumber = pokemonDictionary["National Number"].As<string>();
		Species = pokemonDictionary["Species"].As<string>();
		Height = pokemonDictionary["Height"].As<float>();
		Weight = pokemonDictionary["Weight"].As<float>();
		Description = pokemonDictionary["Description"].As<string>();
		Sprite = PokemonManager.Instance.GetPokemonSprite(pokemonName);

		int experienceYield = pokemonDictionary["Base Experience Yield"].As<int>();
		Experience = new PokemonExperience(experienceYield);

		foreach (string pokemonType in pokemonTypes)
		{
			Types.Add(Enum.Parse<PokemonType>(pokemonType));
		}

		HP = pokemonStats["HP"].As<int>();
		MaxHP = pokemonStats["HP"].As<int>();
		Attack = pokemonStats["Attack"].As<int>();
		Defense = pokemonStats["Defense"].As<int>();
		SpecialAttack = pokemonStats["Special Attack"].As<int>();
		SpecialDefense = pokemonStats["Special Defense"].As<int>();
		Speed = pokemonStats["Speed"].As<int>();

		if (pokemonName.Contains("Nido"))
		{
			AssignGender(pokemonName);
			return;
		}
		Gender = PokemonManager.Instance.GetRandomGender();
	}

	// For Nidoran Female & Nidoran Male
	private void AssignGender(string pokemonName)
	{
		foreach (string nidoranFemaleString in PokemonManager.Instance.NidoranFemaleStrings)
		{
			if (!pokemonName.Contains(nidoranFemaleString)) continue;
			
			Gender = Gender.Female;
			return;
		}

		foreach (string nidoranMaleString in PokemonManager.Instance.NidoranMaleStrings)
		{
			if (!pokemonName.Contains(nidoranMaleString)) continue;
			
			Gender = Gender.Male;
			return;
		}
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
		_statusConditions.Add(statusCondition);
	}

	public void RemoveStatusCondition(StatusCondition statusCondition)
	{
		_statusConditions.Remove(statusCondition);
	}

	public void RemoveAllStatusConditions()
	{
		_statusConditions.Clear();
	}

	public List<StatusCondition> GetStatusConditions()
	{
		return _statusConditions;
	}

	public bool HasStatusCondition(StatusCondition statusCondition)
	{
		return _statusConditions.Contains(statusCondition);
	}

	// Set Current Move
	public void GetNextMove()
	{
		// Filter out most pokemon moves that give stat increases 
		for (int i = 0; i < Moves.Count; i++)
		{
			// Skip to current move
			PokemonMove nextPokemonMove = Moves.SkipWhile(move => move != Moves[i]).FirstOrDefault();
			bool hasIncreasingStatChanges = PokemonStats.Instance.HasIncreasingStatChanges(nextPokemonMove) && (nextPokemonMove.Name != "Skull Bash" || nextPokemonMove.Name != "Rage");
			if (hasIncreasingStatChanges || nextPokemonMove.Name == "Focus Energy") continue;

			Move = nextPokemonMove;
			break;
		}
	}

	public void IncreaseLevel(int levels)
	{
		Level += levels;
		Level = Mathf.Clamp(Level, 1, PokemonTD.MaxPokemonLevel);

		MaxHP = PokemonManager.Instance.GetPokemonHP(this);
		HP = PokemonManager.Instance.GetPokemonHP(this);
		
		PokemonManager.Instance.SetPokemonStats(this);
	}

	public void SetLevel(int level)
	{
		// Default to the minimum level if below the threshold
		level = level < PokemonTD.MinPokemonLevel ? PokemonTD.MinPokemonLevel : level;
		Level = PokemonTD.AreLevelsRandomized ? PokemonTD.GetRandomLevel() : level;
	}
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
