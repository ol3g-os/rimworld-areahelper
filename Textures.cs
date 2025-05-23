using UnityEngine;
using Verse;

namespace AreaHelper
{
    [StaticConstructorOnStartup]
    public static class Textures
    {
        public static readonly Texture2D RedTex = SolidColorMaterials.NewSolidColorTexture(Color.red);
        
        public static readonly Texture2D GreenTex = SolidColorMaterials.NewSolidColorTexture(Color.green);
    }
}