using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;

[ExecuteAlways]
public class BuiltInISF : MonoBehaviour
{

    [SerializeField]
    Shader deferredShading = null;

    [SerializeField]
    bool enableInverseSquareFalloff = true;

    void OnValidate()
    {
        if (enableInverseSquareFalloff)
        {
            GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, deferredShading);
            GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.UseCustom);

            RegenerateFalloff();
        }
        else
        {
            GraphicsSettings.SetCustomShader(BuiltinShaderType.DeferredShading, Shader.Find("Hidden/Internal-DeferredShading"));
            GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.Disabled);
            GraphicsSettings.SetShaderMode(BuiltinShaderType.DeferredShading, BuiltinShaderMode.UseBuiltin);
        }


        AssetDatabase.Refresh();
    }
    void OnEnable()
    {
        RegenerateFalloff();
    }
    void Start()
    {
        RegenerateFalloff();
    }
    void OnScriptsReloaded()
    {
        RegenerateFalloff();
    }

    void RegenerateFalloff()
    {
        GraphicsSettings.lightsUseLinearIntensity = true;
        GraphicsSettings.lightsUseColorTemperature = true;


        // True Attenuation
        int pixelCount = 16;
        Texture2D m_AttenTex = new Texture2D(pixelCount, 1, TextureFormat.ARGB32, false, true);
        m_AttenTex.filterMode = FilterMode.Bilinear;
        m_AttenTex.wrapMode = TextureWrapMode.Clamp;
        Color[] pixels = new Color[pixelCount * pixelCount];
        Vector2 center = new Vector2(0, pixelCount / 2);
        int blackLimit = pixelCount - 1;
        int maxDistance = 10;

        for (int i = 1; i <= pixelCount; i++)
        {
            float v;

            if (i < blackLimit)
            {
                float normalizedIntensity = (float)i / blackLimit;
                float linearIntensity = normalizedIntensity * maxDistance;
                v = 1.0f / (linearIntensity * linearIntensity);
            }
            else
                v = 0.0f;

            pixels[i - 1] = new Color(v, v, v, v);
        }

        m_AttenTex.SetPixels(pixels);
        m_AttenTex.Apply();
        Shader.SetGlobalTexture("_LightTexturePoint", m_AttenTex);
        Shader.SetGlobalTexture("_LightTextureSpot", m_AttenTex);

    }
}