public abstract class Shape
{
    public string Name { get; set; }
    public static int Created;

    public Shape(string name)
    {
        Name = name;
        Created++;
    }

    public abstract float Area();

    public virtual string Describe()
    {
        return Name + " with area " + Area();
    }
}

public class Rect : Shape
{
    public float W;
    public float H;

    public Rect(float w, float h) : base("Rect")
    {
        W = w;
        H = h;
    }

    public override float Area() => W * H;
}

public class Square : Rect
{
    public Square(float side) : base(side, side)
    {
        Name = "Square";
    }

    public override string Describe()
    {
        return "[" + base.Describe() + "]";
    }
}

public class Circle : Shape
{
    private readonly float _r;

    public Circle(float r) : base("Circle")
    {
        _r = r;
    }

    public override float Area() => 3.14159f * _r * _r;
}

public class ClassesInheritanceApp : SwitchApp
{
    public override void OnInit()
    {
        Shape a = new Rect(2, 3);
        Shape b = new Circle(1);
        Shape c = new Square(4);
        Console.WriteLine(a.Describe());
        Console.WriteLine(b.Describe());
        Console.WriteLine(c.Describe());
        Console.WriteLine("created: " + Shape.Created);
    }
}
