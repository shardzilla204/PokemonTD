using System;
using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class PokemonTeamSlot : Button
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
		PokemonTD.Signals.PokemonHealed -= OnPokemonHealed;
		PokemonTD.Signals.PokemonDamaged -= OnPokemonDamaged;
		PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;
    }

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonHealed += OnPokemonHealed;
		PokemonTD.Signals.PokemonDamaged += OnPokemonDamaged;
		PokemonTD.Signals.PokemonEvolved += PokemonEvolved;

		Toggled += (isToggled) => 
		{
			_mutedTexture.Visible = isToggled;
			_isMuted = isToggled;
			PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamSlotMuted, TeamSlotIndex, isToggled);
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
		if (_dragPreview != null) PokemonTD.Tween.TweenSlotDragRotation(_dragPreview, _isDragging);
	}

	public override void _Notification(int what)
	{
		if (what != NotificationDragEnd || !_isDragging) return;

		_dragPreview = null;
		_isDragging = false;
		HighlightPokemonStageSlot();

		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingPokemonTeamSlot, false);

		if (IsDragSuccessful()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUsed, true, TeamSlotIndex);
	}

	public void SetMute()
	{
		_mutedTexture.Visible = true;
		_isMuted = true;
		PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonTeamSlotMuted, TeamSlotIndex, true);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (Disabled) return PokemonTD.GetStageDragPreview(Pokemon);

		_isDragging = true;
		
		if (InUse)
		{
			HighlightPokemonStageSlot();
			return PokemonTD.GetStageDragPreview(Pokemon);
		}

		_dragPreview = PokemonTD.GetStageDragPreview(Pokemon);
		SetDragPreview(_dragPreview);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.DraggingPokemonTeamSlot, true);

		return PokemonTD.GetStageDragData(Pokemon, TeamSlotIndex, true, _isMuted);
	}

	private void PokemonEvolved(Pokemon pokemonEvolution, int teamSlotIndex)
	{
		if (TeamSlotIndex == teamSlotIndex) SetControls(pokemonEvolution);
	}

	private void HighlightPokemonStageSlot()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot PokemonStageSlot = pokemonStage.FindPokemonStageSlot(TeamSlotIndex);
		if (PokemonStageSlot != null) PokemonStageSlot.SetOpacity(_isDragging);
	}

	private Image GetMutedImage()
	{
		int minimumSize = 20;
		Texture2D texture = _mutedTexture.Texture;
		Image image = texture.GetImage();
		image.Resize(minimumSize, minimumSize);
		return image;
	}

	// Update text, textures and progress bars
	public void SetControls(Pokemon pokemon)
	{
		try
		{
			Pokemon = pokemon;

			// Remove Gender Icons In The Name For Nidoran
			string pokemonName = pokemon == null ? "" : pokemon.Name;
        	if (Pokemon != null) pokemonName = pokemon.Name.Contains("Nidoran") ? "Nidoran" : pokemon.Name;

			_pokemonName.Text = pokemonName;
			_pokemonSprite.Texture = pokemon != null ? pokemon.Sprite : null;

			_pokemonExperienceBar.Update(pokemon);
			_pokemonHealthBar.Update(pokemon);

			_genderSprite.Texture = PokemonTD.GetGenderSprite(pokemon);
			
			_pokemonMoveButton.Update(pokemon.Move);
		}
		catch (NullReferenceException error)
		{
			GD.PrintErr($"{error}");
		}
	}

	private void OnPokemonMoveChanged(int teamSlotIndex, PokemonMove pokemonMove)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		Pokemon.Move = pokemonMove;
		_pokemonMoveButton.Update(pokemonMove);

		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot pokemonStageSlot = pokemonStage.FindPokemonStageSlot(TeamSlotIndex);
		pokemonStageSlot.HasCounter = pokemonMove.Name == "Counter";

		// Print Message To Console
		string changedPokemonMoveMessage = $"{Pokemon.Name}'s Move Is Now {pokemonMove.Name}";
		PrintRich.PrintLine(TextColor.Purple, changedPokemonMoveMessage);
	}

   	private void OnMoveButtonPressed()
	{
		PokemonTD.Signals.EmitSignal(Signals.SignalName.ChangeMovesetPressed);

		MovesetInterface movesetInterface = GetMovesetInterface();
		PokemonStage pokemonStage = GetPokemonStage();
		pokemonStage.AddChild(movesetInterface);
	}

	private MovesetInterface GetMovesetInterface()
	{
		MovesetInterface movesetInterface = PokemonTD.PackedScenes.GetMovesetInterface();
		movesetInterface.Pokemon = Pokemon;
		movesetInterface.TeamSlotIndex = TeamSlotIndex;
		movesetInterface.PokemonMoveChanged += OnPokemonMoveChanged;

		return movesetInterface;
	}

	public void AddExperience(int experience)
	{
		if (Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;

		_pokemonExperienceBar.AddExperience(Pokemon, experience);

		// Print Message To Console
		string pokemonGainedExperienceMessage = $"{Pokemon.Name} Gained {experience} EXP";
		PrintRich.PrintLine(TextColor.Purple, pokemonGainedExperienceMessage);
	}

	public void OnPokemonHealed(int health, int teamSlotIndex)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		_pokemonHealthBar.AddHealth(Pokemon, health);
	}

	public void OnPokemonDamaged(int damage, int teamSlotIndex)
	{
		if (TeamSlotIndex != teamSlotIndex) return;

		_pokemonHealthBar.SubtractHealth(Pokemon, damage);
	}

	private void OnPokemonFainted()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot PokemonStageSlot = pokemonStage.FindPokemonStageSlot(TeamSlotIndex);
		PokemonStageSlot.EmitSignal(PokemonStageSlot.SignalName.Fainted, PokemonStageSlot);

		PokemonStageSlot.IsActive = false;
		_pokemonSleepBar.Start(Pokemon);

		Modulate = Modulate.Darkened(0.15f);
		Disabled = true;
	}

	private void OnSleepFinished()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot PokemonStageSlot = pokemonStage.FindPokemonStageSlot(TeamSlotIndex);

		if (PokemonStageSlot == null) return;

		PokemonStageSlot.IsActive = true;
		_pokemonHealthBar.ResetHealth(Pokemon); // Reset Health
		Modulate = Colors.White;
		Disabled = false;
	}

	private PokemonStage GetPokemonStage()
	{
		PokemonTeamSlots PokemonTeamSlots = GetParentOrNull<Node>().GetOwnerOrNull<PokemonTeamSlots>();
		StageInterface stageInterface = PokemonTeamSlots.GetParentOrNull<Node>().GetOwnerOrNull<StageInterface>();
		return stageInterface.GetParentOrNull<PokemonStage>();
	}
}
