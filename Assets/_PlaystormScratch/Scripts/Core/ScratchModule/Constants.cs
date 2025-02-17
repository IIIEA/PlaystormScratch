using UnityEngine;

namespace _PlaystormScratch.Core.ScratchModule
{
  public static class Constants
  {
    public static class BrushShader
    {
      public const string BLEND_OP_SHADER_PARAM = "_BlendOpValue";
    }
    
    public static class MaskShader
    {
      private const string MASK_TEXTURE_PARAM = "_MaskTex";
      private const string OFFSET_PARAM = "_Offset";
      public static readonly int MaskTexture = Shader.PropertyToID(MASK_TEXTURE_PARAM);
      public static readonly int Offset = Shader.PropertyToID(OFFSET_PARAM);
    }
  }
}