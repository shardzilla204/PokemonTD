using System;
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
	private PokemonExperienceBar _pokemonExperienceBar;

	[Export]
	private PokemonHealthBar _pokemonHealthBar;

	[Export]
	private PokemonSleepBar _pokemonSleepBar;

	[Export]
	private PokemonMoveButton _pokemonMoveButton;

	[Export]
	private TextureRect _mutedTexture;

	private bool _isDragging;
	
	public int TeamSlotIndex = 0;
	public Pokemon Pokemon;
	public bool InUse = false;

	private Control _dragPreview;
	private bool _isMuted;

    public override void _ExitTree()
    {
		PokemonTD.Signals.PokemonDamaged -= OnPokemonDamaged;
		PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;
    }

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonDamaged += OnPokemonDamaged;
		PokemonTD.Signals.PokemonEvolved += PokemonEvolved;
		Toggled += (isToggled) => 
		{
			_mutedTexture.Visible = isToggled;
			_isMuted = isToggled;
			PokemonTD.Signals.EmitSignal(Signals.SignalName.StageTeamSlotMuted, TeamSlotIndex, isToggled);
		};
		_mutedTexture.Visible = false;
		_pokemonMoveButton.Pressed += OnMoveButtonPressed;
		_pokemonSprite.MouseEntered += () => Input.SetCustomMouseCursor(GetMutedImage(), Input.CursorShape.Arrow);
		_pokemonSprite.MouseExited += () => Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow);

		_pokemonExperienceBar.LeveledUp += (levels) => PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonLeveledUp, Pokemon, TeamSlotIndex, levels);
		_pokemonHealthBar.Fainted += OnPokemonFainted;
		_pokemonSleepBar.Finished += OnSleepFinished;
	}

	public override void _Process(double delta)
	{
		if (_dragPreview == null) return;
		
		PokemonTD.Tween.TweenSlotDragRotation(_dragPreview, _isDragging);
	}

	public override void _Notification(int what)
	{
		if (what != NotificationDragEnd || !_isDragging) return;

		_dragPreview = null;
		_isDragging = false;
		HighlightStageSlot();

		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageTeamSlot, false);

		if (IsDragSuccessful()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonInUse, true, TeamSlotIndex);
	}

	public void SetMute()
	{
		_mutedTexture.Visible = true;
		_isMuted = true;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.StageTeamSlotMuted, TeamSlotIndex, true);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (Disabled) return GetDragPreview();

		_isDragging = true;
		if (InUse) 
		{
			HighlightStageSlot();
			return GetDragPreview();
		}

		_dragPreview = GetDragPreview();
		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingStageTeamSlot, true);

		SetDragPreview(_dragPreview);
		return GetDragData();
	}

	private void PokemonEvolved(Pokemon pokemonEvolution, int teamSlotIndex)
	{
		if (TeamSlotIndex != teamSlotIndex) return;
		SetControls(pokemonEvolution);
	}

	private void HighlightStageSlot()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		StageSlot stageSlot = pokemonStage.FindStageSlot(TeamSlotIndex);
		if (stageSlot is null) return;
		stageSlot.SetOpacity(_isDragging);
	}

	private Dictionary<string, Variant> GetDragData()
	{
		return new Dictionary<string, Variant>()
		{
			{ "TeamSlotIndex", TeamSlotIndex },
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
	public void SetControls(Pokemon pokemon)
	{
		try
		{
			Pokemon = pokemon;

			string pokemonName = pokemon == null ? "" : pokemon.Name;
        	if (Pokemon != null) pokemonName = pokemon.Name.Contains("Nidoran") ? "Nidoran" : pokemon.Name;

			_pokemonName.Text = pokemonName;
			_pokemonSprite.Texture = pokemon != null ? pokemon.Sprite : null;

			_pokemonExperienceBar.Update(pokemon);
			_pokemonHealthBar.Update(pokemon);

			_genderSprite.Texture = PokemonTD.GetGenderSprite(pokemon);
			
			_pokemonMoveButton.Update(pokemon.Move);
		}
		catch (NullReferenceException e)
		{
			GD.PrintErr($"{e}");
		}
	}

	private Control GetDragPreview()
	{
		int minValue = 125;
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

	private void OnPokemonMoveChanged(int id, PokemonMove pokemonMove)
	{
		if (TeamSlotIndex != id) return;

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
		movesetInterface.TeamSlotIndex = TeamSlotIndex;

		movesetInterface.PokemonMoveChanged += OnPokemonMoveChanged;
		
		PokemonStage pokemonStage = GetPokemonStage();
		pokemonStage.AddChild(movesetInterface);
	}

	public void AddExperience(int experience)
	{
		if (Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;

		_pokemonExperienceBar.AddExperience(Pokemon, experience);

		string pokemonGainedExperienceMessage = $"{Pokemon.Name} Gained {experience} EXP";
		PrintRich.PrintLine(TextColor.Purple, pokemonGainedExperienceMessage);
		PokemonTD.AddStageConsoleMessage(TextColor.Purple, pokemonGainedExperienceMessage);
	}

	public void OnPokemonDamaged(int damage, int teamSlotIndex)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		_pokemonHealthBar.SubtractHealth(Pokemon, damage);
	}

	private void OnPokemonFainted()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		StageSlot stageSlot = pokemonStage.FindStageSlot(TeamSlotIndex);
		stageSlot.EmitSignal(StageSlot.SignalName.Fainted, stageSlot);

		stageSlot.IsActive = false;
		_pokemonSleepBar.Start(Pokemon);

		Modulate = Modulate.Darkened(0.15f);
		Disabled = true;
	}

	private void OnSleepFinished()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		StageSlot stageSlot = pokemonStage.FindStageSlot(TeamSlotIndex);

		if (stageSlot == null) return;

		stageSlot.IsActive = true;
		_pokemonHealthBar.ResetHealth(Pokemon); // Reset Health
		Modulate = Colors.White;
		Disabled = false;
	}

	private PokemonStage GetPokemonStage()
	{
		StageTeamSlots stageTeamSlots = GetParentOrNull<Node>().GetOwnerOrNull<StageTeamSlots>();
		StageInterface stageInterface = stageTeamSlots.GetParentOrNull<Node>().GetOwnerOrNull<StageInterface>();
		return stageInterface.GetParentOrNull<PokemonStage>();
	}
}
