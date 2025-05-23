using Godot;

namespace PokemonTD;

public partial class InformationInterface : CanvasLayer
{
    [Export]
    private CustomButton _exitButton;

    [Export]
    private Label _notesLabel;

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

    private int _pageIndex = 0;

    public bool FromMainMenu; // For Pokemon Stage

    public override void _Ready()
    {
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
            if (_pageIndex > PokemonVideos.Instance.Videos.Count) _pageIndex = 0;
            SetVideo(_pageIndex);
        };

        _previousButton.MouseEntered += PokemonTD.AudioManager.PlayButtonHovered;
        _previousButton.Pressed += () =>
        {
            PokemonTD.AudioManager.PlayButtonPressed();
            
            _pageIndex--;
            if (_pageIndex < 0) _pageIndex = PokemonVideos.Instance.Videos.Count;
            SetVideo(_pageIndex);
        };

        SetVideo(_pageIndex);
    }

    private void SetVideo(int pageIndex)
    {
        _notesLabel.Visible = pageIndex == PokemonVideos.Instance.Videos.Count;
        _videoStreamPlayer.Visible = pageIndex != PokemonVideos.Instance.Videos.Count;
        if (pageIndex == PokemonVideos.Instance.Videos.Count)
        {
            _videoTitle.Text = $"Extra Notes";
            _videoCaption.Text = "";
            return;
        }

        PokemonVideo pokemonVideo = PokemonVideos.Instance.Videos[pageIndex];
        _videoTitle.Text = $"{pokemonVideo.Title}";
        _videoCaption.Text = $"{pokemonVideo.Caption}";

        VideoStreamTheora video = PokemonVideos.Instance.GetVideoStream(pageIndex);
        _videoStreamPlayer.Stream = video;
        _videoStreamPlayer.Play();
    }
}
