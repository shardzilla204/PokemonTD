using System.Collections.Generic;
using Godot;
using GC = Godot.Collections;

namespace PokémonTD;

public enum PokémonType
{
	Normal, // #B7B8AA
	Grass, // #8DD651
	Fire, // #F85244
	Water, // #55AEFF
	Fighting, // #8DD651
	Flying, // #79A2FF
	Poison, // #A65B9C
	Ground, // #E5C65D
	Rock, // #CCBB73
	Bug, // #BFD01F
	Ghost, // #7471CF
	Electric, // #FEE139
	Psychic, // #F461B1
	Ice, // #96F0FF
	Dragon // #8A75FF
}

public enum EffectiveType
{
	SuperEffective,
	Effective,
	NotEffective,
	NoEffect
}

public partial class PokémonTypes : Node
{
	private GC.Dictionary<string, Variant> _typeMatchupDictionary = new GC.Dictionary<string, Variant>();

	public override void _EnterTree()
	{
		PokémonTD.PokémonTypes = this;
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

	public float GetTypeMultiplier(PokémonType pokémonMoveType, PokémonType pokémonEnemyType)
	{
		GC.Dictionary<string, Variant> typeDictionary = _typeMatchupDictionary[pokémonMoveType.ToString()].As<GC.Dictionary<string, Variant>>();
		return typeDictionary[pokémonEnemyType.ToString()].As<float>();
	}

	public List<float> GetTypeMultipliers(PokémonType pokémonMoveType, List<PokémonType> pokémonEnemyTypes)
	{
		GC.Dictionary<string, Variant> typeDictionary = _typeMatchupDictionary[pokémonMoveType.ToString()].As<GC.Dictionary<string, Variant>>();
		
		List<float> typeMultipliers = new List<float>();
		foreach (PokémonType pokémonEnemyType  in pokémonEnemyTypes)
		{
			float typeMultiplier = typeDictionary[pokémonEnemyType.ToString()].As<float>();
			typeMultipliers.Add(typeMultiplier);
		}
		return typeMultipliers;
	}

	public Color GetTypeColor(PokémonType pokémonType)
	{
		string typeColorHex = GetTypeColorHex(pokémonType);
		return Color.FromString(typeColorHex, Colors.White);
	}

	private string GetTypeColorHex(PokémonType pokémonType) => pokémonType switch
	{
		PokémonType.Normal => "B7B8AA",
		PokémonType.Grass => "8DD651",
		PokémonType.Fire => "F85244",
		PokémonType.Water => "55AEFF",
		PokémonType.Fighting => "AA5541",
		PokémonType.Flying => "79A2FF",
		PokémonType.Poison => "A65B9C",
		PokémonType.Ground => "E5C65D",
		PokémonType.Rock => "CCBB73",
		PokémonType.Bug => "BFD01F",
		PokémonType.Ghost => "7471CF",
		PokémonType.Electric => "FEE139",
		PokémonType.Psychic => "F461B1",
		PokémonType.Ice => "88DCED",
		PokémonType.Dragon => "8A75FF",
		_ => "B7B8AA"
	};

	public Texture2D GetTypeIcon(PokémonType pokémonType)
	{
		string fileDirectory = "res://Assets/Images/TypeIcon/";
		string fileName = $"{pokémonType}TypeIcon";
		string fileExtension = ".png";

		string filePath = $"{fileDirectory}{fileName}{fileExtension}";
		
		return ResourceLoader.Load<Texture2D>(filePath);
	}
}
