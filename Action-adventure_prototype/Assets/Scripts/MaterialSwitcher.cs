using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSwitcher : MonoBehaviour
{
    [SerializeField] private Material _materialToApply;
    [SerializeField] private Material[] _originalMaterials;
    [SerializeField] private MeshRenderer[] _meshRenderers;
    [SerializeField] private bool _isMaterialApplied;

    private void Awake()
    {
        _meshRenderers = GetComponentsInChildren<MeshRenderer>(true);
        if(_meshRenderers != null)
        {
            _originalMaterials = new Material[_meshRenderers.Length];
            for (int i = 0; i < _meshRenderers.Length; i++)
            {
                _originalMaterials[i] = _meshRenderers[i].material;
            }
        }

        _isMaterialApplied = false;
    }

    public void ApplyMaterial()
    {
        if (_meshRenderers == null) return;

        for (int i = 0; i < _meshRenderers.Length; i++)
        {
            _meshRenderers[i].material = _isMaterialApplied ? _originalMaterials[i] : _materialToApply;
        }

        _isMaterialApplied = !_isMaterialApplied;
    }

}
