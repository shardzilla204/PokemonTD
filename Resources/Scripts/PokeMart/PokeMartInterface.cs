using Godot;

namespace PokemonTD;

public partial class PokeMartInterface : CanvasLayer
{
    [Export]
    private CustomButton _exitButton;

    [Export]
    private Label _pokeDollarsLabel;

    [Export]
    private Container _pokeMartItems;

    [Export]
    private Container _pokeMartTeamSlots;

    [Export]
    private PokeMartInventory _pokeMartInventory;

    public override void _ExitTree()
    {
        PokemonTD.Signals.PokeDollarsUpdated -= PokeDollarsUpdated;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.PokeDollarsUpdated += PokeDollarsUpdated;

        _exitButton.Pressed += () =>
        {
            StageSelectInterface stageSelectInterface = PokemonTD.PackedScenes.GetStageSelectInterface();
            AddSibling(stageSelectInterface);
            QueueFree();
        };
        _pokeDollarsLabel.Text = $"₽ {PokemonTD.PokeDollars}";

        ClearPokeMartItems();
        AddPokeMartItems();
    }

      public override void _Input(InputEvent @event)
    {
        if (@event is not InputEventKey eventKey) return;

        if (eventKey.Keycode == Key.Ctrl)
        {
            Control parent = _pokeMartItems.GetParentOrNull<Control>();
            parent.MouseFilter = eventKey.Pressed ? Control.MouseFilterEnum.Ignore : Control.MouseFilterEnum.Stop;
        }
    }

    private void ClearPokeMartItems()
    {
        foreach (Node child in _pokeMartItems.GetChildren())
        {
            child.QueueFree();
        }
    }

    private void AddPokeMartItems()
    {
        foreach (PokeMartItem pokeMartItemData in PokeMart.Instance.Items)
        {
            PokeMartItem pokeMartItem = PokemonTD.PackedScenes.GetPokeMartItem(pokeMartItemData);
            _pokeMartItems.AddChild(pokeMartItem);
        }
    }

    private void PokeDollarsUpdated()
    {
        _pokeDollarsLabel.Text = $"₽ {PokemonTD.PokeDollars}";
    }
}
