#pragma once
// ============================================================================
// cs2sx_prelude.h — the fixed part every translation unit starts with.
//
// Included first by the generated _forward.h (app code) and by the runtime
// modules (src/rt_*.c). It must not depend on the app: the runtime modules are
// compiled once and reused (cached object files) for every app build.
// ============================================================================

#include <switch.h>
#include <stdlib.h>
#include <string.h>
#include <stdio.h>
#include <stdbool.h>
#include <limits.h>
#include <float.h>
#include <math.h>

typedef void (*Action)(void*);
typedef void (*Action_t)(void);

typedef struct Control     Control;
typedef struct Form        Form;
typedef struct Label       Label;
typedef struct Button      Button;
typedef struct ProgressBar ProgressBar;
typedef struct SwitchApp   SwitchApp;

#include "switchapp.h"

extern char _cs2sx_strbuf[1024];
