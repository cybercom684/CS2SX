public class SwitchApisApp : SwitchApp
{
    private int _x = 100;

    public override void OnInit()
    {
        Graphics.Init(1280, 720);
        int r = Ftp.Start(5000);
        Console.WriteLine("FTP " + Ftp.GetIP() + ":" + Ftp.GetPort() + " r=" + r + " running=" + Ftp.IsRunning());
        if (Nsp.Open("/x.nsp") == 1)
            Console.WriteLine(Nsp.TitleId() + " " + Nsp.TotalSize() + " " + Nsp.FileCount() + " " + Nsp.FileName(0));
        if (Usb.Init() == 0)
            Console.WriteLine(Usb.Count() + " " + Usb.Name(0) + " " + Usb.Capacity(0));
    }

    public override void OnFrame()
    {
        if (Input.IsHeld(NpadButton.Right)) _x += 4;
        if (Input.IsHeld(NpadButton.Left)) _x -= 4;
        if (Input.IsDown(NpadButton.A)) App.Quit();
        if (Input.IsDown(NpadButton.B)) App.LaunchNro("/switch/hbmenu.nro");
        if (Input.IsDown(NpadButton.X)) Ftp.Stop();

        Graphics.BeginFrame();
        Graphics.FillScreen(Color.Black);
        Graphics.FillRect(_x, 300, 80, 80, Color.Orange);
        Graphics.DrawCircle(640, 360, 50, Color.Cyan);
        Graphics.DrawText(20, 20, "x=" + _x, Color.White, 2);
        Graphics.EndFrame();
    }
}
