using Godot;

namespace PokemonTD;

public enum SortCategory
{
    Level,
    NationalNumber,
    Name,
    Type
}

public partial class SortButton : CustomButton
{
    [Export]
    private SortCategory _sortCategory;

    [Export]
    private Label _sortCategoryLabel;

    [Export]
    private TextureRect _upArrow;

    [Export]
    private TextureRect _downArrow;

	public bool IsDescending;

    public override void _ExitTree()
    {
        PokemonTD.Signals.SortButtonPressed -= OnSortButtonPressed;
    }

    public override void _Ready()
    {
        base._Ready();
        _sortCategoryLabel.Text = _sortCategory switch 
        {
            SortCategory.Level => "Level",
            SortCategory.NationalNumber => "Number",
            SortCategory.Name => "Name",
            SortCategory.Type => "Type",
            _ => "",
        };
		Pressed += () => 
        {
            PokemonTD.Signals.EmitSignal(Signals.SignalName.SortButtonPressed);

            IsDescending = !IsDescending;
            _upArrow.Visible = IsDescending ? false : true;
            _downArrow.Visible = IsDescending ? true : false;
        };

        PokemonTD.Signals.SortButtonPressed += OnSortButtonPressed;
    }

    private void OnSortButtonPressed()
    {
        _upArrow.Visible = true;
        _downArrow.Visible = true;
    }
}
