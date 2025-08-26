using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraShader : MonoBehaviour
{
    public Shader replacementShader;

    Camera _cam;

    void OnEnable()
    {
        _cam = GetComponent<Camera>();
        if (_cam && replacementShader)
            _cam.SetReplacementShader(replacementShader, "");
    }

    void OnDisable()
    {
        if (_cam) _cam.ResetReplacementShader();
    }
}