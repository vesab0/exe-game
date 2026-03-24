using UnityEngine;

public class CRTPostProcess : MonoBehaviour
{
    public Material crtMaterial;
    private MeshFilter mf;
    private MeshRenderer mr;

    void Start()
    {
        GameObject quad = new GameObject("CRTQuad");
        quad.transform.parent = transform;

        mf = quad.AddComponent<MeshFilter>();
        mr = quad.AddComponent<MeshRenderer>();

        mf.mesh = CreateFullscreenMesh();
        mr.material = crtMaterial;
        mr.material.renderQueue = 5000;

        // Position the quad just in front of the camera
        quad.transform.localPosition = new Vector3(0, 0, 1f);
        quad.transform.localRotation = Quaternion.identity;

        // Scale it to fill the camera view
        Camera cam = GetComponent<Camera>();
        float h = 2f * cam.orthographicSize;
        float w = h * cam.aspect;
        quad.transform.localScale = new Vector3(w, h, 1f);

        // Make sure it doesn't cast shadows
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.receiveShadows = false;
    }

    Mesh CreateFullscreenMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[]
        {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0),
        };
        mesh.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
        };
        mesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
        return mesh;
    }
}