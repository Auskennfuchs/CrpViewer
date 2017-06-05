Texture2D ShaderTexture : register(t0);
Texture2D ShaderNormalSpecular : register(t1);
SamplerState Sampler : register(s0);
SamplerState SamplerNormal : register(s1);

cbuffer PerObject: register(b0)
{
	matrix worldViewProjMatrix;
	matrix worldMatrix;
	matrix viewMatrix;
	matrix projMatrix;
};

struct VS_Input
{
	float3 Position : POSITION;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT;
	float2 TextureUV : TEXCOORD0;
	float3 BiNormal : BINORMAL;
};


struct VS_Output
{
	float4 Position : SV_Position;
	float3 Normal : NORMAL;
	float3 Tangent : TANGENT;
	float3 BiNormal : BINORMAL;
	float2 TextureUV : TEXCOORD0;
};

struct PS_Output
{
	float4 Diffuse : SV_Target0;
	float4 Normal : SV_Target1;
	float4 Position: SV_Target2;
};

VS_Output VSMain(VS_Input input) {
	VS_Output output;

	output.Position = mul(float4(input.Position, 1.0f), worldViewProjMatrix);
	output.Normal = normalize(mul(input.Normal, (float3x3)worldMatrix));
	output.Tangent = normalize(mul(input.Tangent, (float3x3)worldMatrix));
	output.BiNormal = normalize(mul(input.BiNormal, (float3x3)worldMatrix));
	output.TextureUV = input.TextureUV;

	return output;
}

PS_Output PSMain(VS_Output input)
{
	PS_Output output;

	float3 diffuse = ShaderTexture.Sample(Sampler, input.TextureUV).rgb;
	float4 bM = ShaderNormalSpecular.Sample(SamplerNormal, input.TextureUV);
	float3 bumpMap = bM.rgb;
	bumpMap.r = bumpMap.r;
	bumpMap = bumpMap*2.0f - 1.0f;
	bumpMap.z = sqrt(1 - dot(bumpMap.xy, bumpMap.xy));

	float3 bumpNormal = bumpMap.x*input.Tangent + bumpMap.y*input.BiNormal + bumpMap.z*input.Normal;
	bumpNormal = normalize(bumpNormal);


	output.Diffuse = float4(diffuse.rgb, 1);
	output.Normal = float4(bumpNormal/2.0f+0.5f, 1-bM.b);
	output.Position = input.Position;

	return output;
}
