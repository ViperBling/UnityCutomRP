using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshBall : MonoBehaviour
{
    private void Awake()
    {
        for (int i = 0; i < matrices.Length; i++)
        {
            matrices[i] = Matrix4x4.TRS(
                UnityEngine.Random.insideUnitSphere * 10f, 
                Quaternion.identity, 
                Vector3.one);
            baseColors[i] = new Vector4(
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                UnityEngine.Random.value,
                1.0f);
        }
    }

    private void Update()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
            _block.SetVectorArray(_baseColorID, baseColors);
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, matrices, 1023, _block);
    }

    private static int _baseColorID = Shader.PropertyToID("_BaseColor");

    [SerializeField] Mesh mesh = default;
    [SerializeField] Material material = default;

    private Matrix4x4[] matrices = new Matrix4x4[1023];
    private Vector4[] baseColors = new Vector4[1023];

    private MaterialPropertyBlock _block;
}
