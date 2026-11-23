using System;
using System.Collections.Generic;

// The things on the playfield.

public class Player
{
    public int Index;            // controller / player number 0…3
    public string Name;
    public uint Tint;
    public float X, Y, Vx, Vy;
    public int Points;
    public int Lives = 3;
    public int Shield;           // frames of protection after a hit
    public bool Out => Lives <= 0;

    public Player(int index, uint tint, float x, float y)
    {
        Index = index;
        Name = "Spieler " + (index + 1);
        Tint = tint;
        X = x;
        Y = y;
    }
}

public class Spark
{
    public float X, Y;
    public int Age;
    public bool Golden;          // worth 5
}

public class Rock
{
    public float X, Y, Vx, Vy, Angle, Spin;
    public int Size;
}

public class Particle
{
    public float X, Y, Vx, Vy;
    public int Life;
    public uint Color;
}

/// Small helpers for the playfield.
public static class Field
{
    public const int Top = 80, Bottom = 720, Left = 0, Right = 1280;

    public static float Distance(float ax, float ay, float bx, float by)
    {
        float dx = ax - bx, dy = ay - by;
        return (float)Math.Sqrt(dx * dx + dy * dy);
    }

    public static float Clamp(float v, float lo, float hi)
    {
        return v < lo ? lo : v > hi ? hi : v;
    }
}
