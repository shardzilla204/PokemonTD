using Godot;
using Godot.Collections;
using System;

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
	
	public int PokemonTeamIndex = 0;
	public Pokemon Pokemon;
	public bool InUse = false;

	private bool _isDragging;
	private Control _dragPreview;

	public override void _ExitTree()
	{
		PokemonTD.Signals.PokemonHealed -= PokemonHealed;
		PokemonTD.Signals.PokemonDamaged -= PokemonDamaged;
		PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;

		if (Pokemon != null) Pokemon.ClearStatusConditions();
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonHealed += PokemonHealed;
		PokemonTD.Signals.PokemonDamaged += PokemonDamaged;
		PokemonTD.Signals.PokemonEvolved += PokemonEvolved;

		_mutedTexture.Visible = false;
		_pokemonMoveButton.Pressed += MoveButtonPressed;
		_pokemonSprite.MouseEntered += () => Input.SetCustomMouseCursor(GetMutedImage(), Input.CursorShape.Arrow);
		_pokemonSprite.MouseExited += () => Input.SetCustomMouseCursor(null, Input.CursorShape.Arrow);

		_pokemonExperienceBar.LeveledUp += (levels) => PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonLeveledUp, levels, PokemonTeamIndex);
		_pokemonHealthBar.Fainted += PokemonFainted;
		_pokemonSleepBar.Finished += SleepFinished;
	}

	public override void _Process(double delta)
	{
		if (_dragPreview != null) PokemonTD.Tween.TweenSlotDragRotation(_dragPreview, _isDragging);
	}

	public override void _Notification(int what)
	{
		if (what != NotificationDragEnd || !_isDragging) return;

		_isDragging = false;
		_dragPreview = null;
		
		PokemonTD.Signals.EmitSignal(Signals.SignalName.Dragging, false);

		if (IsDragSuccessful()) PokemonTD.Signals.EmitSignal(Signals.SignalName.PokemonUsed, true, PokemonTeamIndex);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (Disabled) return PokemonTD.GetStageDragPreview(Pokemon);

		_isDragging = true;
		_dragPreview = PokemonTD.GetStageDragPreview(Pokemon);
		SetDragPreview(_dragPreview);

		PokemonTD.Signals.EmitSignal(Signals.SignalName.Dragging, true);

		Dictionary<string, Variant> dataDictionary = new Dictionary<string, Variant>()
		{
			{ "Origin", this },
			{ "Pokemon", Pokemon },
			{ "PokemonTeamIndex", PokemonTeamIndex }
		};
		return dataDictionary;
	}

	private void PokemonEvolved(Pokemon pokemonEvolution, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex == pokemonTeamIndex) SetControls(pokemonEvolution);
	}

	private Image GetMutedImage()
	{
		int minSize = 20;
		Texture2D texture = _mutedTexture.Texture;
		Image image = texture.GetImage();
		image.Resize(minSize, minSize);
		return image;
	}

	// Update text, textures and progress bars
	public void SetControls(Pokemon pokemon)
	{
		try
		{
			Pokemon = pokemon;

			// Remove gender icons in the name for Nidoran
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

	private void PokemonMoveChanged(int pokemonTeamIndex, PokemonMove pokemonMove)
	{
		if (PokemonTeamIndex != pokemonTeamIndex) return;

		Pokemon.Move = pokemonMove;
		_pokemonMoveButton.Update(pokemonMove);

		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot pokemonStageSlot = pokemonStage.FindPokemonStageSlot(PokemonTeamIndex);
		if (pokemonStageSlot != null) pokemonStageSlot.Effects.HasCounter = pokemonMove.Name == "Counter";

		// Print message to console
		string changedPokemonMoveMessage = $"{Pokemon.Name}'s Move Is Now {pokemonMove.Name}";
		PrintRich.PrintLine(TextColor.Purple, changedPokemonMoveMessage);
	}

   	private void MoveButtonPressed()
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
		movesetInterface.PokemonTeamIndex = PokemonTeamIndex;
		movesetInterface.PokemonMoveChanged += PokemonMoveChanged;

		return movesetInterface;
	}

	public void AddExperience(int experience)
	{
		if (Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;

		_pokemonExperienceBar.AddExperience(Pokemon, experience);

		// Print message to console
		string pokemonGainedExperienceMessage = $"{Pokemon.Name} Gained {experience} EXP";
		PrintRich.PrintLine(TextColor.Purple, pokemonGainedExperienceMessage);
	}

	public void PokemonHealed(int health, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex != pokemonTeamIndex) return;

		_pokemonHealthBar.AddHealth(Pokemon, health);
	}

	public void PokemonDamaged(int damage, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex != pokemonTeamIndex) return;

		_pokemonHealthBar.SubtractHealth(Pokemon, damage);
	}

	private void PokemonFainted()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot pokemonStageSlot = pokemonStage.FindPokemonStageSlot(PokemonTeamIndex);
		pokemonStageSlot.EmitSignal(PokemonStageSlot.SignalName.Fainted, pokemonStageSlot);
		
		pokemonStageSlot.IsActive = false;
		float additionalTime = pokemonStageSlot.Effects.UsedFaintMove ? 3.5f : 0;
		_pokemonSleepBar.Start(Pokemon, additionalTime);

		Modulate = Modulate.Darkened(0.15f);
		Disabled = true;
	}

	private void SleepFinished()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot pokemonStageSlot = pokemonStage.FindPokemonStageSlot(PokemonTeamIndex);

		if (pokemonStageSlot == null) return;

		pokemonStageSlot.IsActive = true;
		_pokemonHealthBar.ResetHealth(Pokemon); 
		Modulate = Colors.White;
		Disabled = false;
	}

	private PokemonStage GetPokemonStage()
	{
		PokemonTeamSlots pokemonTeamSlots = GetParentOrNull<PokemonTeamSlots>();
		StageInterface stageInterface = pokemonTeamSlots.GetParentOrNull<Node>().GetOwnerOrNull<StageInterface>();
		return stageInterface.GetParentOrNull<PokemonStage>();
	}
}
