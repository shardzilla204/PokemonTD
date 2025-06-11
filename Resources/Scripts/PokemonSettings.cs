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
    public int WindowModeIndex = 3;
    public Vector2I WindowSize = DisplayServer.WindowGetSize();
    public bool IsWindowMaximized = false;
    public bool ButtonSFXEnabled = true;
    public bool PokemonSFXEnabled = true;
    public bool PokemonMoveSFXEnabled = true;
    public bool AutoHealEnabled = false;
    
    public override void _ExitTree()
    {
        SaveSettings();
    }

    public override void _EnterTree()
    {
        Instance = this;

        PokemonTD.Signals.GameSaved += SaveSettings;
        PokemonTD.Signals.GameLoaded += LoadSettings;
        GetWindow().SizeChanged += () =>
        {
            WindowSize = GetWindow().Size;
        };

        GetWindow().WindowInput += (inputEvent) =>
        {
            IsWindowMaximized = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Maximized;
        };

        if (FileAccess.FileExists(_settingsFilePath))
        {
            LoadSettings();
        }
        else
        {
            SaveSettings();
            LoadSettings(); // Apply the settings
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
            { "Window Size", GetWindowSizeData() },
            { "Is Window Maximized", IsWindowMaximized },
            { "Button SFX Enabled", ButtonSFXEnabled },
            { "Pokemon SFX Enabled", PokemonSFXEnabled },
            { "Pokemon Move SFX Enabled", PokemonMoveSFXEnabled },
            { "Auto Heal Enabled", AutoHealEnabled }
        };
        return settingsData;
    }

    private GC.Dictionary<string, Variant> GetWindowSizeData()
    {
        GC.Dictionary<string, Variant> windowSizeData = new GC.Dictionary<string, Variant>()
        {
            { "X", WindowSize.X },
            { "Y", WindowSize.Y }
        };
        return windowSizeData;
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
        GC.Dictionary<string, Variant> windowSizeData = settingsData["Window Size"].As<GC.Dictionary<string, Variant>>();
        SetWindowSizeData(windowSizeData);
        IsWindowMaximized = settingsData["Is Window Maximized"].As<bool>();
        ButtonSFXEnabled = settingsData["Button SFX Enabled"].As<bool>();
        PokemonSFXEnabled = settingsData["Pokemon SFX Enabled"].As<bool>();
        PokemonMoveSFXEnabled = settingsData["Pokemon Move SFX Enabled"].As<bool>();
        AutoHealEnabled = settingsData["Auto Heal Enabled"].As<bool>();

        ApplySettings();
    }

    private void SetWindowSizeData(GC.Dictionary<string, Variant> windowSizeData)
    {
        int x = windowSizeData["X"].As<int>();
        int y = windowSizeData["Y"].As<int>();
        WindowSize = new Vector2I(x, y);
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
        Vector2I windowSize = WindowSize;

        DisplayServer.MouseMode mouseMode = WindowModeIndex == 0 ? DisplayServer.MouseMode.Confined : DisplayServer.MouseMode.Visible;
        DisplayServer.MouseSetMode(mouseMode);

        DisplayServer.WindowMode windowMode = WindowModeIndex switch
        {
            2 => DisplayServer.WindowMode.Windowed,
            _ => DisplayServer.WindowMode.Fullscreen
        };

        if (IsWindowMaximized && windowMode == DisplayServer.WindowMode.Windowed)
        {
            windowMode = DisplayServer.WindowMode.Maximized;
        }

        DisplayServer.WindowSetMode(windowMode);
        if (windowMode != DisplayServer.WindowMode.Fullscreen) GetWindow().Size = windowSize;
    }
}