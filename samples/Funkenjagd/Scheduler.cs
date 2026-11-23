using System.Collections.Generic;

// Coroutines: a routine is a method returning IEnumerator<int>. Each value it
// yields is the number of frames to wait before it goes on:
//
//   IEnumerator<int> Blink() { Show(); yield return 30; Hide(); }
//
// CS2SX turns such methods into small state machines, so a routine keeps its
// local variables between frames, like in .NET.

public class Routine
{
    public IEnumerator<int> Steps;
    public int Wait;

    public Routine(IEnumerator<int> steps)
    {
        Steps = steps;
    }
}

public class Scheduler
{
    private readonly List<Routine> _routines = new List<Routine>();

    public int Count => _routines.Count;

    public void Start(IEnumerator<int> steps)
    {
        _routines.Add(new Routine(steps));
    }

    public void StopAll()
    {
        _routines.Clear();
    }

    /// One frame: every routine that is not waiting runs to its next yield.
    /// (A routine may start others; they are added at the end and run next frame.)
    public void Tick()
    {
        int count = _routines.Count;
        int i = 0;
        while (i < count)
        {
            var r = _routines[i];
            if (r.Wait > 0)
            {
                r.Wait--;
                i++;
            }
            else if (r.Steps.MoveNext())
            {
                r.Wait = r.Steps.Current;
                i++;
            }
            else
            {
                _routines.RemoveAt(i);
                count--;
            }
        }
    }
}
