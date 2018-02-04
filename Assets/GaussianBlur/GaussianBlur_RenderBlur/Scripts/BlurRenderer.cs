/*
BlurRenderer.cs
Creates global textures that have been passed through the GaussianBlur_RenderBlur(Hidden).shader
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]
public class BlurRenderer : MonoBehaviour
{

    #region Variables

    /// <summary>
    /// The Camera this Script is Attached to
    /// </summary>
    private Camera thisCamera;

    /// <summary>
    /// The Maxium Blur we will render at (for best results between 5 and 10)
    /// </summary>
    [Range(0, 20)]
    public float maxBlur = 5f;
    
    /// <summary>
    /// the quality of the blur we will render at (for best results 2)
    /// </summary>
    [Range(0, 3)]
    public int quality = 2;

    /// <summary>
    /// The number of textures we will create (1 minium , 5 maxium)
    /// </summary>
    [Range(1, 5)]
    public int textureCnt = 5;
    
    /// <summary>
    /// the amount we'll down resolution the screen
    /// </summary>
    [Range(0, 3)]
    public int downRes;
    
    /// <summary>
    /// the base name of the global textures we'll be rendering
    /// </summary>
    private string globalBlurName = "_Blur";

    /// <summary>
    /// Just a material we'll use to store the shader
    /// </summary>
    private Material mat;
    
    /// <summary>
    /// indicates the number of OnPreRenders we've executed
    /// </summary>
    private int preRenderInd = 100;

    /// <summary>
    /// indicates the number of OnPostRenders we've executed
    /// </summary>
    private int postRenderInd = 100;

    /// <summary>
    /// indicates if the textures are currently getting rendered
    /// </summary>
    private bool rendering = false;

    /// <summary>
    /// rendering Texture Width
    /// </summary>
    private int rtWidth;

    /// <summary>
    /// rendering Texture Height
    /// </summary>
    private int rtHeight;

    //render testures used to store the image while procesing
    private RenderTexture rt0;
    private RenderTexture rt1;
    private RenderTexture rt2;

    //the texture we'll be using to process the image
    private Texture2D tex;

    /// <summary>
    /// mostly used for debugging in unity...triggers rendering to occur. (not really recommended for during run time)
    /// </summary>
    public bool renderNow = false;

    #endregion

    /// <summary>
    /// LateUpdate is used to debug/test in unity
    /// </summary>
    void LateUpdate()
    {
        if (renderNow)
        {
            RenderBlur(maxBlur,quality,textureCnt,downRes);

            renderNow = false;
        }
    }

    /// <summary>
    /// render blur textures (Override)
    /// </summary>
    /// <param name="MaxBlur"></param>
    /// <param name="Quality"></param>
    /// <param name="TextureCnt"></param>
    /// <param name="DownRes"></param>
    /// <param name="syncWithMainCamera"></param>
    public void RenderBlur(float MaxBlur = 10f, int Quality = 2, int TextureCnt = 5, int DownRes = 2, bool syncWithMainCamera = true)
    {
        //store inputs from the users
        maxBlur = MaxBlur;
        quality = Quality;
        textureCnt = Mathf.Clamp(TextureCnt, 1, 5);
        downRes = DownRes;
        
        RenderBlur(syncWithMainCamera);
    }

    /// <summary>
    /// render blur textures
    /// </summary>
    public void RenderBlur(bool syncWithMainCamera = true)
    {
        //makes sure we have this Camera and it's enabled
        if (thisCamera == null)
        {
            thisCamera = gameObject.GetComponent<Camera>();
            if (thisCamera == null)
            {
                thisCamera = gameObject.AddComponent<Camera>();
            }
        }
        thisCamera.enabled = true;

        //create our material if we have not already
        if (mat == null)
        {
            mat = new Material(Shader.Find("Hidden/GaussianBlur_RenderBlur"));
        }

        //sync with the main camera
        if (syncWithMainCamera)
        {
            SyncWithMainCamera();
        }

        if (rendering)
        {
            //do not allow this method to be called too frquently
        }
        else
        {
            preRenderInd = 0;
            postRenderInd = 0;
            rendering = true;
        }
    }




    public void OnPreRender()
    {
        if (preRenderInd < textureCnt)
        {
            SetUpForRender(preRenderInd);
            preRenderInd++;
        }
    }

    public void OnPostRender()
    {
        if (postRenderInd < textureCnt)
        {
            GenerateTextures(postRenderInd);
            postRenderInd++;
        }
        else
        {
            rendering = false;
        }
    }


    /// <summary>
    /// Used to sync this camera with the main Camera
    /// </summary>
    private void SyncWithMainCamera()
    {
        Camera MC = Camera.main;
        thisCamera.transform.position = MC.transform.position;
        thisCamera.transform.rotation = MC.transform.rotation;
        thisCamera.transform.localScale = MC.transform.localScale;

        thisCamera.clearFlags = MC.clearFlags;
        thisCamera.backgroundColor = MC.backgroundColor;
        thisCamera.cullingMask = MC.cullingMask;
        thisCamera.orthographic = MC.orthographic;
        thisCamera.fieldOfView = MC.fieldOfView;
        thisCamera.farClipPlane = MC.farClipPlane;
        thisCamera.nearClipPlane = MC.nearClipPlane;
        thisCamera.rect = MC.rect;
        thisCamera.depth = -100; //so this camera's output doesn't show on the screen;
        thisCamera.renderingPath = MC.renderingPath;
        //thisCamera.targetTexture = MC.targetTexture; //you don't want to sync this
        thisCamera.useOcclusionCulling = MC.useOcclusionCulling;
        thisCamera.allowHDR = MC.allowHDR;
        thisCamera.allowMSAA = MC.allowMSAA;
        //thisCamera.targetDisplay = thisCamera.targetDisplay; //it doesn't matter if you sync this

        print("Camera Synced");
    }
    


    /// <summary>
    /// set up values for rendering
    /// </summary>
    private void SetUpForRender(int i)
    {
        print("rtWidth: " + rtWidth);
        print("rtHeight: " + rtHeight);

        //get the width and height of our textures
        rtWidth = thisCamera.pixelWidth >> downRes;
        rtHeight = thisCamera.pixelHeight >> downRes;

        print("rtWidth: " + rtWidth);
        print("rtHeight: " + rtHeight);

        //set up some render textures to be used later
        rt0 = new RenderTexture(rtWidth, rtHeight, 16);
        rt1 = new RenderTexture(rtWidth, rtHeight, 16);
        rt2 = new RenderTexture(rtWidth, rtHeight, 16);

        //set up this camera
        thisCamera.targetTexture = rt0;
        thisCamera.targetTexture.filterMode = FilterMode.Bilinear;
        //thisCamera.Render();

        //allow any shader to use the _TextureCnt value
        Shader.SetGlobalFloat("_TextureCnt", (float)textureCnt);

        //set the quality of the Blur
        mat.SetInt("_Quality", quality);

        //set how much blur this texture should have
        float renderBlur = ((float)maxBlur / textureCnt) * (i + 1);
        mat.SetFloat("_BlurSize", renderBlur);
        
        //print("_BlurSize: " + renderBlur);
        //print("i:" + i);
    }

    /// <summary>
    /// Obtains texture/image from camera, blurs them, and allows any shader to use them
    /// </summary>
    private void GenerateTextures(int i)
    {
        //get texture2d from camera
        tex = new Texture2D(rtWidth, rtHeight, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rtWidth, rtHeight), 0, 0);
        tex.Apply();


        //blur the textures (2 Passes)
        Graphics.Blit(tex, rt1, mat, 1);
        Graphics.Blit(rt1, rt2, mat, 0);
        //Graphics.SetRenderTarget(null);

        Shader.SetGlobalTexture(globalBlurName + (i).ToString(),null);
        //allows any shader to use this texture _Blur[x] ( x being a from bwteen 0 and 4)
        Shader.SetGlobalTexture(globalBlurName + (i).ToString(), rt2);

        //print the name of this texture for debuggin'
        print("The " + globalBlurName + (i).ToString() + " can now be used in a shader");

        //clean up!

        rt0 = null;
        rt1 = null;
        rt2 = null;
        thisCamera.targetTexture = null;

        tex = null;

    }


    //semi-Singleton Pattera to easily create a new BlurRenderer
    #region SINGLETON PATTERN
    public static BlurRenderer _instance;
    public static BlurRenderer instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GameObject.FindObjectOfType<BlurRenderer>();

                if (_instance == null)
                {
                    //load from Resources
                    _instance = Instantiate(Resources.Load("BlurRenderer", typeof(BlurRenderer))) as BlurRenderer;
                    _instance.gameObject.name = _instance.gameObject.name.Replace("(Clone)", ""); //removed the "(Clone)" from the name

                }
            }

            return _instance;
        }
    }
    #endregion
}
