// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MFPS/Utils/Render OverAll"
{
    Properties{
       _Color("Main Color", Color) = (1,1,1,1)
       _MainTex("Base (RGB)", 2D) = "white" {}
    }
        Category{
        Tags { "Queue" = "Overlay" "IgnoreProjector" = "True" "RenderType" = "Opaque" }

           ZWrite On
           Cull Off
           ZTest Always

           SubShader {
                Pass {
                   SetTexture[_MainTex] {
                        constantColor[_Color]
                        Combine texture * constant, texture * constant
                     }
                }
            }
    }
}