using System.Collections.Generic;
using Godot;
using GC = Godot.Collections;

namespace PokemonTD;

public enum PokemonType
{
	Normal, // #AEAB8F
	Grass, // #3BCB01
	Fire, // #FC6F0F
	Water, // #707FFE
	Fighting, // #C80001
	Flying, // #AA91FF
	Poison, // #AE04CC
	Ground, // #E7BD60
	Rock, // #C59100
	Bug, // #9CC31B
	Ghost, // #653B94
	Electric, // #FFD624
	Psychic, // #FE509B
	Ice, // #88DCED
	Dragon // #6622EF
}

public enum EffectiveType
{
	SuperEffective,
	Effective,
	NotEffective,
	NoEffect
}

public partial class PokemonTypes : Node
{
	private GC.Dictionary<string, Variant> _typeMatchupDictionary = new GC.Dictionary<string, Variant>();

	public override void _EnterTree()
	{
		PokemonTD.PokemonTypes = this;
	}

	public override void _Ready()
	{
		LoadTypeMatchupFile();
	}

	private void LoadTypeMatchupFile()
	{
		string filePath = "res://JSON/TypeMatchup.json";
		
		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		Json json = new Json();

		if (json.Parse(jsonString) != Error.Ok) return;

		string loadSuccessMessage = "Type Matchup File Successfully Loaded";
		PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
		_typeMatchupDictionary = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
	}

	public EffectiveType GetEffectiveType(float typeMultiplier) => typeMultiplier switch
	{
		2 => EffectiveType.SuperEffective,
		1 => EffectiveType.Effective,
		0.5f => EffectiveType.NotEffective,
		0 => EffectiveType.NoEffect,
		_ => EffectiveType.Effective 
	};

	public float GetTypeMultiplier(PokemonType pokemonMoveType, PokemonType pokemonEnemyType)
	{
		GC.Dictionary<string, Variant> typeDictionary = _typeMatchupDictionary[pokemonMoveType.ToString()].As<GC.Dictionary<string, Variant>>();
		return typeDictionary[pokemonEnemyType.ToString()].As<float>();
	}

	public List<float> GetTypeMultipliers(PokemonType pokemonMoveType, List<PokemonType> pokemonEnemyTypes)
	{
		GC.Dictionary<string, Variant> typeDictionary = _typeMatchupDictionary[pokemonMoveType.ToString()].As<GC.Dictionary<string, Variant>>();
		
		List<float> typeMultipliers = new List<float>();
		foreach (PokemonType pokemonEnemyType in pokemonEnemyTypes)
		{
			float typeMultiplier = typeDictionary[pokemonEnemyType.ToString()].As<float>();
			typeMultipliers.Add(typeMultiplier);
		}
		return typeMultipliers;
	}

	public Color GetTypeColor(PokemonType pokemonType)
	{
		string typeColorHex = GetTypeColorHex(pokemonType);
		return Color.FromString(typeColorHex, Colors.White);
	}

	private string GetTypeColorHex(PokemonType pokemonType) => pokemonType switch
	{
		PokemonType.Normal => "B7B8AA",
		PokemonType.Grass => "8DD651",
		PokemonType.Fire => "F85244",
		PokemonType.Water => "55AEFF",
		PokemonType.Fighting => "AA5541",
		PokemonType.Flying => "79A2FF",
		PokemonType.Poison => "A65B9C",
		PokemonType.Ground => "E5C65D",
		PokemonType.Rock => "CCBB73",
		PokemonType.Bug => "BFD01F",
		PokemonType.Ghost => "7471CF",
		PokemonType.Electric => "FEE139",
		PokemonType.Psychic => "F461B1",
		PokemonType.Ice => "88DCED",
		PokemonType.Dragon => "8A75FF",
		_ => "B7B8AA"
	};

	public Texture2D GetTypeIcon(PokemonType pokemonType)
	{
		string filePath = $"res://Assets/Images/TypeIcon/{pokemonType}TypeIcon.png";
		return ResourceLoader.Load<Texture2D>(filePath);
	}
}
