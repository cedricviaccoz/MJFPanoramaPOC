Shader "EMPlus/WireFrameLineShader" {
SubShader { Pass {
   BindChannels { Bind "Color",color }
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite on Cull Off Fog { Mode Off }
} } }
