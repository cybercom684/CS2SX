#version 460
// CS2SX 2D renderer — vertex shader.
// Vertices are in LOGICAL screen coordinates (Graphics.Init size, y down);
// xform maps them to normalized device coordinates (y up).

layout (location = 0) in vec2 inPos;
layout (location = 1) in vec2 inUv;
layout (location = 2) in vec4 inColor;

layout (location = 0) out vec2 outUv;
layout (location = 1) out vec4 outColor;

layout (std140, binding = 0) uniform Xform
{
    vec4 xform;   // (2/W, -2/H, -1, +1)
} u;

void main()
{
    gl_Position = vec4(inPos.x * u.xform.x + u.xform.z,
                       inPos.y * u.xform.y + u.xform.w,
                       0.0, 1.0);
    outUv = inUv;
    outColor = inColor;
}
