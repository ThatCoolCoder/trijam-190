using Godot;
using System;

public class StartScreen : Control
{
	private void OnPlayButtonPressed()
	{
		LevelManager.LoadLevel(1, GetTree());
	}
}
