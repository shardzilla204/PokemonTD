using Godot;

namespace PokemonTD;

public partial class StatusConditionIcon : TextureRect
{
    public StatusCondition StatusCondition;

    public void SetIcon(StatusCondition statusCondition)
    {
        StatusCondition = statusCondition;
        Texture = PokemonTD.GetStatusIcon(statusCondition);
    }
}
