using Godot;
using System;

namespace PokemonTD;

public partial class PokemonSleepBar : TextureProgressBar
{
    [Signal]
    public delegate void FinishedEventHandler();

    [Export]
    private Timer _sleepTimer;

    public override void _Ready()
    {
        _sleepTimer.Timeout += () => EmitSignal(SignalName.Finished);
        Value = 0;
    }

    public override void _Process(double delta)
    {
        if (_sleepTimer.IsStopped()) return;

        Value = _sleepTimer.TimeLeft;
    }

    public void Start(Pokemon pokemon)
    {
        float waitTime = GetWaitTime(pokemon);
        Value = waitTime;
        MaxValue = waitTime;
        _sleepTimer.WaitTime = waitTime;
        _sleepTimer.Start();
    }

    private float GetWaitTime(Pokemon pokemon)
    {
        float maxWaitTime = 5;
        int pokemonLevel = pokemon.Level;
        int levelThreshold = 5;
        float waitTime = 0;
        float time = 1.1f;

        while (pokemonLevel > 0)
        {
            waitTime += time;
            pokemonLevel -= levelThreshold;
        }
        return Math.Clamp(waitTime, 0, maxWaitTime);
    }
}
