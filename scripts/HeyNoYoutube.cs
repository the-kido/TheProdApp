using Godot;
using System.Linq;
using System.Threading.Tasks;

public partial class HeyNoYoutube : Control
{
	[Export]
	HSlider slider;
	[Export]
	Label time;

	[Export]
	Button commitTime;
	[Export]
	SpinBox customCommitedTime;
	[Export]
	Button btnUp, btnDown, btnLeft, btnRight;
	[Export]
	AnimationPlayer movementAnimation, angryAnimation, cycleThroughSuggestionAnimation;
	[Export]
	Label suggestionText;
	[Export]
	AudioStreamPlayer2D yesAudioPlayer, noAudioPlayer;
	Window parent;

	int commitedTimeSeconds = (int) (1.0 * 3600); // Default is 1 hour


	Vector2 left = new(30, 930), right = new(3510, 930), up = new(1770, 30), down = new(1770, 1830);
	
	void InitMovePoints()
	{
		int x = GetTree().Root.Size.X;
		int y = GetTree().Root.Size.Y;
		
		int windowX = parent.Size.X, windowY = parent.Size.Y;

		left = new(30, y/2 - windowY / 2);
		right = new(x - 30 - windowX, y/2 - windowY / 2);
		up = new(x/2 - windowX / 2, 30);
		down = new(x/2 - windowX / 2, y - 30 - windowY);
	}

    readonly ChromeTabDetector detector = ChromeTabDetector.instance;
	
	private void Associate(Button button, Vector2 direction) {
		button.Pressed += () => {
			// parent.Position = direction;
			// idk why this isn't working :( 

			var anim = movementAnimation.GetAnimation("move");
			
			anim.TrackSetKeyValue(0, 0, (Vector2) parent.Position);
			anim.TrackSetKeyValue(0, 1, direction);
			anim.TrackSetKeyValue(0, 2, direction);
			
			CallDeferred("PlayAnimation");
		};
	}

	private void PlayAnimation() {
		movementAnimation.Play("move");
	}

	public override async void _Ready() {
		parent = GetParent<Window>();
		

		ChooseNewText();
		
		
		Associate(btnLeft, left);
		Associate(btnRight, right);
		Associate(btnUp, up);
		Associate(btnDown, down);

		// detector = new();
		GD.Print(detector.OnYoutube);
		ProcessMode = ProcessModeEnum.Disabled;
		await Task.Delay(10);
		ProcessMode = ProcessModeEnum.Always;

		commitTime.Pressed += () => {
			commitedTimeSeconds = (int) (customCommitedTime.Value * 3600);
			commitTime.Disabled = true;
			customCommitedTime.Editable = false;

			// In case we change the time when it goes into danger zone
			angryIsPlaying = false; 
			angryAnimation.Stop();
			warnIsPlaying = false;
		};

		await Task.Delay(1000); // Just to be safe vs. Main.cs which changes main window size
		InitMovePoints();
	}

	double timeSpent = 0;
	bool showingWindow = false;
	public override void _Process(double delta)
	{
		if (detector.OnYoutube) 
		{
			if (!parent.Visible) 
			{
				movementAnimation.Play("Open");
				noAudioPlayer.Play();
			}
			else timeSpent += delta;
		} 
		else if (parent.Visible && !movementAnimation.IsPlaying()) 
		{
			movementAnimation.PlayBackwards("Open");
			yesAudioPlayer.Play();
		}

		int timeSpentSeconds = (int) timeSpent;

		time.Text = Utils.TimerText(timeSpentSeconds / 3600, timeSpentSeconds % 3600 / 60, timeSpentSeconds % 60);
		slider.Value = 1.0 * timeSpent / commitedTimeSeconds;
		
		// Switch text every 10 seconds
		if (timeSpentSeconds % 10 == 0)
		{
			cycleThroughSuggestionAnimation.Play("suggest");
		}

		if (slider.Value >= 0.85 && !warnIsPlaying)
		{
			warnIsPlaying = true;
			angryAnimation.Play("warn");
		}
		
		if (timeSpentSeconds >= commitedTimeSeconds && !angryIsPlaying) 
		{
			angryIsPlaying = true;
			angryAnimation.Play("pulse");
		}
	}
	bool angryIsPlaying = false, warnIsPlaying = false;

	static readonly string[] thingsToDo = {
		"Go through your QMB",
		"Get some rest instead",
		"Go outside",
		"Did you finish everything in your QMB?",
		"Progress your career",
		"Develop your experience",
	};
	int index = 0;

	public void ChooseNewText()
	{
		suggestionText.Text = $"ⓘ {thingsToDo[index]}";
		index = (index + 1) % thingsToDo.Length;
	}
}

