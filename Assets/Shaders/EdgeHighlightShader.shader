// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Shader created with Shader Forge v1.30 
// Shader Forge (c) Neat Corporation / Joachim Holmer - http://www.acegikmo.com/shaderforge/
// Note: Manually altering this data may prevent you from opening it in Shader Forge
/*SF_DATA;ver:1.30;sub:START;pass:START;ps:flbk:,iptp:0,cusa:False,bamd:0,lico:1,lgpr:1,limd:0,spmd:1,trmd:0,grmd:0,uamb:True,mssp:True,bkdf:False,hqlp:False,rprd:False,enco:False,rmgx:True,rpth:0,vtps:0,hqsc:True,nrmq:1,nrsp:0,vomd:0,spxs:False,tesm:0,olmd:1,culm:2,bsrc:3,bdst:3,dpts:6,wrdp:False,dith:0,rfrpo:False,rfrpn:Refraction,coma:15,ufog:True,aust:False,igpj:False,qofs:0,qpre:4,rntp:2,fgom:False,fgoc:False,fgod:False,fgor:False,fgmd:0,fgcr:0.5,fgcg:0.5,fgcb:0.5,fgca:1,fgde:0.01,fgrn:0,fgrf:300,stcl:False,stva:128,stmr:255,stmw:255,stcp:6,stps:6,stfa:0,stfz:0,ofsf:0,ofsu:0,f2p0:False,fnsp:False,fnfb:False;n:type:ShaderForge.SFN_Final,id:5182,x:33726,y:32916,varname:node_5182,prsc:2|emission-4244-OUT;n:type:ShaderForge.SFN_Fresnel,id:3337,x:32327,y:32986,varname:node_3337,prsc:2|EXP-3272-OUT;n:type:ShaderForge.SFN_Add,id:5632,x:32944,y:33025,varname:node_5632,prsc:2|A-145-OUT,B-145-OUT,C-145-OUT;n:type:ShaderForge.SFN_Multiply,id:2759,x:33160,y:33006,varname:node_2759,prsc:2|A-3337-OUT,B-5632-OUT;n:type:ShaderForge.SFN_ConstantClamp,id:145,x:32733,y:33040,varname:node_145,prsc:2,min:0.95,max:1|IN-3337-OUT;n:type:ShaderForge.SFN_Vector1,id:3272,x:31993,y:33124,varname:node_3272,prsc:2,v1:4;n:type:ShaderForge.SFN_Color,id:5708,x:33175,y:33180,ptovrint:False,ptlb:edgeColour,ptin:_edgeColour,varname:node_5708,prsc:2,glob:False,taghide:False,taghdr:False,tagprd:False,tagnsco:False,tagnrm:False,c1:1,c2:0.8,c3:0,c4:1;n:type:ShaderForge.SFN_Multiply,id:4244,x:33419,y:33080,varname:node_4244,prsc:2|A-2759-OUT,B-5708-RGB;proporder:5708;pass:END;sub:END;*/

Shader "Custom/EdgeHighlightShader" {
    Properties {
        _edgeColour ("edgeColour", Color) = (1,0.8,0,1)
    }
    SubShader {
        Tags {
            "Queue"="Overlay"
            "RenderType"="Transparent"
        }
        LOD 200
        Pass {
            Name "FORWARD"
            Tags {
                "LightMode"="ForwardBase"
            }
            Blend SrcAlpha SrcAlpha
            Cull Off
            ZTest Always
            ZWrite Off
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #define UNITY_PASS_FORWARDBASE
            #include "UnityCG.cginc"
            #pragma multi_compile_fwdbase_fullshadows
            #pragma multi_compile_fog
            #pragma target 3.0
            uniform float4 _edgeColour;
            struct VertexInput {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct VertexOutput {
                float4 pos : SV_POSITION;
                float4 posWorld : TEXCOORD0;
                float3 normalDir : TEXCOORD1;
                UNITY_FOG_COORDS(2)
            };
            VertexOutput vert (VertexInput v) {
                VertexOutput o = (VertexOutput)0;
                o.normalDir = UnityObjectToWorldNormal(v.normal);
                o.posWorld = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex );
                UNITY_TRANSFER_FOG(o,o.pos);
                return o;
            }
            float4 frag(VertexOutput i, float facing : VFACE) : COLOR {
                float isFrontFace = ( facing >= 0 ? 1 : 0 );
                float faceSign = ( facing >= 0 ? 1 : -1 );
                i.normalDir = normalize(i.normalDir);
                i.normalDir *= faceSign;
                float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
                float3 normalDirection = i.normalDir;
////// Lighting:
////// Emissive:
                float node_3337 = pow(1.0-max(0,dot(normalDirection, viewDirection)),4.0);
                float node_145 = clamp(node_3337,0.95,1);
                float3 emissive = ((node_3337*(node_145+node_145+node_145))*_edgeColour.rgb);
                float3 finalColor = emissive;
                fixed4 finalRGBA = fixed4(finalColor,1);
                UNITY_APPLY_FOG(i.fogCoord, finalRGBA);
                return finalRGBA;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
    CustomEditor "ShaderForgeMaterialInspector"
}
