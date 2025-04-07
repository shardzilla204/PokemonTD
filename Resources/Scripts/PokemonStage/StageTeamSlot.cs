using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class StageTeamSlot : NinePatchRect
{
	[Signal]
	public delegate void DraggingEventHandler(bool isDragging);

	[Export]
	private TextureRect _genderSprite;

	[Export]
	private Label _pokemonName;

	[Export]
	private TextureRect _pokemonSprite;

	[Export]
	private Label _pokemonLevel;

	[Export]
	private TextureProgressBar _experienceBar;

	[Export]
	private PokemonMoveButton _pokemonMoveButton;

	private bool _isFilled;
	private bool _isDragging;
	private bool _isPokemonOut;

	public int ID = 0;

	public Pokemon Pokemon;

	public override void _Ready()
	{
		_pokemonMoveButton.Pressed += OnMoveButtonPressed;

		if (Pokemon is not null) UpdateControls();;
	}

	public override void _Notification(int what)
	{
		if (what != NotificationDragEnd || !_isDragging) return;

		_isDragging = false;
		EmitSignal(SignalName.Dragging, false);

		if (IsDragSuccessful()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonOnStage, ID);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		_isDragging = true; 
		EmitSignal(SignalName.Dragging, true);
		
		SetDragPreview(GetDragPreview());
		return GetDragData();
	}

	private Dictionary<string, Variant> GetDragData()
	{
		return new Dictionary<string, Variant>()
		{
			{ "TeamSlotID", ID },
			{ "FromTeamSlot", true },
			{ "Pokemon", Pokemon }
		};
	}

	// Update text, textures and progress bars
	private void UpdateControls()
	{
		_pokemonName.Text = Pokemon != null ? $"{Pokemon.Name}" : "";
		_pokemonSprite.Texture = Pokemon != null ? Pokemon.Sprite : null;
		_pokemonLevel.Text = Pokemon != null ? $"Lvl. {Pokemon.Level}": null;

		_experienceBar.Value = Pokemon.MinExperience;
		_experienceBar.MaxValue = Pokemon.MaxExperience;

		_genderSprite.Texture = GetGenderSprite();

		_pokemonMoveButton.Update(Pokemon.Move);
	}

	private Texture2D GetGenderSprite()
	{
		string filePath = $"res://Assets/Images/Gender/{Pokemon.Gender}Icon.png";
		return ResourceLoader.Load<Texture2D>(filePath);
	}

	private Control GetDragPreview()
	{
		int minValue = 85;
		Vector2 minSize = new Vector2(minValue, minValue);
		TextureRect textureRect = new TextureRect()
		{
			CustomMinimumSize = minSize,
			Texture = Pokemon.Sprite,
			TextureFilter = TextureFilterEnum.Nearest,
			Position = -minSize / 2
		};

		Control control = new Control();
		control.AddChild(textureRect);

		return control;
	}

	private void OnPokemonMoveChanged(int teamSlotID, PokemonMove pokemonMove)
	{
		if (ID != teamSlotID) return;

		string changedPokemonMoveMessage = $"{Pokemon.Name}'s Move Is Now {pokemonMove.Name}";
		PrintRich.PrintLine(TextColor.Purple, changedPokemonMoveMessage);
		
		Pokemon.Move = pokemonMove;

		_pokemonMoveButton.Update(pokemonMove);
	}

   	private void OnMoveButtonPressed()
	{
		PokemonTD.Signals.EmitSignal(Signals.SignalName.ChangeMovesetPressed);

		MovesetInterface movesetInterface = PokemonTD.PackedScenes.GetMovesetInterface();
		movesetInterface.Pokemon = Pokemon;
		movesetInterface.TeamSlotID = ID;

		movesetInterface.PokemonMoveChanged += OnPokemonMoveChanged;
		
		StageInterface stageInterface = GetParentOrNull<Node>().GetOwnerOrNull<StageInterface>();
		PokemonStage PokemonStage = stageInterface.GetParentOrNull<PokemonStage>();
		PokemonStage.AddChild(movesetInterface);
	}

	// ? EXP Formula
	// EXP = b * L / 7
	// b = Pokemon Enemy Experience Yield
	// L = Pokemon Enemy Level
	public int GetExperience(PokemonEnemy PokemonEnemy)
	{
		return Mathf.RoundToInt(PokemonEnemy.Pokemon.ExperienceYield * PokemonEnemy.Pokemon.Level / 7);
	}

	public void AddExperience(int experience)
	{
		if (Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;

		_experienceBar.Value += experience;
		Pokemon.MinExperience = (int) _experienceBar.Value;

		string pokemonGainedExperienceMessage = $"{Pokemon.Name} Gained {experience} EXP";
		PrintRich.PrintLine(TextColor.Purple, pokemonGainedExperienceMessage);

		if (_experienceBar.Value >= _experienceBar.MaxValue) LevelUp();
	}

	private void LevelUp()
	{
		while (Pokemon.MinExperience >= Pokemon.MaxExperience)
		{
			Pokemon.Level++;

			_experienceBar.Value -= _experienceBar.MaxValue;
			_experienceBar.MaxValue = Pokemon.GetExperienceRequired();

			Pokemon.MinExperience = (int) _experienceBar.Value;
			Pokemon.MaxExperience = (int) _experienceBar.MaxValue;

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonLeveledUp, Pokemon);

			UpdateControls();

			// Update stats and moves
			Pokemon.SetStats();
			Pokemon.FindAndAddPokemonMove();

			string pokemonLeveledUpMessage = $"{Pokemon.Name} Has Leveled Up To Level {Pokemon.Level}";
			PrintRich.PrintLine(TextColor.Purple, pokemonLeveledUpMessage); ;
		}
	}
}
