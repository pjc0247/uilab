/*
GaussianBlur_RenderBlur.shader
uses properties and the global textures to generate a blurred effect on the screen.

*/


Shader "Custom/GaussianBlur_RenderBlur"
{
	Properties
	{
		[PerRendererData] _MainTex ("_MainTex", 2D) = "white" {}
		_Lightness ("_Lightness", Range(-1,1)) = 0


		_BlurSize ("_BlurSize", Range(0, 1)) = 1

		[Toggle] _AlphaAsBlurSize("_AlphaAsBlurSize", int) = 0


		//this was used for debugging
		_RenderBlurTex("_RenderBlurTex", int) = -1

	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
			"DisableBatching" = "True"
		}

		Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
				half4 screenpos : TEXCOORD2;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 screenuv : TEXCOORD1;
				half4 color : COLOR;
				float2 screenpos : TEXCOORD2;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.screenuv = ((o.vertex.xy / o.vertex.w) + 1) * 0.5;
				o.color = v.color;
				o.screenpos = ComputeScreenPos(o.vertex);
				return o;
			}

			float2 safemul(float4x4 M, float4 v)
			{
				float2 r;

				r.x = dot(M._m00_m01_m02, v);
				r.y = dot(M._m10_m11_m12, v);

				return r;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;
			// x contains 1.0/width
			// y contains 1.0/height
			// z contains width
			// w contains height

			uniform float _Lightness;

			float _BlurSize;
			int _AlphaAsBlurSize;
			int _TextureCnt;

			uniform sampler2D _Blur0;
			uniform sampler2D _Blur1;
			uniform sampler2D _Blur2;
			uniform sampler2D _Blur3;
			uniform sampler2D _Blur4;

			//this was used for debugging
			int _RenderBlurTex;

			float4 frag(v2f i) : SV_Target
			{
				float4 m = tex2D(_MainTex, i.uv);
				// float2 offset = safemul(unity_ObjectToWorld, tex2D(_OffsetTex, i.uv) * 2 - 1);
				// float4 ambient = tex2D(_AmbientTex, (i.screenuv + offset * _GlobalRefractionMag * 5) * 2);


				float2 uvWH = float2(_MainTex_TexelSize.z / _ScreenParams.x,_MainTex_TexelSize.w / _ScreenParams.y);
				uvWH.x *= _MainTex_TexelSize.x;
				uvWH.y *= _MainTex_TexelSize.y;


				float4 blurColor = float4(0,0,0,0);
				float2 buv = float2(i.screenpos.x - (uvWH.x / 2),i.screenpos.y - (uvWH.y / 2));

				float4 bcn = float4(0, 0, 0, 0);
				float4 bc0 = tex2D(_Blur0, buv);
				float4 bc1 = tex2D(_Blur1, buv);
				float4 bc2 = tex2D(_Blur2, buv);
				float4 bc3 = tex2D(_Blur3, buv);
				float4 bc4 = tex2D(_Blur4, buv);
				bcn = bc0;
				bcn.a = 0;

				if (_AlphaAsBlurSize == 1)
				{
					_BlurSize *= m.a;
				}

				 float textureSection = (1.0 / _TextureCnt);

				 if (_BlurSize < textureSection)
				 {
					 blurColor = bc0;
					 blurColor.a = (_BlurSize * _TextureCnt);
				 }
				 else if (_BlurSize >= textureSection && _BlurSize < textureSection * 2)
				 {
					 float bs = (_BlurSize - textureSection) * _TextureCnt;
					 blurColor = lerp(bc0, bc1, bs);
				 }
				 else if (_BlurSize >= textureSection * 2 && _BlurSize < textureSection * 3)
				 {
					 float bs = (_BlurSize - (textureSection * 2)) * _TextureCnt;
					 blurColor = lerp(bc1, bc2, bs);
				 }
				 else if (_BlurSize >= textureSection * 3 && _BlurSize < textureSection * 4)
				 {
					 float bs = (_BlurSize - (textureSection * 3)) * _TextureCnt;
					 blurColor = lerp(bc2, bc3, bs);
				 }
				 else if (_BlurSize >= textureSection * 4 && _BlurSize < textureSection * 5)
				 {
					 float bs = (_BlurSize - (textureSection * 4)) * _TextureCnt;
					 blurColor = lerp(bc3, bc4, bs);
				 }
				 else //if (_BlurSize >= textureSection * 5 && _BlurSize < textureSection * 6)
				 {
					 //float bs = (_BlurSize - (textureSection * 5)) * _TextureCnt;
					 //blurColor = lerp(bc4, bc4, bs);
					 blurColor = bc4;
				 }

				 


				float4 finalColor = blurColor * i.color;
				finalColor.a = i.color.a * m.a * blurColor.a;



				float l = _Lightness + 1;

				finalColor.rgb *= l;

				//this was used for debugging
				
				if (_RenderBlurTex == -1)
				{
					//do nothing
				}
				else if (_RenderBlurTex == 0)
				{
					finalColor = bc0;
				}
				else if (_RenderBlurTex == 1)
				{
					finalColor = bc1;
				}
				else if (_RenderBlurTex == 2)
				{
					finalColor = bc2;
				}
				else if (_RenderBlurTex == 3)
				{
					finalColor = bc3;
				}
				else if (_RenderBlurTex == 4)
				{
					finalColor = bc4;
				}
				

				return finalColor;
			}
			ENDCG
		}
	}

	Fallback "Sprites/Default"
}