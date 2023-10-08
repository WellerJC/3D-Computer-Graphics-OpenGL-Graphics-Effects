#version 330 
 
uniform vec4 uLightDirection; 
uniform vec4 uEyePosition;
uniform vec3 uAmbientLight;
uniform sampler2D uTextureSampler;

in vec2 oTexCoords;

in vec4 oNormal; 
in vec4 oSurfacePosition; 

out vec4 FragColour; 
 
void main()  
{  
float textureFactor = 0.5;

 vec4 scatteredLight = vec4(vec3(uAmbientLight),1);
vec4 lightDir = normalize(uLightDirection - oSurfacePosition); 
vec4 eyeDirection = normalize(uEyePosition - oSurfacePosition);
vec4 reflectedVector = reflect(-lightDir, oNormal);

float diffuseFactor = max(dot(oNormal, lightDir), 0); 
float specularFactor = pow(max(dot(reflectedVector, eyeDirection), 0.0), 2); 
float ambientFactor = max(dot(oNormal, scatteredLight),0);

vec4 lightColor = vec4(vec3((diffuseFactor + specularFactor) * ambientFactor), 1); 

vec4 textureColor = texture(uTextureSampler, oTexCoords);

FragColour = vec4(vec3(lightColor * (1 - textureFactor) + textureColor * textureFactor), 1);
}
