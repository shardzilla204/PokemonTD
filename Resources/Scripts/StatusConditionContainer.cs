using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class StatusConditionContainer : Container
{
    private List<StatusConditionIcon> _statusConditionIcons = new List<StatusConditionIcon>();
    
    public void AddStatusCondition(StatusCondition statusCondition)
    {
        StatusConditionIcon statusConditionIcon = new StatusConditionIcon(statusCondition);
        AddChild(statusConditionIcon);
        _statusConditionIcons.Add(statusConditionIcon);
    }

    public void RemoveStatusCondition(StatusCondition statusCondition)
    {
        StatusConditionIcon statusConditionIcon = _statusConditionIcons.Find(statusConditionIcon => statusConditionIcon.StatusCondition == statusCondition);
        _statusConditionIcons.Remove(statusConditionIcon);

        if (statusConditionIcon != null && IsInstanceValid(statusConditionIcon)) statusConditionIcon.QueueFree();
    }

    public void ClearStatusConditions()
    {
        foreach (StatusConditionIcon statusConditionIcon in _statusConditionIcons)
        {
            statusConditionIcon.QueueFree();
        }
        _statusConditionIcons.Clear();   
    }
}
