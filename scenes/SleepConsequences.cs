using Godot;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class SleepConsequences : Panel
{
	const int MINUTE_I_SHOULD_BE_SHUT_DOWN = (12 + 9) * 60 + 30;
	[Export] Button shutDown, letMeExplain, wait;
	[Export] LineEdit edit1, edit2;

	[Export] Panel explainPanel;
	[Export] Window snoozeBarWindow;
	[Export] AnimationPlayer snoozeBarAnimation;


	[Export] Button explainPanelClose;

	int[] snoozes = {5, 3, 2, 1, 1, 1, 1, 1}; // 15 minutes in total. If there's a serious problem, I would shut down the prod app instead of waiting for these timers!
	int indexAt = 0;

	int snoozeTimeMs;

	public override void _Ready()
	{
		letMeExplain.Pressed += () =>
		{
			edit1.Text = "";
			edit2.Text = "";
			explainPanel.Visible = true;
		};

		explainPanelClose.Pressed += () =>
		{
			explainPanel.Visible = false;
		};

        void onLineEditChanged(string _)
        {
			wait.Disabled = string.IsNullOrEmpty(edit1.Text) || string.IsNullOrEmpty(edit2.Text);
        };

		edit1.TextChanged += onLineEditChanged;
		edit2.TextChanged += onLineEditChanged;

		wait.Pressed += async () =>
		{
			(GetParent() as Window).Visible = false;
			var a = snoozeBarAnimation.GetAnimation("play");
			a.TrackSetKeyTime(0, 1, snoozeTimeMs / 1000);
			snoozeBarAnimation.Play("play");
			snoozeBarWindow.Visible = true;

			await Task.Delay(snoozeTimeMs);
			Prompt();
		};

		shutDown.Pressed += () =>
		{
			var psi = new ProcessStartInfo("shutdown", "/s /t 0")
			{
				CreateNoWindow = true,
				UseShellExecute = false
			};
			Process.Start(psi);
		};
	}

    private void Prompt()
    {
		explainPanel.Visible = false;

		snoozeBarAnimation.Stop();
		(GetParent() as Window).Visible = true;
		snoozeBarWindow.Visible = false;

		if (indexAt >= snoozes.Length)
		{
			letMeExplain.Disabled = true;
			wait.Disabled = true;
			return;
		}

		int snoozeMinutes = snoozes[indexAt];
        wait.Text = $"Give me {snoozeMinutes} minute{(snoozeMinutes == 1 ? "" : "s")}";
        snoozeTimeMs = snoozeMinutes * 60 * 1000;

        indexAt++;
    }

    bool timesUp = false;
	public override void _Process(double delta)
	{
		if (timesUp) return;

		if (DateAndTime.Now.Hour * 60 + DateAndTime.Now.Minute > MINUTE_I_SHOULD_BE_SHUT_DOWN)
		{
			Prompt();
			timesUp = true;
		}
	}
}
