using System;
using System.Collections.Generic;

// `is`, type patterns and switch on the type check the run-time type: a Square
// is a Shape but not a Circle; an interface value knows its object's class;
// exceptions match by their type. Expected output in comments.
public interface IShape { string Kind(); }

public abstract class Shape : IShape
{
    public abstract string Kind();
}

public class Circle : Shape
{
    public int R = 2;
    public override string Kind() => "circle";
}

public class Square : Shape
{
    public int Side = 3;
    public override string Kind() => "square";
}

public class Cube : Square
{
    public override string Kind() => "cube";
}

public class TypeTestsApp : SwitchApp
{
    private static string Describe(Shape s)
    {
        if (s is Circle c) return "circle r" + c.R;
        if (s is Square q) return (s is Cube ? "cube" : "square") + " side" + q.Side;
        return "?";
    }

    private static string Switch(Shape s)
    {
        switch (s)
        {
            case Cube: return "C";
            case Square sq: return "S" + sq.Side;
            case Circle: return "O";
            default: return "-";
        }
    }

    public override void OnInit()
    {
        var shapes = new List<Shape> { new Circle(), new Square(), new Cube() };
        foreach (var s in shapes)
            Console.WriteLine(Describe(s) + " " + Switch(s) + " " + (s is Square));   // circle r2 O False / square side3 S3 True / cube side3 C True

        IShape i = new Cube();
        Console.WriteLine((i is Square) + " " + (i is Circle));                       // True False
        if (i is Square sq) Console.WriteLine("side " + sq.Side + " " + sq.Kind());  // side 3 cube

        Exception e = new ArgumentNullException("p");
        Console.WriteLine((e is ArgumentException) + " " + (e is InvalidOperationException)); // True False
        Shape? none = null;
        Console.WriteLine((none is Circle) + " done");                                // False done
    }
}
