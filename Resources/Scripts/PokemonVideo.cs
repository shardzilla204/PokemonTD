using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokemonVideo : Node
{
    public PokemonVideo(string title, Dictionary<string, Variant> dataDictionary)
    {
        Title = title;
        Caption = dataDictionary["Caption"].As<string>();
        FilePath = dataDictionary["File Path"].As<string>();
    }
    
    public string Title;
    public string Caption;
    public string FilePath;
}