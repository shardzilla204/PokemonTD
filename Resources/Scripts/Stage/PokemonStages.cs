using Godot;
using GC = Godot.Collections;

using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonStages : Node
{
	private static PokemonStages _instance;

	public static PokemonStages Instance
	{
		get => _instance;
		private set
		{
			if (_instance == null) _instance = value;
		}
	}

	private GC.Dictionary<string, Variant> _stagesDictionary = new GC.Dictionary<string, Variant>();
	private List<PokemonStage> _pokemonStages = new List<PokemonStage>();
	private GC.Dictionary<int, bool> _stageCompletionList = new GC.Dictionary<int, bool>()
	{
		{ 1, false },
		{ 2, false },
		{ 3, false },
		{ 4, false },
		{ 5, false },
		{ 6, false }
	};

	public override void _EnterTree()
	{
		Instance = this;

		PokemonTD.Signals.GameReset += () =>
		{
			foreach (int key in _stageCompletionList.Keys)
			{
				_stageCompletionList[key] = false;
			}
        };

		LoadStagesFile();
		SetPokemonStages();
	}

	private void LoadStagesFile()
	{
		string filePath = "res://JSON/PokemonStage.json";

		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		Json json = new Json();

		if (json.Parse(jsonString) != Error.Ok) return;

		_stagesDictionary = new GC.Dictionary<string, Variant>((GC.Dictionary)json.Data);

		// Print message to console
		string loadSuccessMessage = "Pokemon Stages File Successfully Loaded";
		if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
	}

	public PokemonStage FindPokemonStage(int stageID)
	{
		return _pokemonStages.Find(pokemonStage => pokemonStage.ID == stageID);
	}

	public List<PokemonStage> GetPokemonStages()
	{
		return _pokemonStages;
	}

	private void SetPokemonStages()
	{
		List<string> stageIDs = _stagesDictionary.Keys.ToList();
		foreach (string stageID in stageIDs)
		{
			GC.Dictionary<string, Variant> stageDictionary = _stagesDictionary[stageID].As<GC.Dictionary<string, Variant>>();
			PokemonStage pokemonStage = new PokemonStage();
			pokemonStage.ID = int.Parse(stageID);
			pokemonStage.WaveCount = stageDictionary["Wave Count"].As<int>();
			pokemonStage.PokemonPerWave = stageDictionary["Pokemon Per Wave"].As<int>();
			pokemonStage.PokemonLevels = stageDictionary["Pokemon Levels"].As<GC.Array<int>>().ToList();

			GC.Dictionary<string, int> pokemonDictionary = stageDictionary["Pokemon"].As<GC.Dictionary<string, int>>();
			pokemonStage.PokemonNames = pokemonDictionary.Keys.ToList();

			string pokemonCountMessage = $"Stage {stageID} Pokemon Count: {pokemonStage.PokemonNames.Count}";
			PrintRich.Print(TextColor.Blue, pokemonCountMessage);

			_pokemonStages.Add(pokemonStage);
		}
		GD.Print(); // Print Spacing
	}

	public int GetRandomEnemyCount()
	{
		int minCount = 5;
		int maxCount = 10;

		RandomNumberGenerator RNG = new RandomNumberGenerator();
		int enemyCount = RNG.RandiRange(minCount, maxCount);

		return enemyCount;
	}

	public PokemonStage GetPokemonStage(int pokemonStageID)
	{
		return _pokemonStages.Find(pokemonStage => pokemonStage.ID == pokemonStageID);
	}

	public void CompletedStage(int stageID)
	{
		bool hasCompleted = _stageCompletionList[stageID];
		if (hasCompleted) return;

		_stageCompletionList[stageID] = true;

		// Print message to console
		string completedStageMessage = $"You've Completed Stage {stageID}!";
		if (PrintRich.AreStageMessagesEnabled) PrintRich.PrintLine(TextColor.Yellow, completedStageMessage);
	}

	public GC.Dictionary<string, Variant> GetData()
	{
		GC.Dictionary<string, Variant> stageData = new GC.Dictionary<string, Variant>();
		List<int> keys = _stageCompletionList.Keys.ToList();
		for (int i = 0; i < keys.Count; i++)
		{
			int key = keys[i];
			stageData.Add(key.ToString(), _stageCompletionList[key]);
		}
		return stageData;
	}

	public void SetData(GC.Dictionary<string, Variant> stageData)
	{
		foreach (string keyString in stageData.Keys)
		{
			int key = int.Parse(keyString);
			_stageCompletionList[key] = stageData[keyString].As<bool>();
		}
	}
}
