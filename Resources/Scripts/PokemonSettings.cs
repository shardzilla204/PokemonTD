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

    public bool HasShowedTutorial = false;
    public int WindowModeIndex = 0;
    public bool ButtonSFXEnabled = true;
    public bool PokemonSFXEnabled = true;
    public bool PokemonMoveSFXEnabled = true;

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
        if (what == NotificationWMCloseRequest) SaveSettings();
    }

    public void SaveSettings()
    {
        using FileAccess settingsFile = FileAccess.Open(_settingsFilePath, FileAccess.ModeFlags.Write);
        string jsonString = Json.Stringify(GetData(), "\t");

        if (jsonString == "") return;

        settingsFile.StoreLine(jsonString);

        // Print message to console
        string saveSuccessMessage = "Settings File Successfully Saved";
        if (PrintRich.AreFilePathsVisible)
        {
            saveSuccessMessage += $" At {settingsFile.GetPathAbsolute()}";
        }
        if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, saveSuccessMessage);
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

        // Print message to console
        string loadSuccessMessage = "Settings File Successfully Loaded";
        if (PrintRich.AreFilePathsVisible)
        {
            loadSuccessMessage += $" At {settingsFile.GetPathAbsolute()}";
        }
        if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
    }

    private GC.Dictionary<string, Variant> GetData()
    {
        GC.Dictionary<string, Variant> settingsData = new GC.Dictionary<string, Variant>()
        {
            { "Audio", GetAudioData() },
            { "Has Showed Tutorial", HasShowedTutorial },
            { "Window Mode Index", WindowModeIndex },
            { "Button SFX Enabled", ButtonSFXEnabled },
            { "Pokemon SFX Enabled", PokemonSFXEnabled },
            { "Pokemon Move SFX Enabled", PokemonMoveSFXEnabled }
        };
        return settingsData;
    }

    private GC.Dictionary<string, Variant> GetAudioData()
    {
        GC.Dictionary<string, Variant> audioData = new GC.Dictionary<string, Variant>();
        foreach (AudioBus audioBus in PokemonTD.AudioManager.AudioBuses)
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
        GC.Dictionary<string, Variant> audioData = settingsData["Audio"].As<GC.Dictionary<string, Variant>>();
        SetAudioData(audioData);

        HasShowedTutorial = settingsData["Has Showed Tutorial"].As<bool>();
        WindowModeIndex = settingsData["Window Mode Index"].As<int>();
        ButtonSFXEnabled = settingsData["Button SFX Enabled"].As<bool>();
        PokemonSFXEnabled = settingsData["Pokemon SFX Enabled"].As<bool>();
        PokemonMoveSFXEnabled = settingsData["Pokemon Move SFX Enabled"].As<bool>();

        ApplySettings();
    }

    private void SetAudioData(GC.Dictionary<string, Variant> audioData)
    {
        foreach (AudioBus audioBus in PokemonTD.AudioManager.AudioBuses)
        {
            GC.Dictionary<string, Variant> busData = audioData[audioBus.BusType.ToString()].As<GC.Dictionary<string, Variant>>();
            SetBusData(audioBus, busData);
            AudioServer.SetBusVolumeDb((int)audioBus.BusType, audioBus.Volume);
        }
    }

    private void SetBusData(AudioBus audioBus, GC.Dictionary<string, Variant> busData)
    {
        audioBus.Volume = busData["Volume"].As<int>();
        audioBus.IsMuted = busData["Is Muted"].As<bool>();
    }

    public void ApplySettings()
    {
        DisplayServer.WindowMode windowMode = WindowModeIndex switch
        {
            2 => DisplayServer.WindowMode.Windowed,
            _ => DisplayServer.WindowMode.Fullscreen
        };
        DisplayServer.WindowSetMode(windowMode);

        DisplayServer.MouseMode mouseMode = WindowModeIndex == 0 ? DisplayServer.MouseMode.Confined : DisplayServer.MouseMode.Visible;
        DisplayServer.MouseSetMode(mouseMode);
    }
}