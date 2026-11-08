# CS2SX Lab

Interaktive Demo-App für echte Hardware: vier Seiten, gewechselt mit L/R oder per Tippen auf die Reiter.

| Seite | Inhalt |
|---|---|
| Übersicht | Bildrate mit Frame-Time-Graph, Akku, Uhr, Laufzeit, Systeminfo |
| Controller | Sticks mit Spur, alle Tasten, Touch-Punkte, Tastendrücke pro Sekunde |
| Reaktion | Reaktionstest mit Vibration und Ton; Bestwert, Schnitt, Median, Verlauf |
| Bewegung | Wasserwaage, Beschleunigungs- und Gyro-Werte, Drehgeschwindigkeits-Verlauf |

## Bauen

```bash
dotnet pack src/CS2SX.Switch -o nupkg        # einmalig, falls das Paket fehlt
cs2sx build samples/LabDemo/LabDemo.csproj
```

Die App nutzt die Systemschrift der Switch. Liegt eine `romfs/font.ttf` im Projekt,
wird stattdessen sie verwendet (nötig in Emulatoren ohne Firmware-Schriften).
