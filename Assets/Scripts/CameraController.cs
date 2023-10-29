using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float cameraBuffer;
    [SerializeField] private float cameraYOffset;
    [SerializeField] private float zoomCameraOrthographicSize;
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
        var center = newCameraBounds.center + new Vector3(0, cameraYOffset, -10);
        
        _camera.transform.position = center;
        _camera.orthographicSize = size;
    }

    public void ZoomCameraToTilePosition(Vector3 tilePosition, float zoomTime)
    {
        var newCameraPosition = tilePosition + new Vector3(0, cameraYOffset, -10);
        var cameraStartPosition = transform.position;
        var cameraStartOrthographicSize = _camera.orthographicSize;
        DOVirtual.Float(0, 1, zoomTime, t =>
        {
            transform.position = Vector3.Lerp(cameraStartPosition, newCameraPosition, t);
            _camera.orthographicSize = Mathf.Lerp(cameraStartOrthographicSize, zoomCameraOrthographicSize, t);
        });
    }
}
