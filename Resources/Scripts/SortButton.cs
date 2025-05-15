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

	public bool IsDescending = false;

    public override void _ExitTree()
    {
        PokemonTD.Signals.SortButtonPressed -= OnSortButtonPressed;
    }

    public override void _Ready()
    {
        base._Ready();
        PokemonTD.Signals.SortButtonPressed += OnSortButtonPressed;
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
            PokemonTD.Signals.EmitSignal(Signals.SignalName.SortButtonPressed, (int) _sortCategory);

            IsDescending = !IsDescending;
            UpdateArrows(IsDescending);
        };
    }

    private void OnSortButtonPressed(int sortCategoryID)
    {
        if (_sortCategory == (SortCategory) sortCategoryID) return;
        
        IsDescending = false;

        _upArrow.Visible = true;
        _downArrow.Visible = true;
    }

    public void UpdateArrows(bool isDescending)
    {
        _upArrow.Visible = !isDescending;
        _downArrow.Visible = isDescending;
    }
}
