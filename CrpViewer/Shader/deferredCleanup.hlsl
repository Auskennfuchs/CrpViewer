struct VS_Output
{
	float4 Pos : SV_POSITION;
	float2 Tex : TEXCOORD0;
};

struct PS_Output 
{
	float4 Diffuse : SV_Target0;
	float4 Normal : SV_Target1;
	float4 Position : SV_Target2;
};

VS_Output VSMain(uint id : SV_VertexID)
{
	VS_Output Output;
	Output.Tex = float2((id << 1) & 2, id & 2);
	Output.Pos = float4(Output.Tex * float2(2, -2) + float2(-1, 1), 1, 1);
	return Output;
}

PS_Output PSMain(VS_Output input) 
{
	PS_Output output;

	output.Diffuse = float4(0, 0, 0, 1);
	output.Normal = float4(0, 0, 0, 0);
	output.Position = float4(0, 0, 0, 1);

	return output;
}

