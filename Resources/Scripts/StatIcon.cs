using Godot;

namespace PokemonTD;

public partial class StatIcon : TextureRect
{
    public StatIcon(PokemonStat stat, bool isIncreasing)
    {
        Stat = stat;
        IsIncreasing = isIncreasing;

        ExpandMode = ExpandModeEnum.IgnoreSize;
        StretchMode = StretchModeEnum.KeepAspectCentered;
        CustomMinimumSize = new Vector2(30, 30);

        Texture = PokemonTD.GetStatIcon(stat);
        SelfModulate = isIncreasing ? _increaseColor : _decreaseColor;
    }

    public PokemonStat Stat;
    public bool IsIncreasing = false;

    private Color _increaseColor = Color.FromHtml("#008c0c");
    private Color _decreaseColor = Color.FromHtml("#962121");
}
