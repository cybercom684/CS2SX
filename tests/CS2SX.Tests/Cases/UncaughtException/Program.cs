// An uncaught exception ends the app with the message and the C# call trace
// (in the system error dialog and the debug log).
public class Enemy
{
    public Enemy? Target;
    public int Hp = 3;

    public void Hit()
    {
        Target!.Hp--;
    }
}

public class UncaughtExceptionApp : SwitchApp
{
    private readonly Enemy _enemy = new Enemy();
    private int _frame;

    public override void OnFrame()
    {
        _frame++;
        if (_frame == 3)
            _enemy.Hit();
    }
}
