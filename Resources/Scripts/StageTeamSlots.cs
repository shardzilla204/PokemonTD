using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class StageTeamSlots : Container
{
    [Export]
    private CustomButton _visibilityToggle;

    [Export]
    private Container _container;

    private bool _isVisible = true;
    private List<StageTeamSlot> _stageTeamSlots = new List<StageTeamSlot>();

    public override void _ExitTree()
    {
        PokemonTD.Signals.EvolutionFinished -= UpdateStageTeamSlot;
        PokemonTD.Signals.PokemonTeamUpdated -= PokemonTeamUpdated;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.EvolutionFinished += UpdateStageTeamSlot;
        PokemonTD.Signals.PokemonTeamUpdated += PokemonTeamUpdated;

        _visibilityToggle.Pressed += () => 
		{
			PokemonTD.AudioManager.PlayButtonPressed();
			OnVisibilityPressed();
		};
		_visibilityToggle.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_visibilityToggle.Text = _isVisible ? "Hide Team" : "Show Team";

        ClearStageTeamSlots();
		AddStageTeamSlots();
		AddEmptyStageTeamSlots();
    }

    private void OnVisibilityPressed()
	{
		_isVisible = !_isVisible;
		_visibilityToggle.Text = _isVisible ? "Hide Team" : "Show Team";

		_container.Visible = _isVisible;
	}

    private void ClearStageTeamSlots()
	{
		_stageTeamSlots.Clear();
		foreach (Node child in _container.GetChildren())
		{
			child.QueueFree();
		}
	}

	private void RemoveEmptyStageTeamSlot(int index)
	{
		Control emptyStageTeamSlot = _container.GetChildOrNull<Control>(index);
		emptyStageTeamSlot.QueueFree();
	}

	private void AddStageTeamSlots()
	{
		for (int i = 0; i < PokemonTeam.Instance.Pokemon.Count; i++)
		{
			StageTeamSlot stageTeamSlot = GetStageTeamSlot(i);
			AddStageTeamSlot(stageTeamSlot, i);
		}

		// Mutes Team Slots
		foreach (int teamSlotIndex in PokemonTeam.Instance.StageTeamSlotsMuted)
		{
			StageTeamSlot stageTeamSlot = FindStageTeamSlot(teamSlotIndex);
			stageTeamSlot.SetMute();
		}
	}

	private void AddStageTeamSlot(StageTeamSlot stageTeamSlot, int teamSlotIndex)
	{
		_container.AddChild(stageTeamSlot);
		_container.MoveChild(stageTeamSlot, teamSlotIndex);
		_stageTeamSlots.Insert(teamSlotIndex, stageTeamSlot);
	}

	public void UpdateStageTeamSlot(Pokemon pokemonEvolution, int teamSlotIndex, int levels)
	{
		StageTeamSlot oldStageTeamSlot = _container.GetChildOrNull<StageTeamSlot>(teamSlotIndex);
		oldStageTeamSlot.QueueFree();

		StageTeamSlot newStageTeamSlot = GetStageTeamSlot(teamSlotIndex);
		AddStageTeamSlot(newStageTeamSlot, teamSlotIndex);
	}

	private StageTeamSlot GetStageTeamSlot(int teamSlotIndex)
	{
		StageTeamSlot stageTeamSlot = PokemonTD.PackedScenes.GetStageTeamSlot();
		stageTeamSlot.TeamSlotIndex = teamSlotIndex;
		
		Pokemon pokemon = PokemonTeam.Instance.Pokemon[teamSlotIndex];
		stageTeamSlot.SetControls(pokemon);

		return stageTeamSlot;
	}

	// Fill the rest of the slots with an empty slot state
	private void AddEmptyStageTeamSlots()
	{
		int emptyTeamSlots = PokemonTD.MaxTeamSize - PokemonTeam.Instance.Pokemon.Count;
		for (int i = 0; i < emptyTeamSlots; i++)
		{
			Control emptyStageTeamSlot = PokemonTD.PackedScenes.GetEmptyStageTeamSlot();		
			_container.AddChild(emptyStageTeamSlot);
		}
	}

    public StageTeamSlot FindStageTeamSlot(int teamSlotIndex)
	{
		return _stageTeamSlots.Find(stageTeamSlot => stageTeamSlot.TeamSlotIndex == teamSlotIndex);
	}

    private void PokemonTeamUpdated()
	{
		int iteration = PokemonTeam.Instance.Pokemon.Count - 1;
		RemoveEmptyStageTeamSlot(iteration);

		StageTeamSlot stageTeamSlot = GetStageTeamSlot(iteration);
		AddStageTeamSlot(stageTeamSlot, iteration);
	}
}
