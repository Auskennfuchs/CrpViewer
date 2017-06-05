Texture2D AlbedoTextureInput : register(t0);
Texture2D NormalTextureInput : register(t1);
Texture2D PositionTextureInput : register(t2);
SamplerState NormalSampler : register(s0);

cbuffer LightBuffer: register(b0){
	matrix worldMatrix;
	float3 lightDir;
	matrix invViewProjMatrix;
	float3 viewPosition;
}

struct VS_Output
{
	float4 Position : SV_POSITION;
	float2 TextureUV : TEXCOORD0;
	float3 LightDir : TEXCOORD1;
//	float3 ViewDirection : TEXCOORD2;
};

VS_Output VSMain(uint id : SV_VertexID)
{
	VS_Output Output;
	Output.TextureUV = float2((id << 1) & 2, id & 2);
	Output.Position = float4(Output.TextureUV * float2(2, -2) + float2(-1, 1), 1, 1);
	Output.LightDir = lightDir;

	return Output;
}

float4 PSMain(VS_Output input) : SV_Target
{
	float4 albedo = AlbedoTextureInput.Sample(NormalSampler, input.TextureUV);
	float4 normalSpec = NormalTextureInput.Sample(NormalSampler, input.TextureUV);
	float3 normal = normalSpec.xyz * 2 - 1.0;
	float specularIntensity = normalSpec.a;

	float4 position = PositionTextureInput.Sample(NormalSampler, input.TextureUV);
	float4 pos = position/position.w;
	pos = mul(pos, invViewProjMatrix);
	pos /= pos.w;

	float3 lightVector = -normalize(input.LightDir);
	float NdL = saturate(dot(normal.xyz, lightVector));
	float3 col = albedo.rgb*NdL + albedo.rgb*float3(0.2,0.2,0.2);
	
	if (NdL > 0.0f) {
		float3 reflectionVector = normalize(reflect(normal,lightVector));
		float3 directionToCamera = normalize(-viewPosition + pos.xyz);
		float specularLight = specularIntensity * pow(saturate(dot(reflectionVector, directionToCamera)), 16.0f);
		col += specularLight*albedo.rgb;
	}

	return float4(saturate(col),1);
}

