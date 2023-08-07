using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 其实类似GPUScene的概念，mesh数据保存在了GPU缓存上
// 完全合批要求所有材质的着色器都是相同的，材质的内存布局相同
[DisallowMultipleComponent]
public class PerObjectMaterialProps : MonoBehaviour
{
    private void OnValidate()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
        }
        _block.SetColor(_baseColorID, baseColor);
        _block.SetFloat(_cutoffID, cutoff);
        if (baseMap != null)
        {
            _block.SetTexture(_baseMapID, baseMap);
        }
        GetComponent<Renderer>().SetPropertyBlock(_block);
    }

    private void Awake()
    {
        OnValidate();
    }

    // 和Shader中的属性标识符对应，根据这个ID来设置对应的属性
    static int _baseColorID = Shader.PropertyToID("_BaseColor");
    static int _baseMapID = Shader.PropertyToID("_BaseMap");
    static int _cutoffID = Shader.PropertyToID("_Cutoff");

    [SerializeField]
    Color baseColor = Color.white;
    [SerializeField]
    Texture baseMap;
    [SerializeField]
    float cutoff = 0.5f;

    static MaterialPropertyBlock _block;
    
}
