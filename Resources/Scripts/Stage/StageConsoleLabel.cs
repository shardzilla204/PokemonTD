using Godot;

namespace PokemonTD;

public partial class StageConsoleLabel : Label
{
    [Export]
    private Timer _timer;

    public override void _Ready()
    {
        _timer.Timeout += TweenOpacity;
    }

    private void TweenOpacity()
    {
        Color transparent = Colors.White;
        transparent.A = 0;

        float duration = 1f;
        Tween tween = GetTree().CreateTween();
        tween.TweenProperty(this, "modulate", transparent, duration);
        tween.Finished += QueueFree;
    }
}
