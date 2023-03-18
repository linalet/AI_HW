using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailReseter : MonoBehaviour
{
    [SerializeField]
    private TrailRenderer trailRenderer;

    private void OnDisable()
    {
        trailRenderer.Clear();
    }
}
