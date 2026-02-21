using System;
using System.Threading.Tasks;
using Godot;

public partial class RoutineReminder : Panel {
	const string ROUTINE_APP_LOCATION = "C:\\Users\\alpit\\OneDrive\\Documents\\Krish\\Other Godot\\POC\\Exports\\export.exe";
	[Export]
	bool enabled = false;

	[Export]
	int afternoonHour;

	[Export]
	CheckBox routineLabel;
	[ExportCategory("Cosmetics")]
	[Export]
	GpuParticles2D particles;
	[Export]
	AudioStreamPlayer2D audioPlayer;

	// In minutes
	const int CHECKUP_PERIOD = 25;
	const string SAVE_FILE_LOCATION = "user://routine.txt";

	// Required for timing when to give the next prompt.
	readonly int minuteOpened;
	RoutineReminder() => minuteOpened = DateTime.Now.TimeOfDay.Minutes;

	public void ToggleNotifications(bool toggle) => enabled = toggle;
	
	public override void _Ready() {
		return;
		
		if (!routineLabel.ButtonPressed) Update();
		if (DateTime.Now.TimeOfDay.Hours >= afternoonHour) UpdateToAfternoon();

		routineLabel.Pressed += OnPressed;
		routineLabel.ButtonPressed = LoadIfPressed(); // Do last in case we update to afternoon.
	}

	private static bool LoadIfPressed() {
		using FileAccess saveFile = FileAccess.Open(SAVE_FILE_LOCATION, FileAccess.ModeFlags.Read);
		if (saveFile is null) return false;

        string[] info = saveFile.GetLine().Split(",");
		
		// The second check is to make sure is false if the day is different (new day)
		return info[0] == "True" && info[1] == DateTime.Now.Day.ToString();
	}

	private void Save() {
		using FileAccess saveFile = FileAccess.Open(SAVE_FILE_LOCATION, FileAccess.ModeFlags.Write);
        saveFile.StoreLine($"{routineLabel.ButtonPressed},{DateTime.Now.Day}");
	}

	int currentHour = 0;

	bool HourChanged => currentHour != DateTime.Now.TimeOfDay.Hours;

	async void Update() {
		if (HourChanged && DateTime.Now.TimeOfDay.Hours == afternoonHour) UpdateToAfternoon();

		if ((DateTime.Now.TimeOfDay.Minutes + minuteOpened) % CHECKUP_PERIOD == 0) OpenApp();

		currentHour = DateTime.Now.TimeOfDay.Hours;
		
		// Waits 1 minute to loop for performance gains
		await Task.Delay(1000*60);
		Update();
	}

	private void UpdateToAfternoon() {
		routineLabel.Text = "Finished Night Routine";
		routineLabel.ButtonPressed = false;
	}

	// Open app if button isn't pressed. Button will automagically be reset after 
	// the given time in "afternoonHour" is passed
	private void OpenApp() {
		if (!enabled) return;
		if (!routineLabel.ButtonPressed) System.Diagnostics.Process.Start(ROUTINE_APP_LOCATION);
	}

	Color defaultColor = new(1, 1, 1);
	Color pressedColor = new(1.0f, 1.15f, 1.2f);
	private void OnPressed() {
		Save();
		PlayAnimations();
	}

	private void PlayAnimations() {
		Modulate = routineLabel.ButtonPressed ? pressedColor : defaultColor;
		if (routineLabel.ButtonPressed) {
			particles.Emitting = true;
			audioPlayer.Play(0.1f);
		}
    }
}
