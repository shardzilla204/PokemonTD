using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class StatContainer : Node
{
    private List<StatIcon> _statIcons = new List<StatIcon>();

    public void AddStat(PokemonStat stat, bool isIncreasing)
    {
        bool hasStatIcon = HasStatIcon(stat, isIncreasing);
        if (hasStatIcon) return;

        if (stat == PokemonStat.Accuracy || stat == PokemonStat.Evasion) return;

        StatIcon statIcon = new StatIcon(stat, isIncreasing);
        AddChild(statIcon);
        _statIcons.Add(statIcon);
    }

    public void RemoveStat(PokemonStat stat)
    {
        StatIcon statIcon = _statIcons.Find(statusConditionIcon => statusConditionIcon.Stat == stat && !statusConditionIcon.IsIncreasing);
        _statIcons.Remove(statIcon);

        if (statIcon != null && IsInstanceValid(statIcon)) statIcon.QueueFree();
    }

    public void ClearStats()
    {
        foreach (StatIcon statIcon in _statIcons)
        {
            statIcon.QueueFree();
        }
        _statIcons.Clear();
    }

    private bool HasStatIcon(PokemonStat stat, bool isIncreasing)
    { 
        StatIcon desiredIcon = _statIcons.Find(statIcon => statIcon.Stat == stat && statIcon.IsIncreasing == isIncreasing);
        return desiredIcon != null;
    }
}
