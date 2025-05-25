using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonGame : Node
{
	private static PokemonGame _instance;

    public static PokemonGame Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    private string _gameFilePath = "user://savegame.sav";

    public override void _EnterTree()
    {
		Instance = this;

		PokemonTD.Signals.GameSaved += SaveGame;
		PokemonTD.Signals.GameLoaded += LoadGame;

        if (FileAccess.FileExists(_gameFilePath))
		{
			LoadGame();
		}
		else
		{
			SaveGame();
		}
    }

    public async override void _Notification(int what)
    {
		if (what != NotificationWMCloseRequest) return;

		await ToSignal(GetTree().CreateTimer(0.25f), SceneTreeTimer.SignalName.Timeout);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.GameSaved);
		GetTree().Quit();
    }

    public void SaveGame()
    {
        using FileAccess gameFile = FileAccess.Open(_gameFilePath, FileAccess.ModeFlags.Write);
		string jsonString = Json.Stringify(GetData(), "\t");

		if (jsonString == "") return;

		gameFile.StoreLine(jsonString);

		// Print message to console
		string saveSuccessMessage = "Game File Successfully Saved";
		if (PrintRich.AreFilePathsVisible)
		{
			saveSuccessMessage += $" At {gameFile.GetPathAbsolute()}";
		}
		if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, saveSuccessMessage);
    }

    public void LoadGame()
    {
        using FileAccess gameFile = FileAccess.Open(_gameFilePath, FileAccess.ModeFlags.Read);
		string jsonString = gameFile.GetAsText();

		if (gameFile.GetLength() == 0)
		{
			GD.PrintErr("Game File Is Empty");
			return;
		}

		Json json = new Json();
		Error result = json.Parse(jsonString);

		if (result != Error.Ok) return;

		GC.Dictionary<string, Variant> gameData = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
		SetData(gameData);

		// Print message to console
		string loadSuccessMessage = "Game File Successfully Loaded";
		if (PrintRich.AreFilePathsVisible)
		{
			loadSuccessMessage += $" At {gameFile.GetPathAbsolute()}";
		}
		if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
    }

    public GC.Dictionary<string, Variant> GetData()
    {
		GC.Dictionary<string, Variant> gameData = new GC.Dictionary<string, Variant>()
		{
			{ "Has Selected Starter", PokemonTD.HasSelectedStarter },
			{ "Poke Dollars", PokemonTD.PokeDollars },
			{ "Pokemon Team", PokemonTeam.Instance.GetData() },
			{ "Pokemon Inventory", PokeMart.Instance.GetData() }
		};

        return gameData;
    }

    public void SetData(GC.Dictionary<string, Variant> gameData)
    {
		try
		{
			PokemonTD.HasSelectedStarter = gameData["Has Selected Starter"].As<bool>();
			PokemonTD.PokeDollars = gameData["Poke Dollars"].As<int>();

			GC.Dictionary<string, Variant> pokemonTeamData = gameData["Pokemon Team"].As<GC.Dictionary<string, Variant>>();
			PokemonTeam.Instance.SetData(pokemonTeamData);
			
			GC.Dictionary<string, Variant> pokemonInventoryData = gameData["Pokemon Inventory"].As<GC.Dictionary<string, Variant>>();
			PokeMart.Instance.SetData(pokemonInventoryData);
		}
		catch (KeyNotFoundException e)
		{
			GD.PrintErr(e);
			SaveGame();
		}
    }
}