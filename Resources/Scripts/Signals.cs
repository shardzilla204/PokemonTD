using Godot;

namespace PokémonTD;

public partial class Signals : Node
{
    [Signal]
    public delegate void PokémonStarterSelectedEventHandler(Pokémon pokémon);

    [Signal]
    public delegate void PokémonTeamUpdatedEventHandler();

    [Signal]
    public delegate void PokémonEnemyCapturedEventHandler(PokémonEnemy pokémonEnemy);
    // Pokémon
    [Signal]
    public delegate void PokémonUpdatedEventHandler();

    [Signal]
    public delegate void PokémonLeveledUpEventHandler(Pokémon pokémon);

    [Signal]
    public delegate void PokémonEvolvedEventHandler(Pokémon pokémon);

    [Signal]
    public delegate void PokémonGainedExperienceEventHandler(Pokémon pokémon);

    // Enemy Pokémon
	[Signal]
    public delegate void PokémonEnemyPassedEventHandler(PokémonEnemy pokémonEnemy);

    [Signal]
    public delegate void PokémonEnemyFaintedEventHandler(PokémonEnemy pokémonEnemy);

    // Stage
    [Signal]
    public delegate void StartedWaveEventHandler();

    [Signal]
    public delegate void FinishedWaveEventHandler();

    [Signal]
    public delegate void HasWonEventHandler();

    [Signal]
    public delegate void HasLostEventHandler();

    [Signal]
    public delegate void PokémonOnStageEventHandler(int teamSlotID);

    [Signal]
    public delegate void PokémonOffStageEventHandler(int teamSlotID);

    [Signal]
    public delegate void ChangeMovesetPressedEventHandler();

    // Stage Controls
    [Signal]
    public delegate void PressedPlayEventHandler();

    [Signal]
    public delegate void PressedPauseEventHandler();

    [Signal]
    public delegate void SpeedToggledEventHandler(float speed);

    [Signal]
    public delegate void PokémonEnemyAttackedEventHandler();

    [Signal]
    public delegate void VisibilityToggledEventHandler(bool isVisible);

    [Signal]
    public delegate void InventorySlotAddedEventHandler(Pokémon pokémon);

    [Signal]
    public delegate void InventoryTeamSlotAddedEventHandler(Pokémon pokémon, int teamSlotID);

    [Signal]
    public delegate void InventorySlotRemovedEventHandler(Pokémon pokémon);

    [Signal]
    public delegate void InventoryTeamSlotRemovedEventHandler(Pokémon pokémon, int teamSlotID);
}
