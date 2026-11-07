# CS2SX C runtime

The C code every transpiled app is linked with. Embedded in the `cs2sx` tool and
exported into each build (`cs2sx_out/`).

```
Runtime/
├── cs2sx_prelude.h     first include of every TU (app code via _forward.h, runtime modules)
├── cs2sx_exc.h         exceptions: type info, try frames, CS2SX_Throw
├── cs2sx_delegate.h    delegates/events: CS2SX_Delegate, invocation lists, closures
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

Headers (`Runtime\*.h`) and modules (`Runtime\src\*.c`) are embedded by wildcard; a
new header also needs an entry in `BuildPipeline.GeneratedHeaders`.

## Language runtime

- **Exceptions** (`cs2sx_exc.h`): every exception is an `Exception` struct with a
  `CS2SX_TypeInfo` (`CS2SX_TI_<Name>`, parent pointer → `catch (T)` matches subclasses).
  `try` pushes a `CS2SX_ExFrame` (jmp_buf) on a thread-local stack; `CS2SX_Throw` pops
  the innermost frame and jumps there. Uncaught: stderr + system error dialog + exit.
  Runtime code throws where .NET throws, through the helpers in `cs2sx_exc.h`
  (`CS2SX_ThrowArgOutOfRange("index")`, `CS2SX_ThrowFormat(s)`, …) — never return a
  silent default for an invalid call. `List`/`Dictionary`/`Stack`/`Queue` accessors,
  `CS2SX_AT` (array bounds), `CS2SX_IDIV`/`CS2SX_IMOD` (division by zero) and
  `src/rt_number.c` (Parse, number → text) follow this.
- **Delegates** (`cs2sx_delegate.h`): one `CS2SX_Delegate` (function, bound target,
  next entry) for every delegate type; the per-type invokers are generated into
  `_forward.h`. Lifted functions take the bound object as first parameter.

## Graphics

`cs2sx_gfx.h` + `src/rt_gfx.c` render on the GPU with deko3d (linked via `-ldeko3d`):
one textured/vertex-colored pipeline, a per-frame vertex ring, draws batched by texture
(the 8×8 font and a white block share one atlas, so text and solid shapes batch
together), lazily uploaded textures, a one-pixel alpha fringe for anti-aliasing.

Shaders live in `shaders/*.glsl`. After editing them run `tools/build-shaders.ps1`
(needs devkitPro's `uam`); it regenerates `cs2sx_gfx_shaders.h`, which is committed so
nobody else needs `uam`.
