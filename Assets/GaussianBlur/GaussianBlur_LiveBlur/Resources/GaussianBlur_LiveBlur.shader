/*
GaussianBlur_LiveBlur.shader
This is the first version of the shader for people who still want to use it.
*/

Shader "Custom/GaussianBlur_LiveBlur" 
{
    Properties 
    {

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _BlurSize ("BlurSize", Range(0, 250)) = 25
        _Lightness ("Lightness", Range(-1, 1)) = 0

        _Quality ("Quality", Range(0,3)) = 3
    }
 
    Category 
    {

        Tags
         { 
             "Queue"="Transparent"  
             "RenderType"="Transparent" 
             "PreviewType"="Plane"
             "CanUseSpriteAtlas"="True"
         }

        SubShader 
        {
     
            // Horizontal blur
            GrabPass 
            {                    
                Tags { "LightMode" = "Always" }
            }
            Pass 
            {
                Tags { "LightMode" = "Always" }
             
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
             
                struct appdata_t 
                {
                    float4 vertex : POSITION;
                    float2 texcoord: TEXCOORD0;
                };
             
                struct v2f 
                {
                    float4 vertex : POSITION;
                    float4 uvgrab : TEXCOORD0;
                    float2 mask : TEXCOORD1; 
                };

                float4 _MainTex_ST;

                v2f vert (appdata_t v) 
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    #if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
                    #else
                    float scale = 1.0;
                    #endif
                    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
                    o.uvgrab.zw = o.vertex.zw;
                    o.mask = TRANSFORM_TEX( v.texcoord, _MainTex );
                    return o;
                }
             
                sampler2D _GrabTexture;
                float4 _GrabTexture_TexelSize;
                float _BlurSize;
                float _Lightness;
                float4 _Color;
                int _Quality;

                sampler2D _MainTex;


                half4 frag( v2f i ) : COLOR 
                {

                	
					fixed4 m = tex2D(_MainTex, i.mask);

                	float thisBlur = m.a * _BlurSize;
                	float thisLightness = m.a * _Lightness;
                 
                    half4 sum = half4(0,0,0,0);
                    #define GRABPIXEL(weight,kernely) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x , i.uvgrab.y + _GrabTexture_TexelSize.x * kernely * thisBlur, i.uvgrab.z, i.uvgrab.w))) * weight

                    if (_Quality == 0)
                    {
	                    sum += GRABPIXEL(0.05000,-4.0000);
						sum += GRABPIXEL(0.09000,-3.0000);
						sum += GRABPIXEL(0.12000,-2.0000);
						sum += GRABPIXEL(0.15000,-1.0000);
						sum += GRABPIXEL(0.18000,0.0000);
						sum += GRABPIXEL(0.15000,1.0000);
						sum += GRABPIXEL(0.12000,2.0000);
						sum += GRABPIXEL(0.09000,3.0000);
						sum += GRABPIXEL(0.05000,4.0000);

						sum /= ( ( -thisLightness + 1) * 1 );
                    }
                    else if (_Quality == 1)
                    {
						sum += GRABPIXEL(0.0500,-4.000);
						sum += GRABPIXEL(0.0700,-3.500);
						sum += GRABPIXEL(0.0900,-3.000);
						sum += GRABPIXEL(0.1050,-2.500);
						sum += GRABPIXEL(0.1200,-2.000);
						sum += GRABPIXEL(0.1350,-1.500);
						sum += GRABPIXEL(0.1500,-1.000);
						sum += GRABPIXEL(0.1650,-0.500);
						sum += GRABPIXEL(0.1800,0.000);
						sum += GRABPIXEL(0.1650,0.500);
						sum += GRABPIXEL(0.1500,1.000);
						sum += GRABPIXEL(0.1350,1.500);
						sum += GRABPIXEL(0.1200,2.000);
						sum += GRABPIXEL(0.1050,2.500);
						sum += GRABPIXEL(0.0900,3.000);
						sum += GRABPIXEL(0.0700,3.500);
						sum += GRABPIXEL(0.0500,4.000);

						sum /= ( ( -thisLightness + 1) * 1.95);
                    }
                    else if (_Quality == 2)
                    {
						sum += GRABPIXEL(0.0500,-4.000);
						sum += GRABPIXEL(0.0600,-3.750);
						sum += GRABPIXEL(0.0700,-3.500);
						sum += GRABPIXEL(0.0800,-3.250);
						sum += GRABPIXEL(0.0900,-3.000);
						sum += GRABPIXEL(0.0975,-2.750);
						sum += GRABPIXEL(0.1050,-2.500);
						sum += GRABPIXEL(0.1125,-2.250);
						sum += GRABPIXEL(0.1200,-2.000);
						sum += GRABPIXEL(0.1275,-1.750);
						sum += GRABPIXEL(0.1350,-1.500);
						sum += GRABPIXEL(0.1425,-1.250);
						sum += GRABPIXEL(0.1500,-1.000);
						sum += GRABPIXEL(0.1575,-0.750);
						sum += GRABPIXEL(0.1650,-0.500);
						sum += GRABPIXEL(0.1725,-0.250);
						sum += GRABPIXEL(0.1800,0.000);
						sum += GRABPIXEL(0.1725,0.250);
						sum += GRABPIXEL(0.1650,0.500);
						sum += GRABPIXEL(0.1575,0.750);
						sum += GRABPIXEL(0.1500,1.000);
						sum += GRABPIXEL(0.1425,1.250);
						sum += GRABPIXEL(0.1350,1.500);
						sum += GRABPIXEL(0.1275,1.750);
						sum += GRABPIXEL(0.1200,2.000);
						sum += GRABPIXEL(0.1125,2.250);
						sum += GRABPIXEL(0.1050,2.500);
						sum += GRABPIXEL(0.0975,2.750);
						sum += GRABPIXEL(0.0900,3.000);
						sum += GRABPIXEL(0.0800,3.250);
						sum += GRABPIXEL(0.0700,3.500);
						sum += GRABPIXEL(0.0600,3.750);
						sum += GRABPIXEL(0.0500,4.000);

						sum /= ( ( -thisLightness + 1) * 3.85);
                    }
                    else if (_Quality >= 3)
                    {
						sum += GRABPIXEL(0.0500,-4.000);
						sum += GRABPIXEL(0.0550,-3.875);
						sum += GRABPIXEL(0.0600,-3.750);
						sum += GRABPIXEL(0.0650,-3.625);
						sum += GRABPIXEL(0.0700,-3.500);
						sum += GRABPIXEL(0.0750,-3.375);
						sum += GRABPIXEL(0.0800,-3.250);
						sum += GRABPIXEL(0.0850,-3.125);
						sum += GRABPIXEL(0.0900,-3.000);
						sum += GRABPIXEL(0.0938,-2.875);
						sum += GRABPIXEL(0.0975,-2.750);
						sum += GRABPIXEL(0.1013,-2.625);
						sum += GRABPIXEL(0.1050,-2.500);
						sum += GRABPIXEL(0.1088,-2.375);
						sum += GRABPIXEL(0.1125,-2.250);
						sum += GRABPIXEL(0.1163,-2.125);
						sum += GRABPIXEL(0.1200,-2.000);
						sum += GRABPIXEL(0.1238,-1.875);
						sum += GRABPIXEL(0.1275,-1.750);
						sum += GRABPIXEL(0.1313,-1.625);
						sum += GRABPIXEL(0.1350,-1.500);
						sum += GRABPIXEL(0.1388,-1.375);
						sum += GRABPIXEL(0.1425,-1.250);
						sum += GRABPIXEL(0.1463,-1.125);
						sum += GRABPIXEL(0.1500,-1.000);
						sum += GRABPIXEL(0.1538,-0.875);
						sum += GRABPIXEL(0.1575,-0.750);
						sum += GRABPIXEL(0.1613,-0.625);
						sum += GRABPIXEL(0.1650,-0.500);
						sum += GRABPIXEL(0.1688,-0.375);
						sum += GRABPIXEL(0.1725,-0.250);
						sum += GRABPIXEL(0.1763,-0.125);
						sum += GRABPIXEL(0.1800,0.000);
						sum += GRABPIXEL(0.1763,0.125);
						sum += GRABPIXEL(0.1725,0.250);
						sum += GRABPIXEL(0.1688,0.375);
						sum += GRABPIXEL(0.1650,0.500);
						sum += GRABPIXEL(0.1613,0.625);
						sum += GRABPIXEL(0.1575,0.750);
						sum += GRABPIXEL(0.1538,0.875);
						sum += GRABPIXEL(0.1500,1.000);
						sum += GRABPIXEL(0.1463,1.125);
						sum += GRABPIXEL(0.1425,1.250);
						sum += GRABPIXEL(0.1388,1.375);
						sum += GRABPIXEL(0.1350,1.500);
						sum += GRABPIXEL(0.1313,1.625);
						sum += GRABPIXEL(0.1275,1.750);
						sum += GRABPIXEL(0.1238,1.875);
						sum += GRABPIXEL(0.1200,2.000);
						sum += GRABPIXEL(0.1163,2.125);
						sum += GRABPIXEL(0.1125,2.250);
						sum += GRABPIXEL(0.1088,2.375);
						sum += GRABPIXEL(0.1050,2.500);
						sum += GRABPIXEL(0.1013,2.625);
						sum += GRABPIXEL(0.0975,2.750);
						sum += GRABPIXEL(0.0938,2.875);
						sum += GRABPIXEL(0.0900,3.000);
						sum += GRABPIXEL(0.0850,3.125);
						sum += GRABPIXEL(0.0800,3.250);
						sum += GRABPIXEL(0.0750,3.375);
						sum += GRABPIXEL(0.0700,3.500);
						sum += GRABPIXEL(0.0650,3.625);
						sum += GRABPIXEL(0.0600,3.750);
						sum += GRABPIXEL(0.0550,3.875);
						sum += GRABPIXEL(0.0500,4.000);

						sum /= ( ( -thisLightness + 1) * 7.65);
                    }


					sum.x += lerp(0,_Color.r,_Color.a * m.a );
					sum.y += lerp(0,_Color.g,_Color.a * m.a );
					sum.z += lerp(0,_Color.b,_Color.a * m.a );

					sum.x *= m.r;
					sum.y *= m.g;
					sum.z *= m.b;

                    return sum;
                }
                ENDCG
            }



            //Reprocess Horizontal blur
            GrabPass 
            {                    
                Tags { "LightMode" = "Always" }
            }
            Pass 
            {
                Tags { "LightMode" = "Always" }
             
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest
                #include "UnityCG.cginc"
             
                struct appdata_t 
                {
                    float4 vertex : POSITION;
                    float2 texcoord: TEXCOORD0;
                };
             
                struct v2f 
                {
                    float4 vertex : POSITION;
                    float4 uvgrab : TEXCOORD0;
                    float2 mask : TEXCOORD1; 
                };

                float4 _MainTex_ST;

                v2f vert (appdata_t v) 
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    #if UNITY_UV_STARTS_AT_TOP
                    float scale = -1.0;
                    #else
                    float scale = 1.0;
                    #endif
                    o.uvgrab.xy = (float2(o.vertex.x, o.vertex.y*scale) + o.vertex.w) * 0.5;
                    o.uvgrab.zw = o.vertex.zw;
                    o.mask = TRANSFORM_TEX( v.texcoord, _MainTex );
                    return o;
                }
             
                sampler2D _GrabTexture;
                float4 _GrabTexture_TexelSize;
                float _BlurSize;
                float _Lightness;
                float4 _Color;
                int _Quality;

                sampler2D _MainTex;


                half4 frag( v2f i ) : COLOR 
                {

					fixed4 m = tex2D(_MainTex, i.mask);

                	float thisBlur = m.a * _BlurSize;
                	float thisLightness = m.a * _Lightness;
                 
                    half4 sum = half4(0,0,0,0);
                    #define GRABPIXEL(weight,kernelx) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x + _GrabTexture_TexelSize.x * kernelx * thisBlur, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w))) * weight


                    if (_Quality == 0)
                    {
	                    sum += GRABPIXEL(0.05000,-4.0000);
						sum += GRABPIXEL(0.09000,-3.0000);
						sum += GRABPIXEL(0.12000,-2.0000);
						sum += GRABPIXEL(0.15000,-1.0000);
						sum += GRABPIXEL(0.18000,0.0000);
						sum += GRABPIXEL(0.15000,1.0000);
						sum += GRABPIXEL(0.12000,2.0000);
						sum += GRABPIXEL(0.09000,3.0000);
						sum += GRABPIXEL(0.05000,4.0000);

						sum /= ( ( -thisLightness + 1) * 1 );
                    }
                    else if (_Quality == 1)
                    {
						sum += GRABPIXEL(0.0500,-4.000);
						sum += GRABPIXEL(0.0700,-3.500);
						sum += GRABPIXEL(0.0900,-3.000);
						sum += GRABPIXEL(0.1050,-2.500);
						sum += GRABPIXEL(0.1200,-2.000);
						sum += GRABPIXEL(0.1350,-1.500);
						sum += GRABPIXEL(0.1500,-1.000);
						sum += GRABPIXEL(0.1650,-0.500);
						sum += GRABPIXEL(0.1800,0.000);
						sum += GRABPIXEL(0.1650,0.500);
						sum += GRABPIXEL(0.1500,1.000);
						sum += GRABPIXEL(0.1350,1.500);
						sum += GRABPIXEL(0.1200,2.000);
						sum += GRABPIXEL(0.1050,2.500);
						sum += GRABPIXEL(0.0900,3.000);
						sum += GRABPIXEL(0.0700,3.500);
						sum += GRABPIXEL(0.0500,4.000);

						sum /= ( ( -thisLightness + 1) * 1.95);
                    }
                    else if (_Quality == 2)
                    {
						sum += GRABPIXEL(0.0500,-4.000);
						sum += GRABPIXEL(0.0600,-3.750);
						sum += GRABPIXEL(0.0700,-3.500);
						sum += GRABPIXEL(0.0800,-3.250);
						sum += GRABPIXEL(0.0900,-3.000);
						sum += GRABPIXEL(0.0975,-2.750);
						sum += GRABPIXEL(0.1050,-2.500);
						sum += GRABPIXEL(0.1125,-2.250);
						sum += GRABPIXEL(0.1200,-2.000);
						sum += GRABPIXEL(0.1275,-1.750);
						sum += GRABPIXEL(0.1350,-1.500);
						sum += GRABPIXEL(0.1425,-1.250);
						sum += GRABPIXEL(0.1500,-1.000);
						sum += GRABPIXEL(0.1575,-0.750);
						sum += GRABPIXEL(0.1650,-0.500);
						sum += GRABPIXEL(0.1725,-0.250);
						sum += GRABPIXEL(0.1800,0.000);
						sum += GRABPIXEL(0.1725,0.250);
						sum += GRABPIXEL(0.1650,0.500);
						sum += GRABPIXEL(0.1575,0.750);
						sum += GRABPIXEL(0.1500,1.000);
						sum += GRABPIXEL(0.1425,1.250);
						sum += GRABPIXEL(0.1350,1.500);
						sum += GRABPIXEL(0.1275,1.750);
						sum += GRABPIXEL(0.1200,2.000);
						sum += GRABPIXEL(0.1125,2.250);
						sum += GRABPIXEL(0.1050,2.500);
						sum += GRABPIXEL(0.0975,2.750);
						sum += GRABPIXEL(0.0900,3.000);
						sum += GRABPIXEL(0.0800,3.250);
						sum += GRABPIXEL(0.0700,3.500);
						sum += GRABPIXEL(0.0600,3.750);
						sum += GRABPIXEL(0.0500,4.000);

						sum /= ( ( -thisLightness + 1) * 3.85);
                    }
                    else if (_Quality >= 3)
                    {
						sum += GRABPIXEL(0.0500,-4.000);
						sum += GRABPIXEL(0.0550,-3.875);
						sum += GRABPIXEL(0.0600,-3.750);
						sum += GRABPIXEL(0.0650,-3.625);
						sum += GRABPIXEL(0.0700,-3.500);
						sum += GRABPIXEL(0.0750,-3.375);
						sum += GRABPIXEL(0.0800,-3.250);
						sum += GRABPIXEL(0.0850,-3.125);
						sum += GRABPIXEL(0.0900,-3.000);
						sum += GRABPIXEL(0.0938,-2.875);
						sum += GRABPIXEL(0.0975,-2.750);
						sum += GRABPIXEL(0.1013,-2.625);
						sum += GRABPIXEL(0.1050,-2.500);
						sum += GRABPIXEL(0.1088,-2.375);
						sum += GRABPIXEL(0.1125,-2.250);
						sum += GRABPIXEL(0.1163,-2.125);
						sum += GRABPIXEL(0.1200,-2.000);
						sum += GRABPIXEL(0.1238,-1.875);
						sum += GRABPIXEL(0.1275,-1.750);
						sum += GRABPIXEL(0.1313,-1.625);
						sum += GRABPIXEL(0.1350,-1.500);
						sum += GRABPIXEL(0.1388,-1.375);
						sum += GRABPIXEL(0.1425,-1.250);
						sum += GRABPIXEL(0.1463,-1.125);
						sum += GRABPIXEL(0.1500,-1.000);
						sum += GRABPIXEL(0.1538,-0.875);
						sum += GRABPIXEL(0.1575,-0.750);
						sum += GRABPIXEL(0.1613,-0.625);
						sum += GRABPIXEL(0.1650,-0.500);
						sum += GRABPIXEL(0.1688,-0.375);
						sum += GRABPIXEL(0.1725,-0.250);
						sum += GRABPIXEL(0.1763,-0.125);
						sum += GRABPIXEL(0.1800,0.000);
						sum += GRABPIXEL(0.1763,0.125);
						sum += GRABPIXEL(0.1725,0.250);
						sum += GRABPIXEL(0.1688,0.375);
						sum += GRABPIXEL(0.1650,0.500);
						sum += GRABPIXEL(0.1613,0.625);
						sum += GRABPIXEL(0.1575,0.750);
						sum += GRABPIXEL(0.1538,0.875);
						sum += GRABPIXEL(0.1500,1.000);
						sum += GRABPIXEL(0.1463,1.125);
						sum += GRABPIXEL(0.1425,1.250);
						sum += GRABPIXEL(0.1388,1.375);
						sum += GRABPIXEL(0.1350,1.500);
						sum += GRABPIXEL(0.1313,1.625);
						sum += GRABPIXEL(0.1275,1.750);
						sum += GRABPIXEL(0.1238,1.875);
						sum += GRABPIXEL(0.1200,2.000);
						sum += GRABPIXEL(0.1163,2.125);
						sum += GRABPIXEL(0.1125,2.250);
						sum += GRABPIXEL(0.1088,2.375);
						sum += GRABPIXEL(0.1050,2.500);
						sum += GRABPIXEL(0.1013,2.625);
						sum += GRABPIXEL(0.0975,2.750);
						sum += GRABPIXEL(0.0938,2.875);
						sum += GRABPIXEL(0.0900,3.000);
						sum += GRABPIXEL(0.0850,3.125);
						sum += GRABPIXEL(0.0800,3.250);
						sum += GRABPIXEL(0.0750,3.375);
						sum += GRABPIXEL(0.0700,3.500);
						sum += GRABPIXEL(0.0650,3.625);
						sum += GRABPIXEL(0.0600,3.750);
						sum += GRABPIXEL(0.0550,3.875);
						sum += GRABPIXEL(0.0500,4.000);

						sum /= ( ( -thisLightness + 1) * 7.65);
                    }

					sum.x += lerp(0,_Color.r,_Color.a * m.a );
					sum.y += lerp(0,_Color.g,_Color.a * m.a );
					sum.z += lerp(0,_Color.b,_Color.a * m.a );

					sum.x *= m.r;
					sum.y *= m.g;
					sum.z *= m.b;

                    return sum;


                }
                ENDCG
            }
        }
    }
}