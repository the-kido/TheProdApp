using System;
using System.Linq;
using Godot;
using Godot.Collections;

public partial class GoalTracker : Panel {

	static readonly string[] goals = {
		"Routine",
		"Cardio",
		"Art",
		// "Productivity"
	};
	static Dictionary<string, string> goalDescription = new() {
		{goals[0], "Read and did Routine"},
		{goals[1], "Did my daily workout"},
		{goals[2], "Drew today"},
		// {goals[3], "Was overall productive"},
	};

	[Export]
	VBoxContainer goalsContainer;
	[Export]
	CheckBox prototype;
	[Export]
	AudioStreamPlayer2D goalCompletedAudioPlayer;

	const string SAVE_FILE_LOCATION = "user://goals.json";
	static Dictionary<string, Array<string>> data = []; // Key is day, Value is goals

	
	static private void UpdateCheckBox(CheckBox checkBox, string goal) => checkBox.Text = $"{goalDescription[goal]} ({GetStreak(goal)})"; 
    static DateTime CurrentDate => DateTime.Now;
    static string DateToString(DateTime dateTime) => $"{dateTime.Day}/{dateTime.Month}/{dateTime.Year}";

	public override void _Ready() {
		return;

		data = GetData();
        AddTodayToStreak();

		foreach (string goal in goals) { 
			CheckBox duplicate = prototype.Duplicate() as CheckBox;

			goalsContainer.AddChild(duplicate);

			UpdateCheckBox(duplicate, goal);

			duplicate.Visible = true;
			duplicate.ButtonPressed = data[DateToString(CurrentDate)].Contains(goal);
			duplicate.Toggled += (toggled) => {
                GoalAccomplished(goal, toggled);
				duplicate.GetChild<GpuParticles2D>(0).Emitting = toggled; 
				UpdateCheckBox(duplicate, goal);
			};
		}
	}

	private static void AddTodayToStreak() {
		string date = DateToString(CurrentDate);
		if (!data.ContainsKey(date)) data.Add(date, new());
	}

	void GoalAccomplished(string goalName, bool toggle) {
		string date = DateToString(CurrentDate);

		if (toggle is false) {
			data[date].Remove(goalName);
		} else {
			if (data.ContainsKey(date) && !data[date].Contains(goalName)) data[date].Add(goalName);
			else data.Add(date, new() {goalName});
			goalCompletedAudioPlayer.Play(0.1f);
		}

        Save();
	}

	// Whenever i click, streak goes up;
	// Streak is calculated on READY and it's just the # of times 
	static void Save() {
		using FileAccess saveFile = FileAccess.Open(SAVE_FILE_LOCATION, FileAccess.ModeFlags.Write);

		var stringifiedData = Json.Stringify(data, sortKeys: false);
		saveFile.StoreLine(stringifiedData);
	}    
	
	static int GetStreak(string goal) {
		int streak = 0;
		var keys = data.Keys.ToArray();

		int size = keys.Length - 1;

		// They made C# python and I love it
		// Maybe not the caret for "reverse" though; seems kinda weird.
		if (keys[^1] == DateToString(CurrentDate) && !data[keys[^1]].Contains(goal)) size--;

		// Count down from the start
		for (int i = size; i >= 0; i--) {
			// Keep counting down until we cannot find the goal any more.
			if (data[keys[i]].IndexOf(goal) != -1) streak++;
			else return streak;
		}

		return streak;
	}

	static Dictionary<string, Array<string>> GetData() {
		using FileAccess saveFile = FileAccess.Open(SAVE_FILE_LOCATION, FileAccess.ModeFlags.Read);
		string data = saveFile.GetLine();

		if (string.IsNullOrEmpty(data)) return new();
		
		Json json = new();

		if (json.Parse(data) != Error.Ok) GD.PushError($"JSON Parse Error: {json.GetErrorMessage()} in {data} at line {json.GetErrorLine()}");

		return new((Dictionary) json.Data);
	}

}
