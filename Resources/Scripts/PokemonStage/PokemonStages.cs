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

	public override void _EnterTree()
	{
		Instance = this;
	}

	public override void _Ready()
	{
		LoadStagesFile();
		SetPokemonStages();

		PokemonTD.Signals.EvolutionQueueCleared += () => 
		{
			if (!PokemonMoves.Instance.IsQueueEmpty()) return;
			
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
		};
		PokemonTD.Signals.ForgetMoveQueueCleared += () => 
		{
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PressedPlay);
		};
	}

	private void LoadStagesFile()
	{
		string filePath = "res://JSON/PokemonStage.json";

		using FileAccess file = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
		string jsonString = file.GetAsText();

		Json json = new Json();
		
		if (json.Parse(jsonString) != Error.Ok) return;
		
		string loadSuccessMessage = "Pokemon Stages File Successfully Loaded";
		PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
		_stagesDictionary = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
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

	public PokemonStage GetPokemonStage(int pokemonStageID)
	{
		return _pokemonStages.Find(pokemonStage => pokemonStage.ID == pokemonStageID);
	}
}
