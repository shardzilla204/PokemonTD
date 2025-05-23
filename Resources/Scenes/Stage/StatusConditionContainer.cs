using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class StatusConditionContainer : Container
{
    private List<StatusConditionIcon> _statusConditionIcons = new List<StatusConditionIcon>();
    
    public void AddStatusCondition(StatusCondition statusCondition)
    {
        StatusConditionIcon statusConditionIcon = PokemonTD.PackedScenes.GetStatusConditionIcon(statusCondition);
        AddChild(statusConditionIcon);
        _statusConditionIcons.Add(statusConditionIcon);
    }

    public void RemoveStatusCondition(StatusCondition statusCondition)
    {
        StatusConditionIcon statusConditionIcon = _statusConditionIcons.Find(statusConditionIcon => statusConditionIcon.StatusCondition == statusCondition);
        _statusConditionIcons.Remove(statusConditionIcon);

        if (statusConditionIcon != null) statusConditionIcon.QueueFree();
    }

    public void RemoveAllStatusConditions()
    {
        foreach (StatusConditionIcon statusConditionIcon in _statusConditionIcons)
        {
            statusConditionIcon.QueueFree();
        }
        _statusConditionIcons.Clear();   
    }
}
