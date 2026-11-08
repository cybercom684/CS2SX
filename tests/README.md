# CS2SX Tests

```bash
dotnet test
```

Each folder in `CS2SX.Tests/Cases/` is a small Switch program. Every case runs through:

| Test | What it checks | Needs devkitPro |
|---|---|---|
| `TranspileTests` | `cs2sx build --transpile-only` succeeds; generated C matches `__snapshots__/` | no |
| `CompileTests` | full build through GCC to a `.nro` | yes (skipped otherwise) |
| `DiagnosticsTests` | `cs2sx check` reports every line of `expect-warnings.txt` / `expect-errors.txt` | no |
| `RuntimeTests` | the `.nro` runs in Ryujinx and prints the lines of `expect-output.txt` | yes, plus Ryujinx |

The CLI runs out-of-process per case (the transpiler keeps global static state).

A case with `expect-errors.txt` must be **rejected**: build and check fail with those
errors, and GCC never runs.

## Runtime tests

`RuntimeTests` run each case that has an `expect-output.txt` in Ryujinx (headless) and
compare the first lines it prints with that file. They need Ryujinx 1.3 or newer:

```bash
CS2SX_RYUJINX="C:/path/to/Ryujinx.exe" dotnet test
```

Without `CS2SX_RYUJINX` they are skipped. How it works: the case is built with
`CS2SX_TEST_OUTPUT=1`, so the runtime mirrors every stdout line to
`svcOutputDebugString`; Ryujinx logs those lines, and the harness reads them from its
output. Apps keep running in their frame loop, so the file holds the *first* lines of
the output — keep it to what is deterministic (no timers, no network addresses).

To record the output of every case that prints something:

```bash
CS2SX_RYUJINX=… CS2SX_UPDATE_OUTPUT=1 dotnet test --filter RuntimeTests
```

Check each recorded line against the C# source before committing — a recorded wrong
result would otherwise become the expectation.

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

## Working on the API package (src/CS2SX.Switch)

The transpiler embeds the API sources directly, so tests always use the current API.
App projects built with `dotnet build` restore the **package** instead, and NuGet caches it
by version: after changing the API without bumping the version in `Directory.Build.props`,
delete `~/.nuget/packages/cs2sx.switch/<version>` before trying it in an app project.

## Adding a case

Create `Cases/<Name>/Program.cs` with a class `<Name>App : SwitchApp` (more `.cs`/`.h`
files and a `cs2sx.json` are optional), run `dotnet test`, review the new snapshots.
If the case prints its results, add an `expect-output.txt` (see Runtime tests).
