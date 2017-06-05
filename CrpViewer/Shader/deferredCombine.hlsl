Texture2D AlbedoTexture : register(t0);
Texture2D LightTexture : register(t1);
SamplerState Sampler : register(s0);

struct VS_Output
{
	float4 Position : SV_POSITION;
	float2 TextureUV : TEXCOORD0;
};

VS_Output VSMain(uint id : SV_VertexID)
{
	VS_Output Output;
	Output.TextureUV = float2((id << 1) & 2, id & 2);
	Output.Position = float4(Output.TextureUV * float2(2, -2) + float2(-1, 1), 1, 1);
	return Output;
}

float4 PSMain(VS_Output input) : SV_Target
{
	float3 albedo = AlbedoTexture.Sample(Sampler, input.TextureUV).rgb;
	float3 light = LightTexture.Sample(Sampler, input.TextureUV).rgb;

	return float4(albedo*light,1);
}

