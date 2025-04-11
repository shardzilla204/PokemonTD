using Godot;

namespace PokemonTD;

public partial class Signals : Node
{
    [Signal]
    public delegate void PokemonStarterSelectedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonTeamUpdatedEventHandler();

    // Pokemon
    [Signal]
    public delegate void PokemonUpdatedEventHandler();

    [Signal]
    public delegate void PokemonLeveledUpEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonEvolvedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonGainedExperienceEventHandler(Pokemon pokemon);

    // Enemy Pokemon
    [Signal]
    public delegate void PokemonEnemyFaintedEventHandler(PokemonEnemy pokemonEnemy);

    [Signal]
    public delegate void PokemonEnemyPassedEventHandler(PokemonEnemy pokemonEnemy);
    
    [Signal]
    public delegate void PokemonEnemyCapturedEventHandler(PokemonEnemy pokemonEnemy);

    [Signal]
    public delegate void DraggingTeamStageSlotEventHandler(bool isDragging);

    [Signal]
    public delegate void DraggingStageSlotEventHandler(bool isDragging);

    [Signal]
    public delegate void PokemonOnStageEventHandler(int teamSlotID);

    [Signal]
    public delegate void PokemonOffStageEventHandler(int teamSlotID);

    [Signal]
    public delegate void PokeCenterTeamSlotRemovedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokeCenterSlotRemovedEventHandler(Pokemon pokemon); 

    // Stage
    [Signal]
    public delegate void HasWonEventHandler();

    [Signal]
    public delegate void HasLostEventHandler();

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
    public delegate void MoveSwappedEventHandler();

    [Signal]
    public delegate void PokemonLearnedMoveEventHandler(Pokemon pokemon, PokemonMove pokemonMove);

    [Signal]
    public delegate void ForgetMoveEventHandler(Pokemon pokemon, PokemonMove pokemonMove);
}
