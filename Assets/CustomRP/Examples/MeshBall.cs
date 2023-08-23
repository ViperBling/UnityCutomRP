﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshBall : MonoBehaviour
{
    private void Awake()
    {
        for (int i = 0; i < _matrices.Length; i++)
        {
            _matrices[i] = Matrix4x4.TRS(
                UnityEngine.Random.insideUnitSphere * 10f, 
                Quaternion.Euler(
                    Random.value * 360f, Random.value * 360f, Random.value * 360f
                ), 
                Vector3.one * Random.Range(0.5f, 1.5f));
            _baseColors[i] = new Vector4(
                Random.value, Random.value, Random.value,
                Random.Range(0.5f, 1.0f));
            _metallic[i] = Random.value < 0.25f ? 1.0f : 0.0f;
            _smoothness[i] = Random.Range(0.05f, 0.95f);
        }
    }

    private void Update()
    {
        if (_block == null)
        {
            _block = new MaterialPropertyBlock();
            _block.SetVectorArray(_baseColorID, _baseColors);
            _block.SetFloatArray(_metallicID, _metallic);
            _block.SetFloatArray(_smoothnessID, _smoothness);
        }
        Graphics.DrawMeshInstanced(mesh, 0, material, _matrices, 1023, _block);
    }

    private static int 
        _baseColorID = Shader.PropertyToID("_BaseColor"),
        _metallicID = Shader.PropertyToID("_Metallic"),
        _smoothnessID = Shader.PropertyToID("_Smoothness");

    [SerializeField] Mesh mesh = default;
    [SerializeField] Material material = default;

    Matrix4x4[] _matrices = new Matrix4x4[1023];
    Vector4[] _baseColors = new Vector4[1023];

    float[]
        _metallic = new float[1023],
        _smoothness = new float[1023];

    private MaterialPropertyBlock _block;
}
