using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UGUIMiniMap
{
    public class bl_MiniMapPlane : MonoBehaviour
    {
        public GameObject mapPlane;
        public GameObject gridPlane;
        public Material planeMaterial, planeMobileMaterial;

        private Transform m_Transform;
        private bl_MiniMap m_Minimap;
        private Vector3 currentPosition;
        private Vector3 worldPosition;
        private float defaultYCameraPosition;

        /// <summary>
        /// 
        /// </summary>
        public void Setup(bl_MiniMap minimap)
        {
            m_Transform = transform;
            m_Minimap = minimap;
            //Get Position reference from world space rect.
            Vector3 pos = minimap.WorldSpace.position;
            //Get Size reference from world space rect.
            Vector3 size = minimap.WorldSpace.sizeDelta;
            //Set to camera culling only MiniMap Layer.
            minimap.MinimapCamera.cullingMask = 1 << minimap.MiniMapLayer;
            //Set position
            m_Transform.localPosition = pos;
            //Set Correct size.
            m_Transform.localScale = (new Vector3(size.x, 10, size.y) / 10);
            //Apply material with map texture.
            PlaneRender.material = CreateMaterial();
            worldPosition = minimap.WorldSpace.position;
            //Apply MiniMap Layer
            mapPlane.layer = minimap.MiniMapLayer;
            gridPlane.layer = minimap.MiniMapLayer;
            gameObject.hideFlags = HideFlags.HideInHierarchy;
            mapPlane.SetActive(minimap.m_Type == bl_MiniMap.RenderType.Picture);
            gridPlane.SetActive(minimap.ShowAreaGrid);
            if (minimap.ShowAreaGrid)
            {
                gridPlane.GetComponent<Renderer>().material.SetTextureScale("_MainTex", Vector2.one * minimap.AreasSize);
            }
            Invoke(nameof(DelayPositionInvoke), 1);
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnUpdate()
        {
            currentPosition = m_Transform.localPosition;
            //Get Position reference from world space rect.
            float ydif = defaultYCameraPosition - m_Minimap.MinimapCamera.transform.position.y;
            currentPosition.y = currentPosition.y - ydif;
            m_Transform.position = currentPosition;
        }

        void DelayPositionInvoke() { defaultYCameraPosition = m_Minimap.MinimapCamera.transform.position.y;}

        /// <summary>
        /// 
        /// </summary>
        public void SetMapTexture(Texture2D newTexture)
        {
            PlaneRender.material.mainTexture = newTexture;
        }

        public void SetGridSize(float size)
        {
            gridPlane.GetComponent<Renderer>().material.SetTextureScale("_MainTex", Vector2.one * size);
        }

        /// <summary>
        /// Create Material for Minimap image in plane.
        /// you can edit and add your own shader.
        /// </summary>
        /// <returns></returns>
        public Material CreateMaterial()
        {
            Material mat = new Material(m_Minimap.isMobile ? planeMobileMaterial : planeMaterial);

            mat.mainTexture = m_Minimap.MapTexture;
            if (!mat.HasProperty("_EmissionMap")) return mat;

            mat.SetTexture("_EmissionMap", m_Minimap.MapTexture);
            mat.SetFloat("_EmissionScaleUI", m_Minimap.EmissionAmount);
            mat.SetColor("_EmissionColor", m_Minimap.EmessiveColor);
            mat.SetColor("_SpecColor", m_Minimap.SpecularColor);
            mat.EnableKeyword("_EMISSION");

            return mat;
        }

        public Renderer PlaneRender
        {
            get { return mapPlane.GetComponent<Renderer>(); }
        }
    }
}