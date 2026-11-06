# CS2SX C runtime

The C code every transpiled app is linked with. Embedded in the `cs2sx` tool and
exported into each build (`cs2sx_out/`).

```
Runtime/
├── cs2sx_prelude.h     first include of every TU (app code via _forward.h, runtime modules)
├── switchapp.h         app loop, graphics, input — includes all headers below
├── switchforms.h       strings, collections, StringBuilder, file system, NSP, USB, ...
├── AudioStub.h  HttpStub.h  FtpStub.h  NetStub.h  MotionStub.h  VibrationStub.h
├── SwkbdStub.h  SaveDataStub.h
└── src/rt_*.c          runtime modules (one per header)
```

## Header vs. module

Headers are the **interface**: types, macros, prototypes, `extern` declarations and
small `static inline` helpers. Modules (`src/rt_<header>.c`) hold the **state** and the
**implementations**. Modules are compiled like any other source but their inputs never
change between app builds, so the cached object files are reused — each module is
compiled once per project (and again only when the tool is updated).

Rules for runtime code:

1. **No `static` variables in headers.** A header is included into every translation
   unit, so a `static` there gives every `.c` file of the app its own copy — state set
   in one file is invisible in another (this broke SaveData, Nsp, Ftp, ... before).
   Declare `extern T name;` in the header, define it once in the module.
2. **Function-local `static`s belong in module functions**, for the same reason.
3. **Non-trivial functions go into the module**; keep `static inline` for one-to-three
   line accessors where a call would cost more than the body.
4. **Exception — opt-in extern libs:** functions that call into an optional extern lib
   (stb_image, dr_libs, miniz, libusbhsfs, ...) stay `static inline` in the header (or
   inside `#if __has_include(...)`). A module object is always linked; if it referenced
   an extern lib, every app without that lib would fail to link.
5. Network features take the socket layer from `NetStub.h`
   (`CS2SX_Net_Acquire`/`CS2SX_Net_Release`) — never call `socketInitializeDefault`/
   `socketExit` directly.

A new module needs an `<EmbeddedResource>` entry only if it is not matched by
`Runtime\src\*.c`; a new header needs its own `<EmbeddedResource>` line in
`CS2SX.csproj` and an entry in `BuildPipeline.GeneratedHeaders`.

`switchapp.h` (graphics) is not split yet: the software renderer is replaced by the
deko3d backend in the next phase.
