//This Script just joins the value of the silders to the correct blur and lightness values in the materials.
//this script can help you design your own control script for your game.

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace GaussianBlur_RenderBlur
{

    public class DemoSliderControl : MonoBehaviour
    {

        public Text MaxBlurText;
        public Slider MaxBlur;

        public Text QualityText;
        public Slider Quality;

        public Text DownResText;
        public Slider DownRes;
        
        public Text BlurSizeText;
        public Slider BlurSize;
        
        public Text LightnessText;
        public Slider Lightness;

        public BlurRenderer BR;

        public Material BlurMaterial;

        void Start()
        {
            BR = BlurRenderer.instance;
            BR.RenderBlur(MaxBlur.value, (int)Quality.value, 1, (int)DownRes.value, true);
        }

        // Update is called once per frame
        void Update() 
        {

            QualityText.text = "Quality: " + Quality.value.ToString("");
            DownResText.text = "DownRes: " + DownRes.value.ToString("");
            MaxBlurText.text = "MaxBlur: " + MaxBlur.value.ToString("F3");

            BlurSizeText.text = "BlurSize: " + BlurSize.value.ToString("F3");
            LightnessText.text = "Lightness: " + Lightness.value.ToString("F3");

            BlurMaterial.SetFloat("_BlurSize", BlurSize.value);
            BlurMaterial.SetFloat("_Lightness", Lightness.value);
            
        }

        public void ReRender()
        {
            BR.RenderBlur(MaxBlur.value, (int)Quality.value, 1, (int)DownRes.value, true);
        }
        
    }
}
