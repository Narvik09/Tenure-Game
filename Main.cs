using Godot;
using System;
using System.Collections.Generic;

public class Main : Node2D
{

#pragma warning disable 649
    [Export]
    public PackedScene SoldierScene;
    [Export]
    public PackedScene TileMapScene;
#pragma warning restore 649

    [Export]
    public int maxCount = 15;

    public int aliveSoldiers = 15;
    public int maxLevel = 15;
    public RandomNumberGenerator rng;
    bool killLeft = true;

    [Export]
    bool defenderComputer = true;

    [Export]
    bool attackerComputer = false;

    public int type = 0;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Randomize();
        GetNode<Label>("GameOver").Hide();
        GetNode<Label>("GameInst").Hide();
        Start();
    }

    public void Start()
    {
        var tile = (TileMap)TileMapScene.Instance();
        maxLevel = tile.Columns;
        // bool[,] taken = new bool[tile.Rows * 18, tile.Columns * 18];
        Dictionary<Tuple<int, int>, bool> taken = new Dictionary<Tuple<int, int>, bool>();
        GD.Print(tile.Rows, tile.Columns);
        int curCount = 0;
        while (curCount < maxCount)
        {
            // updated coordinates based on new tile maps
            int x = rng.RandiRange(-tile.Rows / 2, tile.Rows / 2 - 1) * 64;
            int y = rng.RandiRange(-tile.Columns / 2, tile.Columns / 2 - 1) * 64;
            Tuple<int, int> temp = Tuple.Create(x, y);
            bool returnValue;
            taken.TryGetValue(temp, out returnValue);
            if (!returnValue)
            {
                var soldier = (Soldier)SoldierScene.Instance();
                soldier.Position = new Vector2(x, y - 4);
                // check this and fix the levels
                // 64 is one step for the soldier. 
                soldier.level = (tile.Columns / 2 - y / 64);
                GD.Print(soldier.Position);
                soldier.AddToGroup("soldiers");
                // soldier.Hide();
                AddChild(soldier);
                taken[temp] = true;
                curCount++;
            }
        }
        GD.Print($"LMAO : {type}");
        // player chooses attacker
        if (type == 1)
        {
            attackerComputer = false;
            defenderComputer = true;
        }
        // player chooses defender
        else if (type == 2)
        {
            attackerComputer = true;
            defenderComputer = false;
        }
        else
        {
            // multiplayer, attacker starts
        }
        StartP1Turn();
    }

    // Computes the optimal way of partitioning the soldiers.
    public void PartitionSoldiers()
    {
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        List<Tuple<long, Soldier>> weightPairs = new List<Tuple<long, Soldier>>();
        foreach (Soldier soldier in soldiers)
        {
            weightPairs.Add(Tuple.Create((long)(1 << soldier.level), soldier));
        }
        weightPairs.Sort((a, b) => a.Item1.CompareTo(b.Item1));
        weightPairs.Reverse();
        long sumLeft = 0, sumRight = 0;
        foreach (Tuple<long, Soldier> weightPair in weightPairs)
        {
            Soldier curSoldier = weightPair.Item2;
            if (sumLeft <= sumRight)
            {
                sumLeft += weightPair.Item1;
                if (curSoldier.right)
                {
                    GD.Print("I am flipping");
                    curSoldier.FlipSoldier();
                }
            }
            else
            {
                sumRight += weightPair.Item1;
                if (!curSoldier.right)
                {
                    curSoldier.FlipSoldier();
                    //    curSoldier.FlipSoldier();
                }
            }
        }
    }

    public void StartP1Turn()
    {
        GetNode<Button>("P1Done").Hide();
        GetNode<Button>("P2Left").Hide();
        GetNode<Button>("P2Right").Hide();
        GetNode<Button>("P2Done").Hide();
        GetNode<CheckButton>("CheckButton").Hide();
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        foreach (Soldier soldier in soldiers)
        {
            soldier.state = 0;
            soldier._Ready();
        }
        if (attackerComputer)
        {
            PartitionSoldiers();
            OnP1DoneButtonDown();
        }
        GetNode<Button>("P1Done").Show();
        // GetNode<CheckButton>("CheckButton").Show();
        // instructions displayed during the game
        var message = GetNode<Label>("GameInst");
        // message.Text = "Attacker's turn. Click on the players to make them face either left or right. The defender will remove the set facing left or right. Choose wisely!!!";
        message.Show();
    }

    public void OnP1DoneButtonDown()
    {
        // Get the number of soldiers in each direction. 
        int cntLeft = 0, cntRight = 0;
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        foreach (Soldier soldier in soldiers)
        {
            // flipping all the unshoosen players to the right
            var sprite = soldier.GetNode<Sprite>("Position2D/Sprite");
            if (soldier.right)
            {
                sprite.Frame = 6;
                cntRight++;
            }
            else
            {
                cntLeft++;
                if (sprite.Frame == 0)
                {
                    sprite.Frame = 6;
                    sprite.FlipH = !sprite.FlipH;
                }
            }
            soldier.state = 1;
        }
        GD.Print("Number of left facing: " + cntLeft);
        GD.Print("Number of right facing: " + cntRight);
        if (defenderComputer)
        {
            // Choose the larger weighted partition.
            long leftSum = 0, rightSum = 0;
            foreach (Soldier soldier in soldiers)
            {
                if (!soldier.right)
                {
                    leftSum += (1 << soldier.level);
                }
                else
                {
                    rightSum += (1 << soldier.level);
                }
            }
            if (leftSum >= rightSum)
            {
                killLeft = true;
            }
            else
            {
                killLeft = false;
            }
            OnP2DoneButtonDown();
            return;
        }
        GetNode<Button>("P1Done").Hide();
        GetNode<Button>("P2Left").Show();
        GetNode<Button>("P2Right").Show();
        GetNode<Button>("P2Done").Show();
        // GetNode<CheckButton>("CheckButton").Hide();
        var message = GetNode<Label>("GameInst");
        // message.Text = "Defender's Turn. Choose the set of players facing left or right to remove them. The remaining players will advance forward by one step. All the best!";
        message.Show();
    }

    public void OnP2LeftButtonDown()
    {
        killLeft = true;
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        // coloring the players belonging to a set when button is pressed. 
        foreach (Soldier soldier in soldiers)
        {
            if (soldier.right == !killLeft)
            {
                soldier.GetNode<Sprite>("Position2D/Sprite").SelfModulate = new Color("#5ac3f1");
            }
            else
            {
                soldier.GetNode<Sprite>("Position2D/Sprite").SelfModulate = new Color(1, 1, 1);
            }
        }
        var message = GetNode<Label>("GameInst");
        message.Text = "Are you sure? If so, press the 'Done' button!";
        message.Show();
    }

    public void OnP2RightButtonDown()
    {
        killLeft = false;
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        // coloring the players belonging to a set when button is pressed. 
        foreach (Soldier soldier in soldiers)
        {
            if (soldier.right == !killLeft)
            {
                soldier.GetNode<Sprite>("Position2D/Sprite").SelfModulate = new Color("#d75af1");
            }
            else
            {
                soldier.GetNode<Sprite>("Position2D/Sprite").SelfModulate = new Color(1, 1, 1);

            }
        }
        var message = GetNode<Label>("GameInst");
        message.Text = "Are you sure? If so, press the 'Done' button!";
        message.Show();
    }

    public async void OnP2DoneButtonDown()
    {
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        foreach (Soldier soldier in soldiers)
        {
            bool destroy = false;
            if (soldier.right == !killLeft)
            {
                destroy = true;
            }
            if (destroy)
            {
                soldier.RemoveFromGroup("soldier");
                RemoveChild(soldier);
                soldier.QueueFree();
                aliveSoldiers--;
                if (aliveSoldiers == 0)
                {
                    GD.Print("Defender wins!");
                    var label = GetNode<Label>("GameOver");
                    label.Text = "Game Over!\nDefender Wins!!!";
                    label.Show();
                    GetNode<Button>("P1Done").Hide();
                    GetNode<Button>("P2Left").Hide();
                    GetNode<Button>("P2Right").Hide();
                    GetNode<Button>("P2Done").Hide();
                    GetNode<Label>("GameInst").Hide();
                    // add a timer
                    await ToSignal(GetTree().CreateTimer(2), "timeout");
                    GetTree().ChangeScene("res://StartScreen.tscn");
                    return;
                }
            }
            else
            {
                soldier.level++;
                if (soldier.level == maxLevel)
                {
                    GD.Print("Attacker wins!");
                    var label = GetNode<Label>("GameOver");
                    label.Text = "Game Over!\nAttacker Wins!!!";
                    label.Show();
                    // do something so that soldier does not hit the wall
                    // level calculation must be corrected
                    GetNode<Button>("P1Done").Hide();
                    GetNode<Button>("P2Left").Hide();
                    GetNode<Button>("P2Right").Hide();
                    GetNode<Button>("P2Done").Hide();
                    GetNode<Label>("GameInst").Hide();
                    // add a timer 
                    await ToSignal(GetTree().CreateTimer(2), "timeout");
                    GetTree().ChangeScene("res://StartScreen.tscn");
                    return;
                }
                Vector2 targetPos = soldier.Position;
                targetPos.y = targetPos.y - 64;
                Tween tween = soldier.GetNode<Tween>("Tween");
                AnimationPlayer animationPlayer = soldier.GetNode<AnimationPlayer>("AnimationPlayer");
                animationPlayer.Play("walk_up");

                tween.InterpolateProperty(soldier, "position", soldier.Position, targetPos, animationPlayer.CurrentAnimationLength);
                tween.Start();
                soldier.Position = targetPos;
            }
        }
        var message = GetNode<Label>("GameInst");
        message.Text = "The players are moving forward. Wait for you turn...";
        message.Show();
        StartP1Turn();
    }

    public void OnCheckButtonPressed()
    {
        defenderComputer = !defenderComputer;
        // GetNode<Button>("P1Done").Hide();
    }
}
