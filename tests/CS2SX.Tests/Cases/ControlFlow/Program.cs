public class ControlFlowApp : SwitchApp
{
    public override void OnInit()
    {
        int sum = 0;
        for (int i = 0; i < 20; i++)
        {
            if (i == 3) continue;
            if (i > 10) break;
            sum += i;
        }

        int n = 5;
        while (n > 0) n--;

        int k = 0;
        do { k += 2; } while (k < 7);

        int? maybe = null;
        int value = maybe ?? -1;
        maybe = 4;
        if (maybe.HasValue) value += maybe.Value;

        string text = null;
        string shown = text ?? "default";

        bool flag = sum > 10 && !(k == 0) || value < 0;
        Console.WriteLine(sum + " " + n + " " + k + " " + value + " " + shown + " " + flag);

        int x = 0;
        x += 5; x -= 1; x *= 3; x /= 2; x %= 5;
        x <<= 2; x >>= 1; x |= 1; x &= 7; x ^= 2;
        Console.WriteLine(x);
    }
}
