/*
deprecated
-->used GaussianBlur_LiveBlur_NoSmear.shader instead
*/

/*
GaussianBlurV2.shader
This is the second version of the shader with no edge smear problems.
This shader requires the Screen's Width & Height, Panel's Width & Height and Panel's position.
*/

Shader "Custom/GaussianBlurV2" 
{
    Properties 
    {

		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _BlurSize ("BlurSize", Range(0, 250)) = 25
        _Lightness ("Lightness", Range(-1, 1)) = 0

        _Quality ("Quality", Range(0,3)) = 3

        [Space]

        [Toggle] _WorldSpace("WorldSpace", Float) = 0

        _ScreenWidth ("ScreenWidth", Float) = 0
        _ScreenHeight ("ScreenHeight", Float) = 0

        _PanelWidth ("PanelWidth", Float) = 0
        _PanelHeight ("PanelHeight", Float) = 0

        _PanelX ("PanelX", Float) = 0
        _PanelY ("PanelY", Float) = 0
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


                half _ScreenWidth;
                half _ScreenHeight;
                half _PanelX;
                half _PanelY;
                half _PanelWidth;
                half _PanelHeight;

                float _WorldSpace;

                half4 frag( v2f i ) : COLOR 
                {

                	if (_WorldSpace == 1)
                	{
                		_ScreenWidth= 0 ;
                		_ScreenHeight= 0 ;
                		_PanelX= 0 ;
                		_PanelY= 0 ;
                		_PanelWidth= 0.5 ;
                		_PanelHeight= 0.5 ;
                	}

                	//edited
                	#if UNITY_UV_STARTS_AT_TOP
                		i.uvgrab.y = 1-i.uvgrab.y;
                	#endif
                	
					fixed4 m = tex2D(_MainTex, i.mask);

                	// float thisBlur = m.a * _BlurSize;
                	float thisLightness = m.a * _Lightness;
                 
                 	half4 OC = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
                    
//                    #define GRABPIXEL(weight,kernelx) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x + _GrabTexture_TexelSize.x * kernelx * thisBlur, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w))) * weight
                	#define GRABPIXELX(weight,kernel) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x + _GrabTexture_TexelSize.x * kernel * _BlurSize, i.uvgrab.y , i.uvgrab.z, i.uvgrab.w))) * weight
                	#define GRABPIXELY(weight,kernel) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x , i.uvgrab.y + _GrabTexture_TexelSize.y * kernel * _BlurSize, i.uvgrab.z, i.uvgrab.w))) * weight


                    half4 sum = half4(0,0,0,0);

					half k = -4.000;
					half w = 0.0500;

					half ik = 1.000;
					half iw = 0.040; 

					half sumWeight = 0;

                	half P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;
                	half O = (_PanelX + (_PanelWidth/2))  /  _ScreenWidth;
                	half U = (_PanelX - (_PanelWidth/2))  /  _ScreenWidth;

                    if (_Quality == 0)
                    {

                    	ik = 1.000;
                    	iw = 0.040;

                    	P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//1
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//2
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//3
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//4
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//5
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//6
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//7
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//8
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//9
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;



                    }
                    else if (_Quality == 1)
                    {
                    	ik = 0.500;
                    	iw = 0.020;

                    	P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

                    	//1
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//2
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//3
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//4
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//5
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//6
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//7
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//8
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//9
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//10
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//11
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//12
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//13
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//14
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//15
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//16
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//17
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

                    }
                    else if (_Quality == 2)
                    {
                    	ik = 0.250;
                    	iw = 0.010;

                    	P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

                    	//1
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//2
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//3
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//4
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//5
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//6
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//7
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//8
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//9
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//10
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//11
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//12
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//13
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//14
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//15
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//16
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//17
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//18
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//19
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//20
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//21
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//22
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//23
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//24
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//25
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//26
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//27
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//28
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//29
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//30
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//31
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//32
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//33
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

                    }
                    else if (_Quality == 3)
                    {
                    	ik = 0.125;
                    	iw = 0.005;

                    	P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

                    	//1
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//2
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//3
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//4
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//5
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//6
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//7
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//8
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//9
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//10
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//11
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//12
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//13
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//14
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//15
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//16
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//17
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//18
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//19
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//20
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//21
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//22
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//23
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//24
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//25
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//26
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//27
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//28
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//29
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//30
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//31
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//32
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//33
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//34
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//35
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//36
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//37
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//38
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//39
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//40
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//41
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//42
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//43
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//44
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//45
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//46
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//47
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//48
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//49
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//50
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//51
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//52
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//53
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//54
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//55
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//56
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//57
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//58
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//59
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//60
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//61
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//62
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//63
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//64
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

						//65
						if (P > O || P < U)
						{
						 sum += GRABPIXELX(0,0);
						}
						else
						{
						 sum += GRABPIXELX(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.x + _GrabTexture_TexelSize.x * k * _BlurSize;

                    }


                    sum /= ( ( -thisLightness + 1) * (sumWeight * 1));

					sum.x += lerp(0,_Color.r,_Color.a * m.a );
					sum.y += lerp(0,_Color.g,_Color.a * m.a );
					sum.z += lerp(0,_Color.b,_Color.a * m.a );
					       
					sum = lerp(OC,sum,m.a);
					                            
                    return sum;
                }
                ENDCG
            }




            //Vertical 
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


                half _ScreenWidth;
                half _ScreenHeight;
                half _PanelX;
                half _PanelY;
                half _PanelWidth;
                half _PanelHeight;

                float _WorldSpace;

                half4 frag( v2f i ) : COLOR 
                {

                    if (_WorldSpace == 1)
                	{
                		_ScreenWidth= 0 ;
                		_ScreenHeight= 0 ;
                		_PanelX= 0 ;
                		_PanelY= 0 ;
                		_PanelWidth= 0.5 ;
                		_PanelHeight= 0.5 ;
                	}

                	//edited
                	#if UNITY_UV_STARTS_AT_TOP
                		i.uvgrab.y = 1-i.uvgrab.y;
                	#endif
                	
					fixed4 m = tex2D(_MainTex, i.mask);

//                	float thisBlur = m.a * _BlurSize;
                	float thisLightness = m.a * _Lightness;
                 
                 	half4 OC = tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(i.uvgrab));
                    
//                    #define GRABPIXEL(weight,kernelx) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x + _GrabTexture_TexelSize.x * kernelx * thisBlur, i.uvgrab.y, i.uvgrab.z, i.uvgrab.w))) * weight
                	#define GRABPIXELX(weight,kernel) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x + _GrabTexture_TexelSize.x * kernel * _BlurSize, i.uvgrab.y , i.uvgrab.z, i.uvgrab.w))) * weight
                	#define GRABPIXELY(weight,kernel) tex2Dproj( _GrabTexture, UNITY_PROJ_COORD(float4(i.uvgrab.x , i.uvgrab.y + _GrabTexture_TexelSize.y * kernel * _BlurSize, i.uvgrab.z, i.uvgrab.w))) * weight


                    half4 sum = half4(0,0,0,0);

					half k = -4.000;
					half w = 0.0500;

					half ik = 1.000;
					half iw = 0.040; 

					half sumWeight = 0;

                	half P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;
                	half O = (_PanelY + (_PanelHeight/2))  /  _ScreenHeight;
                	half U = (_PanelY - (_PanelHeight/2))  /  _ScreenHeight;

                    if (_Quality == 0)
                    {

                    	ik = 1.000;
                    	iw = 0.040;

                    	P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//1
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//2
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//3
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//4
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//5
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//6
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//7
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//8
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//9
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;



                    }
                    else if (_Quality == 1)
                    {
                    	ik = 0.500;
                    	iw = 0.020;

                    	P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

                    	//1
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//2
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//3
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//4
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//5
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//6
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//7
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//8
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//9
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//10
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//11
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//12
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//13
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//14
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//15
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//16
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//17
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

                    }
                    else if (_Quality == 2)
                    {
                    	ik = 0.250;
                    	iw = 0.010;

                    	P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

                    	//1
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//2
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//3
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//4
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//5
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//6
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//7
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//8
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//9
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//10
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//11
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//12
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//13
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//14
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//15
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//16
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//17
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//18
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//19
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//20
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//21
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//22
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//23
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//24
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//25
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//26
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//27
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//28
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//29
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//30
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//31
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//32
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//33
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

                    }
                    else if (_Quality == 3)
                    {
                    	ik = 0.125;
                    	iw = 0.005;

                    	P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

                    	//1
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//2
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//3
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//4
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//5
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//6
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//7
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//8
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//9
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//10
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//11
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//12
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//13
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//14
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//15
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//16
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//17
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//18
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//19
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//20
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//21
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//22
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//23
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//24
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//25
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//26
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//27
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//28
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//29
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//30
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//31
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//32
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//33
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//34
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//35
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//36
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//37
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//38
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//39
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//40
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//41
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//42
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//43
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//44
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//45
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//46
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//47
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//48
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//49
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//50
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//51
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//52
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//53
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//54
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//55
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//56
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//57
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//58
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//59
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//60
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//61
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//62
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//63
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//64
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

						//65
						if (P > O || P < U)
						{
						 sum += GRABPIXELY(0,0);
						}
						else
						{
						 sum += GRABPIXELY(w,k);
						 sumWeight += w;
						}

						k += ik;

						if (k > 0)
						{
						 w -= iw;
						}
						else
						{
						 w += iw;
						}

						P = i.uvgrab.y + _GrabTexture_TexelSize.y * k * _BlurSize;

                    }


                    sum /= ( ( -thisLightness + 1) * (sumWeight * 1));

					sum.x += lerp(0,_Color.r,_Color.a * m.a );
					sum.y += lerp(0,_Color.g,_Color.a * m.a );
					sum.z += lerp(0,_Color.b,_Color.a * m.a );
					       
					sum = lerp(OC,sum,m.a);

                    return sum;
                }
                ENDCG
            }



            
        }
    }
}