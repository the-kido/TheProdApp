using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

public partial class Settings : Control {

	[ExportCategory("Required External Nodes")]
	[Export]
	RoutineReminder routineReminder;
	[Export]
	HourlyCheckup hourlyCheckup;
	[Export]
	Button globalMinimizeButton; 
	[ExportCategory("Required Internal Nodes")]
	[Export] 
	Notification notification;
	[Export]
	AnimationPlayer animationPlayer;
	[Export]
	Button settingsButton;
	[Export]
	Gallery gallery;

	[ExportCategory("Settings")]
	[Export]
	CheckBox showAnimations, showGallery;
	[Export]
	Button forceExit;

	bool opened = false;
	private void ToggleSettings() {
		opened = !opened;
		settingsButton.Text = opened ? "Close\nSettings" : "Open\nSettings";
		animationPlayer.Play("open", customSpeed: opened ? 1 : -1, fromEnd: !opened);

		forceExit.Text = EXIT_DEFAULT;
		strikes = 0;
	}
	
	public override void _Ready() {
		animationPlayer.Play("reset"); // in case of reasons

		showAnimations.Toggled += ToggleShowAnimations;
		AnimationReminder(); // Show reminder to make sure I don't forget to re-enable them.
		ToggleShowAnimations(true); // Default them to be on.

		settingsButton.Pressed += ToggleSettings;
		// If settings is opened while minimizing, settings should close too
		globalMinimizeButton.Pressed += () => { if (opened) ToggleSettings();};

		forceExit.Pressed += ForceExitPressed;

		// Gallery
		showGallery.Toggled += ToggleGallery;

	}

	int strikes = 0; // Counts the number of times force exit is pressed.
	const string EXIT_DEFAULT = "Force\nClose";
	const string EXIT_WARNING = "You\nSure?";
	private void ForceExitPressed() {
		strikes++;
		switch (strikes) {
			case 1:
				forceExit.Text = EXIT_WARNING;
				break;
			case 2:
				if (Times.IsSleepingTime()) ShutDownDevice();
				else GetTree().Quit();
				return;
		}
	}

	private static void ShutDownDevice()
	{
		string command;
		string arguments;

		// Detect OS and use appropriate shutdown command
		if (OS.GetName() == "Windows")
		{
			command = "shutdown";
			arguments = "/s /t 0";
		}
		else // Linux and other Unix-like systems
		{
			command = "shutdown";
			arguments = "now";
		}

		var psi = new ProcessStartInfo(command, arguments)
		{
			CreateNoWindow = true,
			UseShellExecute = false
		};
		Process.Start(psi);
	}

	private void ToggleShowAnimations(bool toggled) {
		routineReminder.ToggleNotifications(toggled);
		hourlyCheckup.ToggleCheckup(toggled);
	}

	const int HALF_HOUR = 1000 * 60 * 30;
	private async void AnimationReminder() {
		if (!showAnimations.ButtonPressed) notification.Play();
		await Task.Delay(HALF_HOUR);
		AnimationReminder();
	}

	private void ToggleGallery(bool toggled) {
		gallery.Visible = toggled;
	}
}
