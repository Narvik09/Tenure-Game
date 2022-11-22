using Godot;
using System;

public class StartScreen : CanvasLayer
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {

    }

    public void OnStartButtonPressed()
    {
        GetNode<Button>("StartButton").Hide();
        EmitSignal("StartGame");

    }

    public void OnAttackerStartButtonDown()
    {
        GetTree().ChangeScene("res://Main.tscn");
        GD.Print("Attacker pressed!!!");

    }

    public void OnDefenderStartButtonDown()
    {
        GetTree().ChangeScene("res://Main.tscn");
        GD.Print("Defender pressed!!!");
    }

    public void OnMultiplayerStartButtonDown()
    {
        GetTree().ChangeScene("res://NetworkSetup.tscn");
        GD.Print("Multiplayer pressed!!!");
    }
}
