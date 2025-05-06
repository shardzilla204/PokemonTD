using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class StageTeamSlot : Button
{
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

	[Export]
	private TextureRect _mutedTexture;

	private bool _isDragging;
	
	public bool InUse = false;

	public int ID = 0;

	public Pokemon Pokemon;

	private Control _dragPreview;
	private bool _isMuted;

	public override void _Ready()
	{
		Toggled += (isToggled) => 
		{
			_mutedTexture.Visible = isToggled;
			_isMuted = isToggled;
			PokemonTD.Signals.EmitSignal(Signals.SignalName.StageTeamSlotMuted, ID, isToggled);
		};
		_mutedTexture.Visible = false;
		_pokemonMoveButton.Pressed += OnMoveButtonPressed;
		_pokemonSprite.MouseEntered += () => Input.SetCustomMouseCursor(GetMutedImage(), Input.CursorShape.Arrow);
		_pokemonSprite.MouseExited += () => Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow);

		if (Pokemon is not null) UpdateControls();
	}

	public override void _Process(double delta)
	{
		if (_dragPreview is not null) TweenRotate();
	}

	public override void _Notification(int what)
	{
		if (what != NotificationDragEnd || !_isDragging) return;

		_isDragging = false;
		_dragPreview = null;
		
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageTeamSlot, _isDragging);

		if (IsDragSuccessful()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonOnStage, ID);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (InUse) return GetDragPreview();

		_isDragging = true;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageTeamSlot, true);
		
		_dragPreview = GetDragPreview();
		SetDragPreview(_dragPreview);
		return GetDragData();
	}

	private Dictionary<string, Variant> GetDragData()
	{
		return new Dictionary<string, Variant>()
		{
			{ "TeamSlotID", ID },
			{ "FromTeamSlot", true },
			{ "IsMuted", _isMuted },
			{ "Pokemon", Pokemon }
		};
	}

	private Image GetMutedImage()
	{
		int size = 20;
		Texture2D texture = _mutedTexture.Texture;
		Image image = texture.GetImage();
		image.Resize(size, size);
		return image;
	}

	// Update text, textures and progress bars
	private void UpdateControls()
	{
		_pokemonName.Text = Pokemon != null ? $"{Pokemon.Name}" : "";
		_pokemonSprite.Texture = Pokemon != null ? Pokemon.Sprite : null;
		_pokemonLevel.Text = Pokemon != null ? $"LVL. {Pokemon.Level}" : null;

		_experienceBar.Value = Pokemon.MinExperience;
		_experienceBar.MaxValue = Pokemon.MaxExperience;

		_genderSprite.Texture = PokemonTD.GetGenderSprite(Pokemon);

		_pokemonMoveButton.Update(Pokemon.Move);
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
			Position = -new Vector2(minSize.X / 2, minSize.Y / 4),
			PivotOffset = new Vector2(minSize.X / 2, 0)
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
		PokemonStage pokemonStage = stageInterface.GetParentOrNull<PokemonStage>();
		pokemonStage.AddChild(movesetInterface);
	}

	// ? EXP Base Formula
	// ! Will be modified if needed
	// EXP = b * L / 7
	// b = Pokemon Enemy Experience Yield
	// L = Pokemon Enemy Level
	public int GetExperience(PokemonEnemy PokemonEnemy)
	{
		return Mathf.RoundToInt(PokemonEnemy.Pokemon.ExperienceYield * PokemonEnemy.Pokemon.Level / 3);
	}

	public void AddExperience(int experience)
	{
		if (Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;

		_experienceBar.Value += experience;
		Pokemon.MinExperience = (int) _experienceBar.Value;

		string pokemonGainedExperienceMessage = $"{Pokemon.Name} Gained {experience} EXP";
		PrintRich.PrintLine(TextColor.Purple, pokemonGainedExperienceMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Purple, pokemonGainedExperienceMessage);

		if (_experienceBar.Value >= _experienceBar.MaxValue) LevelUp();
	}

	private void LevelUp()
	{
		while (Pokemon.MinExperience >= Pokemon.MaxExperience)
		{
			Pokemon.Level++;

			_experienceBar.Value -= _experienceBar.MaxValue;
			_experienceBar.MaxValue = PokemonManager.Instance.GetExperienceRequired(Pokemon);

			Pokemon.MinExperience = (int) _experienceBar.Value;
			Pokemon.MaxExperience = (int) _experienceBar.MaxValue;

			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonLeveledUp, Pokemon, ID);

			UpdateControls();

			string pokemonLeveledUpMessage = $"{Pokemon.Name} Has Leveled Up To Level {Pokemon.Level}";
			PrintRich.PrintLine(TextColor.Purple, pokemonLeveledUpMessage);
			PokemonTD.AddStageConsoleMessage(TextColor.Purple, pokemonLeveledUpMessage);
		}

		PokemonTD.AudioManager.PlayPokemonLeveledUp();
	}

	private async void TweenRotate()
	{
		Vector2 initialPosition = _dragPreview.Position;

		await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);

		if (!_isDragging) return;

		Vector2 finalPosition = _dragPreview.Position;
		Vector2 direction = finalPosition.DirectionTo(initialPosition);

		float rotationValue = 25;
		float degrees = direction.X > 0 ? -rotationValue : rotationValue;
		degrees = direction.X == 0 ? 0 : degrees;

		float rotation = Mathf.DegToRad(degrees);

		float duration = 0.5f;
		Tween tween = CreateTween();
		tween.TweenProperty(_dragPreview, "rotation", rotation, duration);
	}
}
