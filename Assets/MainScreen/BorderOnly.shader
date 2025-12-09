Shader "Custom/BorderOnly"
{
    Properties
    {
        _Color ("Border Color", Color) = (1,1,1,1)
        _Thickness ("Thickness", Range(0.001, 0.2)) = 0.03
    }
    SubShader
    {
        // 🔸 투명 렌더링 + 뒤에 있는 물체 보이도록
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Pass
        {
            // 🔸 알파 블렌딩 / ZWrite OFF
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f {
                float2 uv     : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Thickness;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float u = i.uv.x;
                float v = i.uv.y;

                // 테두리 영역인지 여부
                bool isBorder =
                    (u < _Thickness) || (u > 1 - _Thickness) ||
                    (v < _Thickness) || (v > 1 - _Thickness);

                if (isBorder)
                {
                    // 테두리는 지정한 색으로(알파 1)
                    return _Color;
                }

                // 내부는 완전 투명
                return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}
