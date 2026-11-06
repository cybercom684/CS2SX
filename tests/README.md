# CS2SX Tests

```bash
dotnet test
```

Each folder in `CS2SX.Tests/Cases/` is a small Switch program. Every case runs through:

| Test | What it checks | Needs devkitPro |
|---|---|---|
| `TranspileTests` | `cs2sx build --transpile-only` succeeds; generated C matches `__snapshots__/` | no |
| `CompileTests` | full build through GCC to a `.nro` | yes (skipped otherwise) |
| `DiagnosticsTests` | `cs2sx check` reports every line of `expect-warnings.txt` | no |

The CLI runs out-of-process per case (the transpiler keeps global static state).

## Snapshots

Snapshots are the approved generated C. After an intended codegen change:

```bash
CS2SX_UPDATE_SNAPSHOTS=1 dotnet test
```

then review the snapshot diff like any other change. A new case without snapshots
fails once after writing them, so nothing passes unreviewed.

## Known issues

`known-issue.txt` in a case marks it as expected to **fail** the GCC build and says why.
When a fix makes it build, `CompileTests` fails with a reminder to delete the marker.
These markers are the live bug backlog of the transpiler.

## Adding a case

Create `Cases/<Name>/Program.cs` with a class `<Name>App : SwitchApp` (more `.cs`/`.h`
files and a `cs2sx.json` are optional), run `dotnet test`, review the new snapshots.
