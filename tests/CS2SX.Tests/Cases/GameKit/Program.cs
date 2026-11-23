using System;
using System.Collections.Generic;

// Game helpers: sprite-sheet frames, turned / mirrored / tinted / faded
// sprites, binary files (File.ReadAllBytes, BinaryReader / BinaryWriter,
// MemoryStream), several players, SaveData in the app's own folder, and a
// property with its own setter. Expected output in comments.
public class Score
{
    private int _points;
    public int Changes;
    public int Points
    {
        get => _points;
        set { _points = value < 0 ? 0 : value; Changes++; }
    }
}

public class GameKitApp : SwitchApp
{
    private Texture? _sheet;
    private int _frame;

    public override void OnInit()
    {
        // Binary files
        var path = "/switch/cs2sx_gamekit.dat";
        using (var w = new BinaryWriter(File.Create(path)))
        {
            w.Write(42); w.Write(true); w.Write(1.5f); w.Write("hallo"); w.Write((byte)7); w.Write(123456789012L);
            w.Write(new byte[] { 1, 2, 3 });
        }
        Console.WriteLine("size " + File.ReadAllBytes(path).Length);                       // size 27
        using (var r = new BinaryReader(File.OpenRead(path)))
        {
            Console.WriteLine(r.ReadInt32() + " " + r.ReadBoolean() + " " + r.ReadSingle() + " "
                + r.ReadString() + " " + r.ReadByte() + " " + r.ReadInt64());             // 42 True 1.5 hallo 7 123456789012
            byte[] rest = r.ReadBytes(10);
            Console.WriteLine("rest " + rest.Length + " " + rest[2] + " " + r.BaseStream.Position);   // rest 3 3 27
        }
        var ms = new MemoryStream();
        var mw = new BinaryWriter(ms);
        mw.Write((short)-2); mw.Write(3.25);
        var mr = new BinaryReader(new MemoryStream(ms.ToArray()));
        Console.WriteLine("mem " + mr.ReadInt16() + " " + mr.ReadDouble());                 // mem -2 3.25
        mr.BaseStream.Position = 0;
        Console.WriteLine("again " + mr.ReadInt16());                                        // again -2
        File.WriteAllBytes(path, new byte[] { 9, 8 });
        File.AppendAllBytes(path, new byte[] { 7 });
        Console.WriteLine("bytes " + File.ReadAllBytes(path).Length);                       // bytes 3
        try { File.ReadAllBytes("/switch/cs2sx_missing.bin"); }
        catch (FileNotFoundException) { Console.WriteLine("missing ok"); }                  // missing ok

        // SaveData: the app's own folder, values of any length
        SaveData.Write("note", "zeile 1\nzeile 2");
        Console.WriteLine("save " + SaveData.Read("note").Length + " " + !File.Exists("/switch/cs2sx_data/note.txt"));   // save 15 True

        // A property with its own setter
        var score = new Score();
        score.Points = 5;
        score.Points += 3;
        score.Points = -4;
        Console.WriteLine("points " + score.Points + " " + score.Changes);                  // points 0 3

        // Players (no controllers in the emulator test besides player 1 / handheld)
        Console.WriteLine("players " + (Input.ConnectedCount() >= 0) + " " + Input.IsDown(0, NpadButton.A)
            + " " + Input.GetStickLeft(1).x);                                               // players True False 0

        // A 4×2 sprite sheet of 2×2 frames
        var pixels = new uint[8 * 4];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.RGB((byte)(i * 8), 100, 200);
        _sheet = new Texture(8, 4, pixels);
        Graphics.Init(1280, 720);
    }

    public override void OnFrame()
    {
        _frame++;
        Graphics.FillScreen(Color.RGB(20, 20, 30));
        int f = _frame / 10 % 4;
        Graphics.DrawTextureRegion(_sheet!, f * 2, 0, 2, 2, 100, 100, 64, 64);
        Graphics.DrawTextureRegion(_sheet!, f * 2, 2, 2, 2, 200, 100, 64, 64, _frame * 3, Color.RGBA(255, 255, 255, 160), true);
        Graphics.DrawTextureEx(_sheet!, 300, 100, 128, 64, -15, Color.RGB(255, 120, 120), false, true);
        Graphics.DrawTextureAlpha(_sheet!, 460, 100, 90);
        if (_frame == 5) Console.WriteLine("done");                                        // done
    }
}
