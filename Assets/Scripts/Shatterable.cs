using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Shatterable : MonoBehaviour
{
    public Sprite originalSprite;
    float explosionForce = 10f;
    float explosionRadius = 5f;
    int gridX = 10;
    int gridY = 10;
    public float fadeOutDuration = 2f;

    public void Shatter()
    {
        Subdivide();
        ApplyExplosionForce();
    }
    
    void Subdivide()
    {
        for (int x = 0; x < gridX; x++)
        {
            for (int y = 0; y < gridY; y++)
            {
                CreateSubSprite(x, y);
            }
        }
    }

    void CreateSubSprite(int x, int y)
    {
        GameObject subSpriteObj = new GameObject($"SubSprite_{x}_{y}");
        subSpriteObj.transform.SetParent(transform, false); // Set parent without keeping world position

        // Calculate and set the local position
        float pieceWidth = originalSprite.bounds.size.x / gridX;
        float pieceHeight = originalSprite.bounds.size.y / gridY;
        Vector3 localPos = new Vector3(pieceWidth * (x + 0.5f) - originalSprite.bounds.size.x / 2, 
                                       pieceHeight * (y + 0.5f) - originalSprite.bounds.size.y / 2, 0);
        subSpriteObj.transform.localPosition = localPos;

        // Rigidbody and MeshCollider setup
        Rigidbody rigidBody = subSpriteObj.AddComponent<Rigidbody>();
        rigidBody.mass = 0.1f;
        MeshCollider meshCollider = subSpriteObj.AddComponent<MeshCollider>();
        meshCollider.convex = true;

        // MeshRenderer with material setup
        MeshRenderer meshRenderer = subSpriteObj.AddComponent<MeshRenderer>();
        Material spriteMaterial = new Material(Shader.Find("Sprites/Default"));
        spriteMaterial.mainTexture = originalSprite.texture;
        meshRenderer.material = spriteMaterial;
        // MeshFilter and Mesh setup
        MeshFilter meshFilter = subSpriteObj.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
        
        
        // Setup mesh vertices, triangles, and UVs
        Vector3[] vertices = {
            new Vector3(0, 0, 0),
            new Vector3(0, pieceHeight, 0),
            new Vector3(pieceWidth, pieceHeight, 0),
            new Vector3(pieceWidth, 0, 0)
        };
        int[] triangles = { 0, 1, 2, 2, 3, 0 };
        Vector2[] uv = {
            new Vector2((float)x / gridX, (float)y / gridY),
            new Vector2((float)x / gridX, (float)(y + 1) / gridY),
            new Vector2((float)(x + 1) / gridX, (float)(y + 1) / gridY),
            new Vector2((float)(x + 1) / gridX, (float)y / gridY)
        };
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh; 
        StartCoroutine(FadeOut(subSpriteObj, fadeOutDuration)); // Start the fade-out coroutine
    }
        
    IEnumerator FadeOut(GameObject obj, float duration)
    {
        Material mat = obj.GetComponent<MeshRenderer>().material;
        float currentTime = 0;
        
        while (currentTime < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, currentTime / duration);
            Color newColor = mat.color;
            newColor.a = alpha;
            mat.color = newColor;
            
            currentTime += Time.deltaTime;
            yield return null;
        }

        Destroy(obj); // Destroy the object after fading out
    }
    
    void ApplyExplosionForce()
    {
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 positionInFront = transform.position + new Vector3(0, 0, -0.5f);
                rb.AddExplosionForce(explosionForce, positionInFront, explosionRadius);
            }
        }
    }
}
