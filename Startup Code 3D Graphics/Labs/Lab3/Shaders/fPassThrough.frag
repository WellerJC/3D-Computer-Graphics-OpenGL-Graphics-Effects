#version 330
in vec3 vNormal;


out vec4 oColour;

void main()
{
     oColour = vec4(vNormal * 0.5 + 0.5, 1); 
}