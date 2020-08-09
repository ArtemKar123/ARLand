// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/MultiTexture" {
    Properties {
        _TexTop ("Top", 2D) = "white" {}
        _TexMid ("Mid", 2D) = "white" {}
        _TexBot ("Bot", 2D) = "white" {}
        _BumpMap ("Bumpmap", 2D) = "bump" {}        
        _MidBumpMap ("MBumpmap", 2D) = "bump" {}
        _Color("Color", Color) = (1,1,1,0)
        _TopLimit("TopLimit", Range(-1, 1)) = 0.7
        _BotLimit("BotLimit", Range(-1, 1)) = 0
        _MidLimit("MidLimit", Range(-1, 1)) = 0.43
        _TotalMetres("TotalMetres", float) = 256
        _MidZone("_MidZone", Range(0, 256)) = 20                     
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader 
    {
        Tags {"Queue"="Transparent" "RenderType"="Opaque"}
        
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        LOD 100 
        Cull back
        
        CGPROGRAM        
        //#pragma surface surf Lambert        
        #pragma exclude_renderers ps3 flash          
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0
        
        sampler2D _TexTop;
        sampler2D _TexMid;
        sampler2D _TexBot;
        sampler2D _BumpMap;
        sampler2D _MidBumpMap;
        float _TopLimit;
        float _MidLimit;
        float _BotLimit;
        float _TotalMetres,_MidZone;
        float4 _Color;        
        //float4 _TexTop_ST;
        half _Glossiness;
        half _Metallic;
        
        struct Input {
            float2 uv_MainTex;
            float2 uv_TexTop;            
            float2 uv_BumpMap;
            float3 worldNormal;
            float incline;
            float3 worldPos;
            half2 texcoord : TEXCOORD0;            
            float4 vertex   : POSITION;
        };
        struct appdata_t 
        {
            float4 vertex   : POSITION;
            float2 texcoord : TEXCOORD0;
        };

        struct v2f 
        {
            float4 vertex  : SV_POSITION;
            half2 texcoord : TEXCOORD0;
        };
        void vert (inout appdata_full v, out Input o) {
          UNITY_INITIALIZE_OUTPUT(Input,o);
          o.incline = abs(v.normal.y);
        }
        
        void surf (Input IN, inout SurfaceOutputStandard o) {      
            float Alti = (IN.worldPos.y)/_TotalMetres;          
            
           /* v2f oo;

            oo.vertex     = UnityObjectToClipPos(IN.vertex);
            IN.texcoord.x = 1 - IN.texcoord.x;
            oo.texcoord   = TRANSFORM_TEX(IN.texcoord, _TexTop);*/
            fixed4 col = tex2D(_TexTop, IN.uv_MainTex) * _Color; // multiply by _Color
            //fixed4 resultCol = col;
            /*fixed4 resultCol =
               Alti >= _TopLimit ? tex2D(_TexTop, IN.uv_TexTop)
            : (Alti <= _BotLimit ? tex2D(_TexBot, IN.uv_TexTop)
            : (Alti <= _MidLimit && Alti >= _BotLimit ? tex2D(_TexMid, IN.uv_TexTop)
            : (Alti > 0 ? lerp(tex2D(_TexTop, IN.uv_TexTop), tex2D(_TexMid, IN.uv_TexTop),
                                 1 - ((Alti - _MidLimit) / (_TopLimit - _MidLimit)))
            : lerp(tex2D(_TexTop, IN.uv_TexTop), tex2D(_TexMid, IN.uv_TexTop),
                1 - ((abs(Alti) - _MidLimit) / (abs(_BotLimit) - _MidLimit))))));*/
            //resultCol = col;*/
                     
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            if(Alti <= 0){            
                o.Albedo = col.rgb;
                o.Alpha = col.a;
            }else{
                fixed4 resultCol;
                if(Alti <= _TopLimit){
                    resultCol = tex2D(_TexMid, IN.uv_MainTex);                    
                    o.Normal = UnpackNormal (tex2D (_MidBumpMap, IN.uv_BumpMap));
                }else{
                    resultCol = tex2D(_TexTop, IN.uv_MainTex);
                    o.Normal = UnpackNormal (tex2D (_BumpMap, IN.uv_BumpMap));
                }
                o.Albedo = resultCol.rgb;
                o.Alpha = resultCol.a;
            }
        }        
        ENDCG        
    }
    FallBack "Diffuse"
}


/*        
        Pass 
        {
            CGPROGRAM

            #pragma vertex vert alpha
            #pragma fragment frag alpha           

            #include "UnityCG.cginc"

            struct appdata_t 
            {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 vertex  : SV_POSITION;
                half2 texcoord : TEXCOORD0;
            };

            sampler2D _TexBot;
            float4 _TexBot_ST;
            float4 _Color;

            v2f vert (appdata_t v)
            {
                v2f o;

                o.vertex     = UnityObjectToClipPos(v.vertex);
                v.texcoord.x = 1 - v.texcoord.x;
                o.texcoord   = TRANSFORM_TEX(v.texcoord, _TexBot);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_TexBot, i.texcoord) * _Color; // multiply by _Color
                return col;
            }

            ENDCG                        
        }*/