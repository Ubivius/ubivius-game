using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    //TODO ajouter un layerMask pour ne pas que les ennemies bloquent le champ de vision 
    private Mesh m_mesh;
    private Vector3 m_origin;
    private float m_startingAngle;
    private float m_fov;
    private float m_viewDistance;
    private void Start()
    {
        m_mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = m_mesh;
        m_origin = Vector3.zero;
        m_fov = 90f; //FOV angle
        m_viewDistance = 10f;//FOV lenght
    }

    private void LateUpdate()
    {
        int rayCount = 1000; //more rays = cpu expensive 
        float angle = m_startingAngle;
        float angleInscrease = m_fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount +1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = m_origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++) {
            Vector3 vertex;
            RaycastHit2D raycastHit2D = Physics2D.Raycast(m_origin, GetVectorFromAngle(angle), m_viewDistance);
            if (raycastHit2D.collider == null)
            {
                //not hit
                vertex = m_origin + GetVectorFromAngle(angle) * m_viewDistance;
            }
            else {
                //hit object
                vertex = raycastHit2D.point;
            }
            vertices[vertexIndex] = vertex;

            if (i > 0) {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }
            vertexIndex++;
            angle -= angleInscrease;
        }

        m_mesh.vertices = vertices;
        m_mesh.uv = uv;
        m_mesh.triangles = triangles;
        m_mesh.bounds = new Bounds(m_origin, Vector3.one * 1000f); //modify according to the map area
    }

    public static Vector3 GetVectorFromAngle(float angle) {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    public static float GetAngleFromVectorFloat(Vector3 dir)
    {
        dir = dir.normalized;
        float n = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (n < 0) n += 360;
        return n;
    }

    public void SetOrigin(Vector3 origin) {
        this.m_origin = origin;
    }

    public void SetAimDirection(Vector3 aimDirection) {
        m_startingAngle = GetAngleFromVectorFloat(aimDirection) + m_fov / 2f;
    }

    public void SetFov(float fov) {
        this.m_fov = fov;
    }

    public void SetViewDistance(float viewDistance) {
        this.m_viewDistance = viewDistance;
    }
}
