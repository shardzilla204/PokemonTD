using System.Collections.Generic;
using Godot;

namespace PokemonTD;

public partial class PokemonTimer : Node
{
    private List<CustomTimer> _timers = new List<CustomTimer>();

    public PokemonTimer()
    {
        PokemonTD.Signals.PressedPlay += StartTimers;
        PokemonTD.Signals.PressedPause += StopTimers;
        PokemonTD.Signals.Dragging += Dragging;
        PokemonTD.Signals.HasLeftStage += ClearTimers;
    }

    public void AddTimer(CustomTimer timer)
    {
        _timers.Add(timer);
    }

    public void RemoveTimer(CustomTimer timer)
    {
        _timers.Remove(timer);
    }

    private void StartTimers()
    {
        foreach (CustomTimer timer in _timers)
        {
            timer.Start();
        }
    }

    private void StopTimers()
    {
        foreach (CustomTimer timer in _timers)
        {
            timer.Stop();
        }
    }

    private void ClearTimers()
    {
        foreach (CustomTimer timer in _timers)
        {
            timer.QueueFree();
        }
    }

    private async void Dragging(bool isDragging)
    {
        if (isDragging)
        {
            StopTimers();
        }
        else
        {
            if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, PokemonSignals.SignalName.PressedPlay);
            StartTimers();
        }
    }
}