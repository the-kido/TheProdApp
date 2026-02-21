using Godot;
using System.Linq;
using System.Threading.Tasks;

public partial class SleepReminderBetter : Panel
{
	[Export] Button buttonGood, buttonGreat, buttonBad;
	[Export] TextEdit reason;
	[Export] Button finish;

	const string PLACEHOLDER_WHY_NOT = "Why not!? Are you sure?";
	const string PLACEHOLDER_WHATCHA_DOING = "Great! What will you be doing?";

	// Called when the node enters the scene tree for the first time.
	Color gray = new("ffffff30");
	public override void _Ready()
	{
		Button[] arr = { buttonGood, buttonGreat, buttonBad };
		foreach (Button button in arr)
		{
			button.Pressed += () =>
			{
				arr.Where((item) => item != button).ToList().ForEach((item) => item.Modulate = gray);
				foreach (Button _button in arr) _button.Disabled = true;

				if (button.Name == buttonGreat.Name)
				{
					// Skip that
					finish.Visible = true;
				}
				else
				{
					reason.Visible = true;
					reason.PlaceholderText = button.Name == buttonGood.Name ? PLACEHOLDER_WHATCHA_DOING : PLACEHOLDER_WHY_NOT;
                }
			};
		}

		reason.TextChanged += () =>
		{
			finish.Visible = true;
		};

		finish.Pressed += () =>
		{
			(GetParent() as Window).Visible = false;
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
}
