using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokemonMasterMode : Node
{
    public bool IsEnabled = false;
    public int MinPokemonLevel = 1;
    public int MaxPokemonLevel = 100;

    public Dictionary<string, Variant> GetData()
    {
        Dictionary<string, Variant> masterModeData = new Dictionary<string, Variant>()
        {
            { "Is Enabled", IsEnabled },
            { "Min Pokemon Level", MinPokemonLevel },
            { "Max Pokemon Level", MaxPokemonLevel }
        };
        return masterModeData;
    }

    public void SetData(Dictionary<string, Variant> masterModeData)
    {
        IsEnabled = masterModeData["Is Enabled"].As<bool>();
        MinPokemonLevel = masterModeData["Min Pokemon Level"].As<int>();
        MaxPokemonLevel = masterModeData["Max Pokemon Level"].As<int>();
    }
}
