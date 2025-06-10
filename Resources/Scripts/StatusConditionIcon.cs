using Godot;

namespace PokemonTD;

public partial class StatusConditionIcon : TextureRect
{
    public StatusConditionIcon(StatusCondition statusCondition)
    {
        StatusCondition = statusCondition;

        ExpandMode = ExpandModeEnum.IgnoreSize;
        StretchMode = StretchModeEnum.KeepAspectCentered;
        CustomMinimumSize = new Vector2(20, 20);
        Texture = PokemonTD.GetStatusIcon(statusCondition);
    }

    public StatusCondition StatusCondition;
}
