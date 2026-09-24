using Godot;
using System.Linq;
using System.Threading.Tasks;

public partial class SleepReminderBetter : Panel
{
	[Export] Button buttonGood, buttonGreat, buttonBad;
	
	[Export] Button finish;
	[Export] LineEdit sleepTime;
	[Export] SpinBox minsBeforeShutdown;
	[Export] Label friendlySleepTimeText, friendlyShutdownText;

	const string PLACEHOLDER_WHY_NOT = "Why not!? Are you sure?";
	const string PLACEHOLDER_WHATCHA_DOING = "Great! What will you be doing?";

	public static int? ABSOLUTE_MAX_MIN { get; private set; } = null;

	// Called when the node enters the scene tree for the first time.
	Color gray = new("ffffff30");

	bool step1 = false; // Sleep time is set
	bool step2 = false; // Mins b4 sleep time is set

	void UpdateFinishButton() => finish.Disabled = !(step1 && step2);

	int possibleSleepTimeTime, possibleShutDownTime;

	static string GetTimerTime (int mins) => $"⏰{mins / 60 % 12}:{mins % 60:D2} {(mins > 60 * 12 ? "PM": "AM")}";
	public override void _Ready()
	{
		Button[] arr = [buttonGood, buttonGreat, buttonBad];

		foreach (Button button in arr)
		{
			button.Pressed += () =>
			{
				arr.Where((item) => item != button).ToList().ForEach((item) => item.Modulate = gray);
				foreach (Button _button in arr) _button.Disabled = true;

				if (button.Name == buttonGreat.Name)
				{
					// Skip that
					step1 = true;
					UpdateFinishButton();
				}
				else
				{
					// reason.Visible = true;
					// reason.PlaceholderText = button.Name == buttonGood.Name ? PLACEHOLDER_WHATCHA_DOING : PLACEHOLDER_WHY_NOT;
                }
			};
		}

		// reason.TextChanged += () =>
		// {
		// 	step1 = true;
		// 	UpdateFinishButton();
		// };

		finish.Pressed += () =>
		{
			ABSOLUTE_MAX_MIN = possibleShutDownTime;
			(GetParent() as Window).Visible = false;
		};

		sleepTime.TextChanged += newString =>
        {
			// Expect format HH:mm (24-hour). Validate and ensure it's a future time today.
			if (string.IsNullOrWhiteSpace(newString) || !MyRegex().IsMatch(newString))
			{
				step1 = false;
				friendlySleepTimeText.Text = "⏰[Invalid]";
				UpdateFinishButton();
				return;
			}

			var parts = newString.Split(':');
			if (!int.TryParse(parts[0], out int hh) || !int.TryParse(parts[1], out int mm))
			{
				step1 = false;
				friendlySleepTimeText.Text = "⏰[Invalid]";
				UpdateFinishButton();
				return;
			}

			int mins = hh * 60 + mm;
			possibleSleepTimeTime = mins;

			step1 = mins >= Times.MINUTE_I_SHOULD_BE_SHUT_DOWN - 60;
			
			friendlySleepTimeText.Text = step1 ? $"Sleep at {GetTimerTime(mins)}" : friendlySleepTimeText.Text = "⏰[Invalid]";
			UpdateFinishButton();
		};

		minsBeforeShutdown.ValueChanged += newMins =>
		{
			step2 = true;
			int shutDownMin = possibleSleepTimeTime - (int) newMins;
			possibleShutDownTime = shutDownMin;

			friendlyShutdownText.Text = step2 ? $"Shut down at {GetTimerTime(shutDownMin)}" : "Invalid shut down time";
			UpdateFinishButton();
		};

        _ = Loop();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.

	bool stopEverything = false;
	private async Task Loop()
	{
		if (stopEverything) return;

		Update();
		await Task.Delay(5000);
		_ = Loop();
	}

	
	private void Update()
	{
		if (Times.IsWarnTime())
		{
			(GetParent() as Window).Visible = true;
			stopEverything = true;
		}
	}

	private static System.Text.RegularExpressions.Regex MyRegex() =>
		new("^\\d{1,2}:\\d{2}$");
}
