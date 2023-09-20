using UnityEngine;


/*
 *  阴影设置类，用于管理阴影各类参数
 *  TextureSize : 阴影贴图大小
 *  maxDistance : 最大阴影绘制距离
 *  Directional : 直射光的阴影配置，单个纹理多阴影贴图
 */
[System.Serializable]
public class ShadowSettings
{
    public enum TextureSize
    {
        _256 = 256, _512 = 512, _1024 = 1024,
        _2048 = 2048, _4096 = 4096, _8192 = 8192
    }
    
    [Min(0f)] public float maxDistance = 100.0f;

    [System.Serializable]
    public struct Directional
    {
        public TextureSize atlasSize;

        [Range(1, 4)] public int cascadeCount;
        [Range(0.0f, 1.0f)] public float cascadeRatio1, cascadeRatio2, cascadeRatio3;
        public Vector3 CascadeRatios => new Vector3(cascadeRatio1, cascadeRatio2, cascadeRatio3);
    }

    public Directional directional = new Directional
    {
        atlasSize = TextureSize._1024,
        cascadeCount = 4,
        cascadeRatio1 = 0.1f,
        cascadeRatio2 = 0.25f,
        cascadeRatio3 = 0.5f
    };
}
