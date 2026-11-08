// Frame timing, collected by the app every frame and shown on the dashboard.
public class Stats
{
    public const int History = 120;

    public readonly float[] FrameMs = new float[History];
    public int Head;
    public long Frames;
    public float Fps = 60;
    public float WorstMs;
    public double UptimeSec;

    private float _windowMs;
    private int _windowFrames;
    private float _windowWorst;

    public void AddFrame(float ms)
    {
        FrameMs[Head] = ms;
        Head = (Head + 1) % History;
        Frames++;
        UptimeSec += ms / 1000.0;

        _windowMs += ms;
        _windowFrames++;
        if (ms > _windowWorst) _windowWorst = ms;
        if (_windowMs >= 500)
        {
            Fps = _windowFrames * 1000f / _windowMs;
            WorstMs = _windowWorst;
            _windowMs = 0;
            _windowFrames = 0;
            _windowWorst = 0;
        }
    }

    public float AverageMs()
    {
        float sum = 0;
        for (int i = 0; i < History; i++) sum += FrameMs[i];
        return sum / History;
    }
}
