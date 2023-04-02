using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorTransitionTest : MonoBehaviour
{
    [SerializeField] private Color color1;
    [SerializeField] private Color color2;
    [SerializeField] private List<Image> images;

    void Start()
    {
        int count = 0;
        var stepSize = 1.0f / 9; 
        foreach (var image in images)
        {
            image.color = Color.Lerp(color1, color2, stepSize * count);
            count++;
        }
    }
}
