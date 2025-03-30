using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokémonTD;

public partial class PokémonStages : Node
{
	private GC.Dictionary<string, Variant> _stagesDictionary = new GC.Dictionary<string, Variant>();
	private List<PokémonStage> _pokémonStages = new List<PokémonStage>();

	public override void _EnterTree()
	{
		PokémonTD.PokémonStages = this;
	}

	public override void _Ready()
	{
		LoadStagesFile();
		SetPokémonStages();
	}

	private void LoadStagesFile()
	{
		string filePath = "res://JSON/PokémonStage.json";

		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		Json json = new Json();
		
		if (json.Parse(jsonString) != Error.Ok) return;
		
		string loadSuccessMessage = "Pokémon Stages File Successfully Loaded";
		PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
		_stagesDictionary = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
	}

	public List<PokémonStage> GetPokémonStages()
	{
		return _pokémonStages;
	}

	private void SetPokémonStages()
	{
		List<string> stageIDs = _stagesDictionary.Keys.ToList();
		foreach (string stageID in stageIDs)
		{
			GC.Dictionary<string, Variant> stageDictionary = _stagesDictionary[stageID].As<GC.Dictionary<string, Variant>>();
			PokémonStage pokémonStage = new PokémonStage();
			pokémonStage.ID = int.Parse(stageID);
			pokémonStage.WaveCount = stageDictionary["Wave Count"].As<int>();
			pokémonStage.PokémonPerWave = stageDictionary["Pokémon Per Wave"].As<int>();
			pokémonStage.PokémonLevels = stageDictionary["Pokémon Levels"].As<GC.Array<int>>().ToList();

			GC.Dictionary<string, int> pokémonDictionary = stageDictionary["Pokémon"].As<GC.Dictionary<string, int>>();
			pokémonStage.PokémonNames = pokémonDictionary.Keys.ToList();

			string pokémonCountMessage = $"Stage {stageID} Pokemon Count: {pokémonStage.PokémonNames.Count}";
			PrintRich.Print(TextColor.Blue, pokémonCountMessage);

			_pokémonStages.Add(pokémonStage);
		}
		GD.Print();
	}

	public int GetRandomEnemyCount()
	{
		int minCount = 5;
		int maxCount = 10;

		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int enemyCount = RNG.RandiRange(minCount, maxCount);

		return enemyCount;
	}

	public List<Pokémon> GetPokémonEnemies(GC.Dictionary<string, int> pokémonDictionary)
	{
		List<Pokémon> pokémonEnemies = new List<Pokémon>();
		List<string> pokémonNames = pokémonDictionary.Keys.ToList();
		foreach (string pokémonName in pokémonNames)
		{
			Pokémon pokémon = PokémonTD.PokémonManager.GetPokémon(pokémonName);
			pokémonEnemies.Add(pokémon);
		}
		return pokémonEnemies;
	}

	public PokémonStage GetPokémonStage(int pokémonStageID)
	{
		return _pokémonStages.Find(pokémonStage => pokémonStage.ID == pokémonStageID);
	}
}
