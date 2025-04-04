using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent (typeof(Camera))]
public class CameraEffect : MonoBehaviour
{
    public Material material = null;


    private Material m_renderMaterial;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (material == null)
        {
            Graphics.Blit(source, destination);
            return;
        }
        
        Graphics.Blit(source, destination, material);
    }
}