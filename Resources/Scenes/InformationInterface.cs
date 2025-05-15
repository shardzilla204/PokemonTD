using Godot;
using Godot.Collections;

namespace PokemonTD;

public partial class InformationInterface : CanvasLayer
{
    [Export]
    private CustomButton _exitButton;

    [Export]
    private VideoStreamPlayer _videoStreamPlayer;

    [Export]
    private Label _videoTitle;

    [Export]
    private Label _videoCaption;

    [Export]
    private CustomButton _nextButton;

    [Export]
    private CustomButton _previousButton;

    [ExportCategory("Videos")]
    [ExportSubgroup("Pokemon Stage")]
    [Export]
    private VideoStreamTheora _placingPokemon;

    [Export]
    private VideoStreamTheora _catchingPokemon;

    [Export]
    private VideoStreamTheora _changingMoves;

    [Export]
    private VideoStreamTheora _movingPokemon;

    [ExportSubgroup("Poke Center")]

    [Export]
    private VideoStreamTheora _analyzingPokemon;

    [Export]
    private VideoStreamTheora _changingPokemonTeam;

    private int _pageIndex;

    public bool FromMainMenu;

    Dictionary<int, VideoStreamTheora> _videoDictionary = new Dictionary<int, VideoStreamTheora>();

    Dictionary<int, string> _videoTitleDictionary = new Dictionary<int, string>()
    {
        { 0, "Placing Pokemon" },
        { 1, "Changing The Pokemon's Move" },
        { 2, "Catching Pokemon" },
        { 3, "Moving Pokemon" },
        { 4, "Changing Pokemon Team" },
        { 5, "Analyzing Pokemon" }
    };

    Dictionary<int, string> _videoCaptionDictionary = new Dictionary<int, string>()
    {
        { 0, "Drag & Drop The Pokemon Onto A Highlighted Slot. Double Click To Retrieve The Pokemon." },
        { 1, "Click To Open The Pokemon's Moveset Menu And Select A Move." },
        { 2, "Drag & Drop The Pokeball Onto An Enemy Pokemon When It's Health Is Red." },
        { 3, "You Can Swap Between Slots Wherever They Are On The Map. You Can Also Swap Between Pokemon." },
        { 4, "Double Click The Pokemon From Your Team To Send It Back To The PC. Vise Versa." },
        { 5, "Drag & Drop Your Pokemon Into The Area To See Various Stats." }
    };

    public override void _Ready()
    {
        // Add videos to video dictionary
        _videoDictionary.Add(0, _placingPokemon);
        _videoDictionary.Add(1, _changingMoves);
        _videoDictionary.Add(2, _catchingPokemon);
        _videoDictionary.Add(3, _movingPokemon);
        _videoDictionary.Add(4, _changingPokemonTeam);
        _videoDictionary.Add(5, _analyzingPokemon);

        _exitButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
        _exitButton.Pressed += () =>
        {
            PokemonTD.AudioManager.PlayButtonPressed();
            
            SettingsInterface settingsInterface = PokemonTD.PackedScenes.GetSettingsInterface();
            settingsInterface.FromMainMenu = FromMainMenu;
            AddSibling(settingsInterface);
            QueueFree();
        };

        _nextButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
        _nextButton.Pressed += () =>
        {
            PokemonTD.AudioManager.PlayButtonPressed();

            _pageIndex++;
            if (_pageIndex > _videoDictionary.Count - 1) _pageIndex = 0;
            SetVideo(_pageIndex);
        };

        _previousButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
        _previousButton.Pressed += () =>
        {
            PokemonTD.AudioManager.PlayButtonPressed();
            
            _pageIndex--;
            if (_pageIndex < 0) _pageIndex = _videoDictionary.Count - 1;
            SetVideo(_pageIndex);
        };

        SetVideo(_pageIndex);
    }

    private void SetVideo(int pageIndex)
    {
        _videoTitle.Text = $"{_videoTitleDictionary[pageIndex]}";
        _videoCaption.Text = $"{_videoCaptionDictionary[pageIndex]}";
        VideoStreamTheora video = _videoDictionary[pageIndex];
        _videoStreamPlayer.Stream = video;
        _videoStreamPlayer.Play();
    }
}
