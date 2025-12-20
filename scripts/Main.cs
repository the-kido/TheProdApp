using Godot;

public partial class Main : Control {

	Main() => Instance = this;
	
	public static Main Instance {get; private set;}

	[Export]
	Button minimize;


	[Export]
	AnimationPlayer animationPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GetTree().AutoAcceptQuit = false;
		minimize.Pressed += PlayMinimizeAnimation;

		FindChild("Main background").GetChild<AnimationPlayer>(0).Play("spin");

		var screenSize = DisplayServer.ScreenGetSize();
		GetWindow().Size = screenSize;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Escape")) PlayMinimizeAnimation();
	}

	private void PlayMinimizeAnimation()
	{
		animationPlayer.Play("Minimize");
		focused = false;
	}

	public static void Minimize()
	{
		DisplayServer.WindowSetMode(DisplayServer.WindowMode.Minimized);
	}

	private void temp()
	{
		if (GetTree().Root.HasFocus() || !focused) {
			animationPlayer.Play("Open");
			focused = true;
		}
	}
	

	bool focused = true;
	public override void _Notification(int what) {
		if (what == MainLoop.NotificationApplicationFocusIn) {
			CallDeferred("temp");
		}
		if (what == MainLoop.NotificationApplicationFocusOut) {
			
		}
	}
}
