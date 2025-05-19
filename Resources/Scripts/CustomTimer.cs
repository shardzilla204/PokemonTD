using Godot;

namespace PokemonTD;

public partial class CustomTimer : Timer
{
    public double WaitTimeLeft;

    public override void _Ready()
    {
        Timeout += QueueFree;
    }
}