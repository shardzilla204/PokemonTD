using Godot;

namespace PokemonTD;

public partial class StageConsole : NinePatchRect
{
    [Export]
    private Container _container;

    [Export]
    private ScrollContainer _scrollContainer;

    public override void _EnterTree()
    {
        PokemonTD.StageConsole = this;

        foreach (Node child in _container.GetChildren())
        {
            child.QueueFree();
        }
    }

    public override void _ExitTree()
    {
        PokemonTD.StageConsole = null;
    }

    public void AddMessage(StageConsoleLabel stageConsoleLabel)
    {
        _container.AddChild(stageConsoleLabel);
        _scrollContainer.ScrollVertical = (int) _container.Size.Y;
    }
}
