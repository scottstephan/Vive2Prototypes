using UnityEngine;
using System.Collections;

public class TextOutline : MonoBehaviour {
    public Color outlineColor = Color.black;
    public float outlineOffset = 1f;

    private TextMesh textMesh;
    private MeshRenderer meshRenderer;

    Transform myTransform;
    TextMesh[] outlineTextMeshes;

    void Start() 
    {
        myTransform = transform;
        textMesh = GetComponent<TextMesh>();    
        meshRenderer = GetComponent<MeshRenderer>();

        outlineTextMeshes = new TextMesh[8];
        outlineColor.a = textMesh.color.a * textMesh.color.a;

        string text = textMesh.text;
        for (int i = 0; i < 8; i++) 
        {
            GameObject outline = new GameObject("outline", typeof(TextMesh));
            outline.transform.parent = myTransform;
            outline.transform.localScale = new Vector3(1, 1, 1);
            outline.transform.localRotation = Quaternion.identity;

            TextMesh otherTextMesh = outline.GetComponent<TextMesh>();
            outlineTextMeshes[i] = otherTextMesh;
            otherTextMesh.text = text;
            otherTextMesh.color = outlineColor;
            otherTextMesh.alignment = textMesh.alignment;
            otherTextMesh.anchor = textMesh.anchor;
            otherTextMesh.characterSize = textMesh.characterSize;
            otherTextMesh.font = textMesh.font;
            otherTextMesh.fontSize = textMesh.fontSize;
            otherTextMesh.fontStyle = textMesh.fontStyle;
            otherTextMesh.richText = textMesh.richText;
            otherTextMesh.tabSize = textMesh.tabSize;
            otherTextMesh.lineSpacing = textMesh.lineSpacing;
            otherTextMesh.offsetZ = textMesh.offsetZ;

            MeshRenderer outlineMeshRenderer = outline.GetComponent<MeshRenderer>();
            outlineMeshRenderer.material = new Material(meshRenderer.material);
            outlineMeshRenderer.castShadows = false;
            outlineMeshRenderer.receiveShadows = false;
            outlineMeshRenderer.sortingLayerID = meshRenderer.sortingLayerID;
            outlineMeshRenderer.sortingLayerName = meshRenderer.sortingLayerName;
        }
    }

    public void UpdateText()
    {
        if (textMesh != null && outlineTextMeshes != null)
        {
            for (int i = 0; i < outlineTextMeshes.Length; i++) 
            {           
                TextMesh other = outlineTextMeshes[i];
                other.text = textMesh.text;
            }
        }
    }

    void LateUpdate() 
    {
        Vector3 pos = myTransform.position;
        Vector3 back = myTransform.forward * outlineOffset;
        Vector3 up = myTransform.up;
        Vector3 right = myTransform.right;

        // copy attributes
        for (int i = 0; i < outlineTextMeshes.Length; i++) 
        {           
            TextMesh other = outlineTextMeshes[i];
            Vector3 worldPoint = pos + (GetOffset(i, up, right) * outlineOffset) + back;
            other.transform.position = worldPoint;
        }
    }
    
    Vector3 GetOffset(int i, Vector3 up, Vector3 right) 
    {
        switch (i % 8) 
        {
        case 0: return up;
        case 1: return right + up;
        case 2: return right;
        case 3: return right - up;
        case 4: return -up;
        case 5: return -right - up;
        case 6: return -right;
        case 7: return -right + up;
        default: return Vector3.zero;
        }
    }
}
