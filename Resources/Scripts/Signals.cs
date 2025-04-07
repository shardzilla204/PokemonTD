using Godot;

namespace PokemonTD;

public partial class Signals : Node
{
    [Signal]
    public delegate void PokemonStarterSelectedEventHandler(Pokemon pokemon);

    [Signal]
    public delegate void PokemonTeamUpdatedEventHandler();

    [Signal]
    public delegate void PokemonEnemyCapturedEventHandler(PokemonEnemy pokemonEnemy);
    
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

    // Stage
    [Signal]
    public delegate void HasWonEventHandler();

    [Signal]
    public delegate void HasLostEventHandler();

    [Signal]
    public delegate void PokemonOnStageEventHandler(int teamSlotID);

    [Signal]
    public delegate void PokemonOffStageEventHandler(int teamSlotID);

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
    public delegate void VisibilityToggledEventHandler(bool isVisible);

    [Signal]
    public delegate void MoveSwappedEventHandler();

    [Signal]
    public delegate void PokemonLearnedMoveEventHandler(Pokemon pokemon, PokemonMove pokemonMove);

    [Signal]
    public delegate void ForgetMoveEventHandler(Pokemon pokemon, PokemonMove pokemonMove);
}
