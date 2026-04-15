Shader "DongA/BlackHoleDistortion"
{
    Properties
    {
        // Unity UI tự điền _MainTex = sprite gắn vào Image component
        // Alpha của sprite = hình dạng vùng bị distort
        _MainTex     ("Sprite (Alpha = Mask)", 2D) = "white" {}

        _SwirlAngle  ("Swirl Angle (rad)", Float) = 0
        _Strength    ("Distortion Strength", Range(0, 10)) = 2.0
        _Falloff     ("Inner Falloff Power", Range(0.5, 8)) = 2.0
        _Center      ("Center (Viewport XY)", Vector) = (0.5, 0.5, 0, 0)
    }

    SubShader
    {
        Tags { "Queue" = "Transparent+10" "RenderType" = "Transparent" }
        Cull Off
        ZWrite Off
        ZTest Always

        GrabPass { "_BlackHoleGrabTex" }

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _BlackHoleGrabTex;
            sampler2D _MainTex;         // sprite alpha = distortion mask
            float4    _MainTex_ST;

            float  _SwirlAngle;
            float  _Strength;
            float  _Falloff;
            float4 _Center;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;  // sprite UV để đọc alpha
            };

            struct v2f
            {
                float4 pos     : SV_POSITION;
                float4 grabPos : TEXCOORD0;
                float2 uv      : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos     = UnityObjectToClipPos(v.vertex);
                o.grabPos = ComputeGrabScreenPos(o.pos);
                o.uv      = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // --- Mask từ alpha của sprite ---
                // Pixel nằm ngoài sprite (alpha=0) -> không distort gì cả
                float spriteMask = tex2D(_MainTex, i.uv).a;

                // --- Falloff từ tâm để distortion giảm dần ra rìa ---
                // Dùng sprite UV (0,0 = góc, 0.5,0.5 = tâm) thay vì screen space
                float2 uvFromCenter = i.uv - 0.5;
                float  dist         = length(uvFromCenter) * 2.0; // 0=tâm, 1=rìa
                float  innerFalloff = pow(saturate(1.0 - dist), _Falloff);

                // Kết hợp: chỉ distort trong vùng sprite alpha, mạnh nhất ở tâm
                float weight = spriteMask * innerFalloff;

                // --- Swirl distortion trong screen space ---
                float2 screenUV = i.grabPos.xy / i.grabPos.w;
                float  aspect   = _ScreenParams.x / _ScreenParams.y;

                float2 delta = screenUV - _Center.xy;
                delta.x     *= aspect;

                float swirlRad = _SwirlAngle * _Strength * weight;
                float s = sin(swirlRad);
                float c = cos(swirlRad);

                float2 rotated;
                rotated.x = c * delta.x - s * delta.y;
                rotated.y = s * delta.x + c * delta.y;
                rotated.x /= aspect;

                float2 distortedUV = _Center.xy + rotated;

                // Nếu ngoài sprite: trả về pixel gốc (không distort)
                float2 finalUV = lerp(screenUV, distortedUV, spriteMask);

                return tex2D(_BlackHoleGrabTex, finalUV);
            }
            ENDCG
        }
    }
}
