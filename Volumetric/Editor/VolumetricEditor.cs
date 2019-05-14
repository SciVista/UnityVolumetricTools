using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Linq;

public class VolumetricEditor : OdinEditorWindow
{
    [InlineEditor(InlineEditorModes.LargePreview)]
    public Texture3D VolumetricTexutre;

    public GameObject target;

    public Vector3Int Size;
    MaterialPropertyBlock mpb = new MaterialPropertyBlock();

    public Func<Vector3, Color> generator;

    [MenuItem("SciVista/Volumetric Editor")]
    private static void OpenWindow()
    {
        GetWindow<VolumetricEditor>().Show();
    }
    [PropertyOrder(-10)]
    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void GenerateTexture()
    {

        VolumetricTools.Create3dTexture(Size, generator);
    }

    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void SaveTexture() { }

    [HorizontalGroup]
    [Button(ButtonSizes.Large)]
    public void TestJobTexture()
    {
        float totalCount = Size.x * Size.y * Size.z;
        if (mpb == null) mpb = new MaterialPropertyBlock();
        float pct = 0;
        float[] values = new float[(int)totalCount];
        var mr = target.GetComponent<MeshRenderer>();
        mr.GetPropertyBlock(mpb);
        for(int i = 0; i < totalCount; i++)
        {
            pct = (float)(i) / totalCount;
            values[i] = pct;
        }
        Action<Texture3D> resultCallback = (tex) =>
        {
            Debug.Log("Result Callback");
            VolumetricTexutre = tex;
            mpb.SetTexture("_Data", tex);
            mr.SetPropertyBlock(mpb);
#if UNITY_EDITOR
            AssetDatabase.CreateAsset(tex, "Assets/auto3dTex.asset");
#endif 
        };
        Debug.Log("starting Texture job test");
        IEnumerator coroutine = VolumetricTools.CreateVolumetricTexture(values, Size, resultCallback);
        target.GetComponent<CoroutineCaller>().StartCoroutine(coroutine);
    }

}

