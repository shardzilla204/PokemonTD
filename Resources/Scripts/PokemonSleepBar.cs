using Godot;
using System;

namespace PokemonTD;

public partial class PokemonSleepBar : TextureProgressBar
{
    [Signal]
    public delegate void FinishedEventHandler();

    [Export]
    private Timer _sleepTimer;
    
    private double _waitTime = 1;

    public override void _ExitTree()
    {
        PokemonTD.Signals.PressedPlay -= ContinueTimer;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.PressedPlay += ContinueTimer;
        _sleepTimer.Timeout += () => EmitSignal(SignalName.Finished);
        Value = 0;
    }

    public override async void _Process(double delta)
    {
        if (_sleepTimer.IsStopped()) return;

        if (PokemonTD.IsGamePaused) 
        {
            _waitTime = _sleepTimer.TimeLeft;
            _sleepTimer.Stop();
            await ToSignal(PokemonTD.Signals, Signals.SignalName.PressedPlay);
        }

        Value = _sleepTimer.TimeLeft;
    }

    private void ContinueTimer()
    { 
        if (PokemonTD.IsGamePaused) return;
        
        _sleepTimer.WaitTime = _waitTime;
        _sleepTimer.Start();
    }

    public void Start(Pokemon pokemon, float additionalTime)
    {
        float waitTime = GetWaitTime(pokemon) + additionalTime;
        Value = waitTime;
        MaxValue = waitTime;
        _sleepTimer.WaitTime = waitTime;
        _sleepTimer.Start();
    }

    private float GetWaitTime(Pokemon pokemon)
    {
        int pokemonLevel = pokemon.Level;
        int levelThreshold = 5;
        
        float maxWaitTime = 5;
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
