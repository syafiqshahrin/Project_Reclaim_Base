Shader "Tests/TestShaderUnlit"
{
	Properties
	{
		_Color("Base Color", Color) = (1, 1, 1, 1)
		_Color2("Highlight Color", Color) = (1, 0, 0, 1)
	}
	Subshader
	{

		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl" 
			struct Attributes
			{
				float4 positionOS : POSITION;
				//half3 normal : NORMAL;
			};

			struct Varyings
			{
				float4 positionHCS : SV_POSITION;
				//half3 normal : TEXCOORD0;
			};

			CBUFFER_START(UnityPerMaterial)
				//put variables here
				bool _Targeted;
				half4 _Color;
				half4 _Color2;
			CBUFFER_END

			Varyings vert(Attributes IN)
			{
				Varyings OUT;
				OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
				//OUT.normal = TransformObjectToWorldNormal(IN.normal.xyz);
				return OUT;
			}

			half4 frag(Varyings IN) : SV_TARGET
			{
				half4 color = 0;

				if (_Targeted > 0)
				{
					color = _Color2.rgba;
				}
				else
				{
					color = _Color.rgba;
				}
				
				//color.rgb = IN.normal * 0.5 + 0.5;
				//color.rgba = _Color1.rgba;
				return color;

			}



			ENDHLSL
		}
	}
}