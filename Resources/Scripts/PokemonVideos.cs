using Godot;
using GC = Godot.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PokemonTD;

public partial class PokemonVideos : Node
{
    private static PokemonVideos _instance;
    public static PokemonVideos Instance
    {
        get => _instance;
        private set
        {
            if (_instance == null) _instance = value;
        }
    }

    public List<PokemonVideo> Videos = new List<PokemonVideo>();
    
    public override void _EnterTree()
	{
		Instance = this;

        LoadPokemonVideoFile();
	}

    private void LoadPokemonVideoFile()
    {
        string filePath = "res://JSON/PokemonVideo.json";

        using FileAccess pokemonVideoFile = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
        string jsonString = pokemonVideoFile.GetAsText();

        Json json = new Json();

        if (json.Parse(jsonString) != Error.Ok) return;

        GC.Dictionary<string, Variant> pokemonVideoDictionaries = new GC.Dictionary<string, Variant>((GC.Dictionary) json.Data);
        SetVideos(pokemonVideoDictionaries);

        // Print message to console
        string loadSuccessMessage = "Pokemon Video File Successfully Loaded";
        if (PrintRich.AreFileMessagesEnabled) PrintRich.PrintLine(TextColor.Green, loadSuccessMessage);
    }

    private void SetVideos(GC.Dictionary<string, Variant> pokemonVideoDictionaries)
    {
        List<string> titles = pokemonVideoDictionaries.Keys.ToList();
        foreach (string title in titles)
        {
            GC.Dictionary<string, Variant> videoDictionary = (GC.Dictionary<string, Variant>) pokemonVideoDictionaries[title];
            AddPokemonVideo(title, videoDictionary);
        }
    }

    private void AddPokemonVideo(string title, GC.Dictionary<string, Variant> videoDictionary)
    {
        PokemonVideo pokemonVideo = new PokemonVideo(title, videoDictionary);
        Videos.Add(pokemonVideo);
    }

    public VideoStreamTheora GetVideoStream(int index)
    {
        string filePath = Videos[index].FilePath;
        return ResourceLoader.Load<VideoStreamTheora>(filePath);
    }
}