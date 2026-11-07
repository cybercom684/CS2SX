#version 460
// CS2SX 2D renderer — fragment shader: texture × vertex color.
// Solid shapes sample the white texel of the font atlas, so every primitive
// uses this one pipeline.

layout (location = 0) in vec2 inUv;
layout (location = 1) in vec4 inColor;

layout (location = 0) out vec4 outColor;

layout (binding = 0) uniform sampler2D tex;

void main()
{
    outColor = texture(tex, inUv) * inColor;
}
