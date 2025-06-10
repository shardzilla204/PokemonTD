using System;
using Godot;

namespace PokemonTD;

public partial class CustomTimer : Timer
{
    public CustomTimer(float waitTime)
    {
        Autostart = true;
        OneShot = true;
        WaitTime = waitTime / PokemonTD.GameSpeed;
    }

    public double WaitTimeLeft;
    private GodotObject _defending;

    public override void _Ready()
    {
        Timeout += async () => 
        {
            if (PokemonTD.IsGamePaused) await ToSignal(PokemonTD.Signals, PokemonSignals.SignalName.PressedPlay);
            QueueFree();
        };
    }

    public void Start()
    {
        try
        {
            Start(WaitTimeLeft);
        }
        catch (ObjectDisposedException) { }
    }

    new public void Stop()
    {
        try
        {
            WaitTimeLeft = TimeLeft;
            base.Stop();
        }
        catch (ObjectDisposedException) { }
    }
}