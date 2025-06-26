using Godot;
using System;

public partial class Menu : Control
{
	public override void _Ready()
	{

	}
	private void _on_play_button_button_down()
	{
		var scene = ResourceLoader.Load<PackedScene>("res://russian_roulette.tscn");
		GetTree().ChangeSceneToPacked(scene);
	}
	private void _on_quit_button_button_down()
	{
		GetTree().Quit();
	}
}
