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
	
	public int PokemonTeamIndex = 0;
	public Pokemon Pokemon;

	public bool IsRecovering = false;

	private bool _isDragging;
	private Control _dragPreview;

	public override void _ExitTree()
	{
		PokemonTD.Signals.PokemonHealed -= PokemonHealed;
		PokemonTD.Signals.PokemonDamaged -= PokemonDamaged;
		PokemonTD.Signals.PokemonEvolved -= PokemonEvolved;
		PokemonTD.Signals.PokemonMoveChanged -= PokemonMoveChanged;
		PokemonTD.Keybinds.ChangePokemonMove -= ChangePokemonMoveKeybind;

		if (Pokemon != null) Pokemon.ClearStatusConditions();
	}

	public override void _Ready()
	{
		PokemonTD.Signals.PokemonHealed += PokemonHealed;
		PokemonTD.Signals.PokemonDamaged += PokemonDamaged;
		PokemonTD.Signals.PokemonEvolved += PokemonEvolved;
		PokemonTD.Signals.PokemonMoveChanged += PokemonMoveChanged;
		PokemonTD.Keybinds.ChangePokemonMove += ChangePokemonMoveKeybind;

		_pokemonMoveButton.Pressed += MoveButtonPressed;

		_pokemonExperienceBar.LeveledUp += (levels) =>
		{
			PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.PokemonLeveledUp, levels, PokemonTeamIndex);
			_pokemonHealthBar.ResetHealth();
		};
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
		
		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.Dragging, false);
	}

	public override Variant _GetDragData(Vector2 atPosition)
	{
		if (Disabled) return PokemonTD.GetStageDragPreview(Pokemon);

		_isDragging = true;
		_dragPreview = PokemonTD.GetStageDragPreview(Pokemon);
		SetDragPreview(_dragPreview);

		PokemonTD.Signals.EmitSignal(PokemonSignals.SignalName.Dragging, true);

		Dictionary<string, Variant> dataDictionary = new Dictionary<string, Variant>()
		{
			{ "Origin", this },
			{ "Pokemon", Pokemon },
			{ "PokemonTeamIndex", PokemonTeamIndex }
		};
		return dataDictionary;
	}

    public override bool _CanDropData(Vector2 atPosition, Variant data)
    {
		if (data.As<PokeMartItem>() is not PokeMartItem) return false;

		return Pokemon.Stats.HP < Pokemon.Stats.MaxHP;
    }

	public override void _DropData(Vector2 atPosition, Variant data)
	{
		PokeMartItem potion = data.As<PokeMartItem>();
		int healAmount = PokeMart.Instance.GetHealAmount(Pokemon, potion);
		_pokemonHealthBar.AddHealth(healAmount);

		PokemonTD.AudioManager.PlayPokemonHealed();

		potion.Quantity--;
    }

	private void PokemonEvolved(Pokemon pokemonEvolution, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex == pokemonTeamIndex) SetControls(pokemonEvolution);
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

	private void PokemonMoveChanged(int pokemonTeamIndex)
	{
		if (PokemonTeamIndex != pokemonTeamIndex) return;

		_pokemonMoveButton.Update(Pokemon.Move);
		Pokemon.ApplyMoveEffect(Pokemon.Move);
	}

	private void ChangePokemonMoveKeybind(int pokemonTeamIndex)
	{
		if (PokemonTeamIndex == pokemonTeamIndex) MoveButtonPressed();
	}

   	private void MoveButtonPressed()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		pokemonStage.AddMovesetInterface(Pokemon, PokemonTeamIndex);
	}

	public void AddExperience(int experience)
	{
		if (Pokemon.Level >= PokemonTD.MaxPokemonLevel) return;

		_pokemonExperienceBar.AddExperience(Pokemon, experience);

		// Print message to console
		string pokemonGainedExperienceMessage = $"{Pokemon.Name} Gained {experience} EXP";
		PrintRich.PrintLine(TextColor.Purple, pokemonGainedExperienceMessage);
	}

	private void PokemonHealed(int health, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex == pokemonTeamIndex) _pokemonHealthBar.AddHealth(health);
	}

	private void PokemonDamaged(int damage, int pokemonTeamIndex)
	{
		if (PokemonTeamIndex == pokemonTeamIndex) _pokemonHealthBar.SubtractHealth(damage);
	}

	private void PokemonFainted()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot pokemonStageSlot = pokemonStage.FindPokemonStageSlot(PokemonTeamIndex);
		pokemonStageSlot.EmitSignal(PokemonStageSlot.SignalName.Fainted, pokemonStageSlot);
		
		pokemonStageSlot.IsRecovering = true;
		IsRecovering = true;
		float additionalTime = pokemonStageSlot.Pokemon.Effects.UsedFaintMove ? 3.5f : 0;
		_pokemonSleepBar.Start(Pokemon, additionalTime);

		Modulate = Modulate.Darkened(0.15f);
		Disabled = true;
	}

	private void SleepFinished()
	{
		PokemonStage pokemonStage = GetPokemonStage();
		PokemonStageSlot pokemonStageSlot = pokemonStage.FindPokemonStageSlot(PokemonTeamIndex);

		if (pokemonStageSlot == null) return;

		GD.Print("Sleep Finished");

		pokemonStageSlot.IsRecovering = false;
		IsRecovering = false;
		_pokemonHealthBar.ResetHealth(); 
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
