using Godot;
using System;
public class Soldier : Node2D
{
    public int state = 0;
    public bool right = true;
    public int down_frame = 8;
    [Export]
    public int right_frame = 6;
    [Export]
    public int level = 15;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var sprite = GetNode<Sprite>("Position2D/Sprite");
        sprite.Frame = 0;
    }

    public void FlipSoldier()
    {
        var sprite = GetNode<Sprite>("Position2D/Sprite");
        sprite.Frame = 6;
        sprite.FlipH = !sprite.FlipH;
        right = !right;
        string dir = (right) ? "R" : "L";
        GD.Print("Soldier Clicked!!!");
        GD.Print("Current Direction " + dir);
    }

    public void OnButtonPressed()
    {
        if (state == 0)
        {
            FlipSoldier();
        }
    }
}
