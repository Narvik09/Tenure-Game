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
    public int maxCount = 10;

    public int aliveSoldiers = 10;
    public int maxLevel = 15;
    public RandomNumberGenerator rng;
    bool killLeft = true;

    [Export]
    bool defenderComputer = true;

    [Export]
    bool attackerComputer = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Randomize();
        GetNode<Label>("GameOver").Hide();
        Start();
    }

    public void NewGame()
    {
        var hud = GetNode<HUD>("HUD");
        hud.ShowMessage("Get Ready!");
    }

    public void GameOver()
    {
        GetNode<HUD>("HUD").ShowGameOver();
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
            int x = rng.RandiRange(-tile.Rows / 2, tile.Rows / 2) * 18;
            int y = rng.RandiRange(-tile.Columns / 2 + 2, tile.Columns / 2) * 18;
            Tuple<int, int> temp = Tuple.Create(x, y);
            bool returnValue;
            taken.TryGetValue(temp, out returnValue);
            if (!returnValue)
            {
                var soldier = (Soldier)SoldierScene.Instance();
                soldier.Position = new Vector2(x, y);
                soldier.level = (tile.Columns / 2 - y / 18);
                GD.Print(soldier.Position);
                soldier.AddToGroup("soldiers");
                AddChild(soldier);
                taken[temp] = true;
                curCount++;
            }
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
                    curSoldier.FlipSoldier();
                }
            }
            else
            {
                sumRight += weightPair.Item1;
                if (!curSoldier.right)
                {
                    curSoldier.FlipSoldier();
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
        if (attackerComputer)
        {
            PartitionSoldiers();
            OnP1DoneButtonDown();
        }
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        foreach (Soldier soldier in soldiers)
        {
            soldier.state = 0;
            soldier._Ready();
        }
        GetNode<Button>("P1Done").Show();
        GetNode<CheckButton>("CheckButton").Show();
    }

    public void OnP1DoneButtonDown()
    {
        // Get the number of soldiers in each direction. 
        int cntLeft = 0, cntRight = 0;
        var soldiers = GetTree().GetNodesInGroup("soldiers");
        foreach (Soldier soldier in soldiers)
        {
            if (soldier.right)
            {
                cntRight++;
            }
            else
            {
                cntLeft++;
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
        GetNode<CheckButton>("CheckButton").Hide();
    }

    public void OnP2LeftButtonDown()
    {
        killLeft = true;
    }

    public void OnP2RightButtonDown()
    {
        killLeft = false;
    }

    async public void OnP2DoneButtonDown()
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
                    return;
                }
            }
            else
            {
                soldier.level++;
                Vector2 targetPos = soldier.Position;
                targetPos.y = targetPos.y - 18;
                Tween tween = soldier.GetNode<Tween>("Tween");
                AnimationPlayer animationPlayer = soldier.GetNode<AnimationPlayer>("AnimationPlayer");
                animationPlayer.Play("walk_up");

                tween.InterpolateProperty(soldier, "position", soldier.Position, targetPos, animationPlayer.CurrentAnimationLength);
                tween.Start();
                // problems : 
                // playing player animation
                // double click on done messes the position
                // at the end, only few players move. 
                await ToSignal(animationPlayer, "animation_finished");
                // animationPlayer.Stop(reset: true);
                soldier.Position = targetPos;
                if (soldier.level == maxLevel)
                {
                    GD.Print("Attacker wins!");
                    var label = GetNode<Label>("GameOver");
                    label.Text = "Game Over!\nAttacker Wins!!!";
                    label.Show();
                    GetNode<Button>("P1Done").Hide();
                    GetNode<Button>("P2Left").Hide();
                    GetNode<Button>("P2Right").Hide();
                    GetNode<Button>("P2Done").Hide();
                    return;
                }
            }
        }
        StartP1Turn();
    }

    public void OnCheckButtonPressed()
    {
        defenderComputer = !defenderComputer;
        // GetNode<Button>("P1Done").Hide();
    }
}
