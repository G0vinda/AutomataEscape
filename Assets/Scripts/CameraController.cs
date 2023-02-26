using System;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float cameraBuffer;
    private Camera _camera;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    public void AlignCameraWithLevel(List<SpriteRenderer> tileRenderers)
    {
        var newCameraBounds = new Bounds();
        
        foreach (var spriteRenderer in tileRenderers)
        {
            newCameraBounds.Encapsulate(spriteRenderer.bounds);
        }
        
        newCameraBounds.Expand(cameraBuffer);
        
        var vertical = newCameraBounds.size.y;
        var horizontal = newCameraBounds.size.x * _camera.pixelHeight / _camera.pixelWidth;
        
        var size = Mathf.Max(horizontal, vertical) * 0.5f;
        var center = newCameraBounds.center + new Vector3(0, 0, -10);
        
        _camera.transform.position = center;
        _camera.orthographicSize = size;
    }
}
