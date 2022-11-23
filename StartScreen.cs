using Godot;
using System;

public class StartScreen : CanvasLayer
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    // Called when the node enters the scene tree for the first time.
    [Export]
    public PackedScene Main;
    
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
        GD.Print("Attacker pressed!!!");
        var main = (PackedScene)ResourceLoader.Load("res://Main.tscn");
        var inst = (Main)main.Instance();
        inst.type = 1;
        AddChild(inst);
    }

    public void OnDefenderStartButtonDown()
    {
        GD.Print("Defender pressed!!!");
        var main = (PackedScene)ResourceLoader.Load("res://Main.tscn");
        var inst = (Main)main.Instance();
        inst.type = 2;
        AddChild(inst);
    }

    public void OnMultiplayerStartButtonDown()
    {
        GD.Print("Multiplayer pressed!!!");
        var main = (PackedScene)ResourceLoader.Load("res://Main.tscn");
        var inst = (Main)main.Instance();
        inst.type = 3;
        AddChild(inst);
    }
}
