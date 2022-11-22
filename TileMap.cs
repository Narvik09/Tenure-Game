using Godot;
using System;

public class TileMap : Godot.TileMap
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    TileSet _tileset;
    [Export]
    public int Rows = 10;
    [Export]
    public int Columns = 10;
    // public RandomNumberGenerator rng;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        _tileset = GetTileset();
        int temp_x = Rows / 2;
        int temp_y = Columns / 2;
        for (int i = -temp_x; i < temp_x; i++)
        {
            for (int j = -temp_y; j < temp_y; j++)
            {
                SetCell(i, j, TileSet.FindTileByName("Grass_8"));
            }
        }
        for (int i = -temp_x; i < temp_x; i++)
        {
            if (i != -1 && i != 0)
            {
                SetCell(i, -temp_y - 2, TileSet.FindTileByName("Bottom_wall"));
            }
            SetCell(i, temp_y, TileSet.FindTileByName("Side_wall"), transpose: true);
        }
        for (int j = -temp_y; j < temp_y; j++)
        {
            SetCell(-temp_x - 1, j, TileSet.FindTileByName("Side_wall"), flipX: true);
            SetCell(temp_x, j, TileSet.FindTileByName("Side_wall"));
        }
        SetCell(-temp_x - 1, -temp_y - 2, TileSet.FindTileByName("Bottom_corner"), flipX: true);
        SetCell(temp_x - 1, -temp_y - 2, TileSet.FindTileByName("Bottom_corner"));
        SetCell(-temp_x - 1, temp_y, TileSet.FindTileByName("Corner"), flipX: true);
        SetCell(temp_x, temp_y, TileSet.FindTileByName("Corner"));
        SetCell(-1, -temp_y - 2, TileSet.FindTileByName("Gate"));

    }
    //  // Called every frame. 'delta' is the elapsed time since the previous frame.
    //  public override void _Process(float delta)
    //  {
    //      
    //  }
}
