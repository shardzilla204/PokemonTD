using Godot;
using System.Collections.Generic;

namespace PokemonTD;

public partial class PokemonTeamSlots : Container
{
    [Export]
    private CustomButton _visibilityToggle;

    [Export]
    private Container _container;

    private bool _isVisible = true;
    private List<PokemonTeamSlot> _PokemonTeamSlots = new List<PokemonTeamSlot>();

    public override void _ExitTree()
    {
        PokemonTD.Signals.EvolutionFinished -= UpdatePokemonTeamSlot;
        PokemonTD.Signals.PokemonTeamUpdated -= PokemonTeamUpdated;
    }

    public override void _Ready()
    {
        PokemonTD.Signals.EvolutionFinished += UpdatePokemonTeamSlot;
        PokemonTD.Signals.PokemonTeamUpdated += PokemonTeamUpdated;

        _visibilityToggle.Pressed += () => 
		{
			PokemonTD.AudioManager.PlayButtonPressed();
			OnVisibilityPressed();
		};
		_visibilityToggle.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
		_visibilityToggle.Text = _isVisible ? "Hide Team" : "Show Team";

        ClearPokemonTeamSlots();
		AddPokemonTeamSlots();
		AddEmptyPokemonTeamSlots();
    }

    private void OnVisibilityPressed()
	{
		_isVisible = !_isVisible;
		_visibilityToggle.Text = _isVisible ? "Hide Team" : "Show Team";

		_container.Visible = _isVisible;
	}

    private void ClearPokemonTeamSlots()
	{
		_PokemonTeamSlots.Clear();
		foreach (Node child in _container.GetChildren())
		{
			child.QueueFree();
		}
	}

	private void RemoveEmptyPokemonTeamSlot(int index)
	{
		Control emptyPokemonTeamSlot = _container.GetChildOrNull<Control>(index);
		emptyPokemonTeamSlot.QueueFree();
	}

	private void AddPokemonTeamSlots()
	{
		for (int i = 0; i < PokemonTeam.Instance.Pokemon.Count; i++)
		{
			PokemonTeamSlot PokemonTeamSlot = GetPokemonTeamSlot(i);
			AddPokemonTeamSlot(PokemonTeamSlot, i);
		}

		// Mutes Team Slots
		foreach (int teamSlotIndex in PokemonTeam.Instance.PokemonTeamSlotsMuted)
		{
			PokemonTeamSlot PokemonTeamSlot = FindPokemonTeamSlot(teamSlotIndex);
			PokemonTeamSlot.SetMute();
		}
	}

	private void AddPokemonTeamSlot(PokemonTeamSlot PokemonTeamSlot, int teamSlotIndex)
	{
		_container.AddChild(PokemonTeamSlot);
		_container.MoveChild(PokemonTeamSlot, teamSlotIndex);
		_PokemonTeamSlots.Insert(teamSlotIndex, PokemonTeamSlot);
	}

	public void UpdatePokemonTeamSlot(Pokemon pokemonEvolution, int teamSlotIndex, int levels)
	{
		PokemonTeamSlot oldPokemonTeamSlot = _container.GetChildOrNull<PokemonTeamSlot>(teamSlotIndex);
		oldPokemonTeamSlot.QueueFree();

		PokemonTeamSlot newPokemonTeamSlot = GetPokemonTeamSlot(teamSlotIndex);
		AddPokemonTeamSlot(newPokemonTeamSlot, teamSlotIndex);
	}

	private PokemonTeamSlot GetPokemonTeamSlot(int teamSlotIndex)
	{
		PokemonTeamSlot PokemonTeamSlot = PokemonTD.PackedScenes.GetPokemonTeamSlot();
		PokemonTeamSlot.TeamSlotIndex = teamSlotIndex;
		
		Pokemon pokemon = PokemonTeam.Instance.Pokemon[teamSlotIndex];
		PokemonTeamSlot.SetControls(pokemon);

		return PokemonTeamSlot;
	}

	// Fill the rest of the slots with an empty slot state
	private void AddEmptyPokemonTeamSlots()
	{
		int emptyTeamSlots = PokemonTD.MaxTeamSize - PokemonTeam.Instance.Pokemon.Count;
		for (int i = 0; i < emptyTeamSlots; i++)
		{
			Control emptyPokemonTeamSlot = PokemonTD.PackedScenes.GetEmptyPokemonTeamSlot();		
			_container.AddChild(emptyPokemonTeamSlot);
		}
	}

    public PokemonTeamSlot FindPokemonTeamSlot(int teamSlotIndex)
	{
		return _PokemonTeamSlots.Find(PokemonTeamSlot => PokemonTeamSlot.TeamSlotIndex == teamSlotIndex);
	}

    private void PokemonTeamUpdated()
	{
		int iteration = PokemonTeam.Instance.Pokemon.Count - 1;
		RemoveEmptyPokemonTeamSlot(iteration);

		PokemonTeamSlot PokemonTeamSlot = GetPokemonTeamSlot(iteration);
		AddPokemonTeamSlot(PokemonTeamSlot, iteration);
	}
}
