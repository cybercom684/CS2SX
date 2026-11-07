// A user file that happens to share its name with an API type's old stub file.
// It used to be dropped silently by name-based exclusion lists.
public class InputMapper
{
    public int Presses;

    public void Poll()
    {
        if (Input.IsDown(NpadButton.A)) Presses++;
    }
}
