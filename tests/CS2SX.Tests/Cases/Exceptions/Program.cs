using System;

public class ExceptionsApp : SwitchApp
{
    private static int Divide(int a, int b)
    {
        if (b == 0) throw new Exception("division by zero");
        return a / b;
    }

    public override void OnInit()
    {
        try
        {
            Console.WriteLine(Divide(10, 2));
            Console.WriteLine(Divide(1, 0));
        }
        catch (Exception ex)
        {
            Console.WriteLine("caught: " + ex.Message);
        }
        finally
        {
            Console.WriteLine("finally");
        }
    }
}
