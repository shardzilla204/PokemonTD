using Godot;
using GC = Godot.Collections;

namespace PokemonTD;

public partial class PokemonSettings : Node
{
    private static PokemonSettings _instance;

    public static PokemonSettings Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    private string _settingsFilePath = "user://settings.cfg";

    public override void _EnterTree()
    {
        Instance = this;

        PokemonTD.Signals.GameSaved += SaveSettings;
		PokemonTD.Signals.GameLoaded += LoadSettings;

        if (FileAccess.FileExists(_settingsFilePath))
        {
            LoadSettings();
        }
        else
        {
            SaveSettings();
        }
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            SaveSettings();
        }
    }

    public void SaveSettings()
		{
			using FileAccess settingsFile = FileAccess.Open(_settingsFilePath, FileAccess.ModeFlags.Write);
			string jsonString = Json.Stringify(GetData(), "\t");

			if (jsonString == "") return;

			settingsFile.StoreLine(jsonString);

			string saveSuccessMessage = "Settings File Successfully Saved";
			if (PokemonTD.AreFilePathsVisible)
			{
				saveSuccessMessage += $" At {settingsFile.GetPathAbsolute()}";
			}
			PrintRich.PrintLine(TextColor.Green, saveSuccessMessage);
		}

    public void LoadSettings()
    {
        using FileAccess settingsFile = FileAccess.Open(_settingsFilePath, FileAccess.ModeFlags.Read);
        string jsonString = settingsFile.GetAsText();

        if (settingsFile.GetLength() == 0)
        {
            GD.PrintErr("Settings File Is Empty");
            return;
        }

        Json json = new Json();

        Error result = json.Parse(jsonString);

        if (result != Error.Ok) return;

        GC.Dictionary<string, Variant> settingsData = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
        SetData(settingsData);

        string loadSuccessMessage = "Settings File Successfully Loaded";

        if (PokemonTD.AreFilePathsVisible)
        {
            loadSuccessMessage += $" At {settingsFile.GetPathAbsolute()}";
        }
        PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
    }

    private GC.Dictionary<string, Variant> GetData()
    {
        GC.Dictionary<string, Variant> audioData = new GC.Dictionary<string, Variant>();
        foreach(AudioBus audioBus in PokemonTD.AudioManager.AudioBuses)
        {
            audioData.Add(audioBus.BusType.ToString(), GetBusData(audioBus));
        }
        return audioData;
    }

    private GC.Dictionary<string, Variant> GetBusData(AudioBus audioBus)
    {
        return new GC.Dictionary<string, Variant>()
        {
            { "Volume", audioBus.Volume },
            { "Is Muted", audioBus.IsMuted },
        };
    }

    private void SetData(GC.Dictionary<string, Variant> settingsData)
    {
        foreach(AudioBus audioBus in PokemonTD.AudioManager.AudioBuses)
        {
            GC.Dictionary<string, Variant> busData = (GC.Dictionary<string, Variant>) settingsData[audioBus.BusType.ToString()];
            SetBusData(audioBus, busData);
            AudioServer.SetBusVolumeDb((int) audioBus.BusType, audioBus.Volume);
        }
    }

    private void SetBusData(AudioBus audioBus, GC.Dictionary<string, Variant> busData)
    {
        audioBus.Volume = busData["Volume"].As<int>();
        audioBus.IsMuted = busData["Is Muted"].As<bool>();
    }
}