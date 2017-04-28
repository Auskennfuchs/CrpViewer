Texture2D ShaderTexture : register(t0);
Texture2D ShaderNormal : register(t1);
SamplerState Sampler : register(s0);
SamplerState SamplerNormal : register(s1);

cbuffer PerObject: register(b0)
{
    matrix worldViewProjMatrix;
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projMatrix;
	float3 lightDir;
	float3 viewPosition;
};

struct VertexShaderInput
{
    float3 Position : POSITION;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT;
	float2 TextureUV : TEXCOORD0;
	float3 BiNormal : BINORMAL;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT;
	float3 BiNormal : BINORMAL;
	float2 TextureUV : TEXCOORD0;
	float3 viewDirection : TEXCOORD1;
};

VertexShaderOutput VSMain(VertexShaderInput input)
{

	VertexShaderOutput output = (VertexShaderOutput)0;

	output.Position = mul(float4(input.Position,1.0f), worldViewProjMatrix);
    output.Normal = normalize(mul(input.Normal, (float3x3)worldMatrix));
	output.Tangent = normalize(mul(input.Tangent, (float3x3)worldMatrix));
	output.BiNormal = normalize(mul(input.BiNormal, (float3x3)worldMatrix));
	output.TextureUV = input.TextureUV;

	float3 worldPosition = mul(input.Position, (float3x3)worldMatrix);
	output.viewDirection = normalize(viewPosition - worldPosition);

    return output;
}

float4 PSMain(VertexShaderOutput input) : SV_Target
{
	float3 lDir = -lightDir;
	float3 ambient = {0.1f,0.1f,0.1f};

	float4 bM = ShaderNormal.Sample(SamplerNormal, input.TextureUV);
	float3 bumpMap = bM.rgb;
	bumpMap.r = bumpMap.r;
	bumpMap = bumpMap*2.0f - 1.0f;
	bumpMap.z = sqrt(1 - dot(bumpMap.xy, bumpMap.xy));

	float3 bumpNormal = bumpMap.x*input.Tangent + bumpMap.y*input.BiNormal + bumpMap.z*input.Normal;
	bumpNormal = normalize(bumpNormal);

	float lightIntensity = saturate(dot(bumpNormal, lDir));
	float3 diffuse = ShaderTexture.Sample(Sampler, input.TextureUV).rgb;

	float3 col = diffuse*lightIntensity + diffuse*ambient;

	if (lightIntensity > 0.0f) {
		float specularIntensity = 1-bM.b;
		float3 reflection = normalize(2 * lightIntensity*bumpNormal - lightDir);
		float3 specular = pow(saturate(dot(reflection, input.viewDirection)), 16.0f);
		specular = specular*specularIntensity;
		col = saturate(col + specular);
	}
/*	if (1.0f - bM.a < 0.1f) {
		discard;
	}*/

	return float4(col,1.0f);

//	return float4(bumpNormal/2.0f+0.5f,1.0f);
//	return float4(input.Position.z / input.Position.w*16.0f, input.Position.z / input.Position.w * 16.0f, input.Position.z / input.Position.w *16.0f, 1);
//	return float4(input.Normal/2.0f+0.5f, 1.0f);
}