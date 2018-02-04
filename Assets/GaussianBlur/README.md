 GaussianBlur
-------------------------------------
[Asset Store Link](http://u3d.as/yJk)  
© 2017 Justin Garza

PLEASE LEAVE A REVIEW OR RATE THE PACKAGE IF YOU FIND IT USEFUL!
Enjoy! :)

## Table of Contents

* [Contact](#Contact)
* [Description/Features](#Description-Features)
* [Terms of Use](#Terms-of-Use)
* [How to Use](#How-to-Use)
	* [Preface](#Preface)
	* [GaussianBlur_Live](#GaussianBlur_Live)
		* [Create Material](#Create-Material)
		* [Use Material](#Use-Material)
		* [Change Material via Script](#Change-Material-via-Script)
		* [Troubleshooting](#Troubleshooting)
		  * [Edge Smear](#Edge-Smear)
		  * [It's Black's](#It's-Black)
		  * [It's Blocky](#It's-Blocky)
		  * [I've got a ton of Errors!](#I've-got-a-ton-of-Errors!)
	* [GaussianBlur_RenderBlur](#GaussianBlur_RenderBlur)
		* [Demo Review](#Demo-Review)
		* [BlurRenderer.cs](#BlurRenderer.cs)
		* [GaussianBlur_RenderBlur.shader](#GaussianBlur_RenderBlur.shader)
		* [Troubleshooting](#Troubleshooting)
		  * [It's Blocky](#It's-Blocky)
		  * [I've got a ton of Errors!](#I've-got-a-ton-of-Errors!)
		  * [Doesn't Do WorldSpace](#Doesn't-Do-WorldSpace)
		  * [Jumpy Update](#Jumpy-Update)
## Contact

Questions, suggestions, help needed?  
Contact me at:  
Email: jgarza9788@gmail.com  
Cell: 1-818-251-0647  
Contact Info: [justingarza.net/contact](http://justingarza.net/contact/)

## Description Features

A GaussianBlur effect for UI Components.

* Adjust Blur and Lightness using C# or JS
* Add alpha mask for different shapes!
* Adjust Quality for mobile/low-end hardware
* Unity Free friendly.
* Fully commented C# code.
* Awesome demos!

## Terms of Use

You are free to add this asset to any game you’d like
However:  
please put my name in the credits, or in the special thanks section.  
please do not re-distribute.  

## How To Use

### Preface

This asset is in fact 2 assets.  

**GaussianBlur_LiveBlur** (folder)  
Contains a Gaussian Blur shader(s) that updates on every frame.  
This asset has been tested on the following devices.  

*	IOS iPhone6s  
*	Android Nexus9 [Specs](https://www.androidcentral.com/nexus-9-specs)
*	OSX Macbook Pro  
*	Windows7(bootcamp) Macbook Pro  
*	Windows 10 (GTX 1080)  

**GaussianBlur_RenderBlur** (folder)  
Contains a Gaussian Blur shader(s), and script(s), that updates global textures when called on.
Call this "Render on Demand". This can be used for low-end devices when you don't need to update the blur every frame, or if you just want to be battery friendly.


### GaussianBlur_Live
#### Create Material
Create a new material, name it as needed.  
Assign the GaussianBlur shader to it.

>**WARNING!**  
Use the GaussianBlur_LiveBlur.shader or GaussianBlur_LiveBlur_NoSmear.shader. GaussianBlurV1 and GaussianBlurV2 will be deleted in the next update.

![Imgur](http://i.imgur.com/qoy3uyxm.png)

#### Use Material
Assign the new material to an image within your canvas.

![Imgur](http://i.imgur.com/XIshcrMm.png)

Note: if you used GaussianBlur_LiveBlur_NoSmear.shader make sure to add SyncCoordinates.cs to the same image in your canvas.

#### Change Material via Script
This Shader has 5 properties.  

1. _Color  
 * This is the Tint Color of the shader.
 * For best uses maintain a low alpha.  
2. _BlurSize  
 * Amount of Blur
3. _Lightness  
 * How light/dark the material should be.
4. _Quality  
 * the Quality of the blur.
 * Use low number for mobile/low-end devices


**Examples:**

~~~cs  
//set the color
ScreenBlurLayer.SetColor("_Color",Color.red);
        
//set the BlurSize
ScreenBlurLayer.SetFloat("_BlurSize",30f);
     
//Set the Lightness   
ScreenBlurLayer.SetFloat("_Lightness",0.2f);

//Set the Quality
ScreenBlurLayer.SetFloat("_Quality",4.0f);

~~~
 

Please see the DemoSliderControl.cs script for more information.

For more information about materials please see
https://docs.unity3d.com/ScriptReference/Material.html


#### Troubleshooting

##### What is edge smear?   
Edge Smear is the name of an issue where objects outside of the UI get smeared in from the edge and effect the blur. 

Edge Smear only occurs if you decided to use the GaussianBlur_LiveBlur.shader.  
![Imgur](http://i.imgur.com/OGPs9vFm.png)

To stop edge smear please use GaussianBlur_LiveBlur_NoSmear.shader.
It doesn't suffer from the Edge Smear, however it needs more information to render properly. (see SyncCoordinates.cs for more info) 

![Imgur](http://i.imgur.com/kwOaR5Gm.png)

##### It's Black?
If GaussianBlur_LiveBlur_NoSmear.shader is being used and the values like ScreenWidth, ScreenHeight, PanelWidth, PanelHeight..etc are not correct than the shader might render as black.

##### It's Blocky?  
If the Shader seems blocky (like in the image below), increase the Quality value in the shader.

![Imgur](http://i.imgur.com/5xclyZ4m.png)

##### I've got a ton of Errors!
The Shaders written in Unity get recompiled into 8 different shaders for each rendering engine. Unfortunately  this shader doesn't work with DirectX9 (D3D9), therefore please disable it in the inspector.

![Imgur](http://i.imgur.com/weoBIhh.png)

If compiling for windows you can choose the graphical API in PlayerSettings
![Imgur](http://i.imgur.com/V909vba.png)

*If need this to work with DirectX9 please let me know, so i can make that more of a priority.*

### GaussianBlur_RenderBlur
#### Demo Review

Please open the Demo.unity file located in
...Assets\GaussianBlur\GaussianBlur_RenderBlur\Demo\Scenes\

When the Scene is played the following events will occur.
1. Demo Control Slider GameObject will create BlurRenderer GameObject.
2. BlurRenderer will take an image(s) of the screen, blur them, and save them as global textures (any shader can use them).
3. UI GameObjects using the GaussianBlur_RenderBlur.shader will display the portion of the blurred images based on where they are on the screen.

![Imgur](http://i.imgur.com/xUszo6a.png)

#### BlurRenderer.cs

Below is a ScreenShot of the BlurRenderer.cs Script.

**MaxBlur**: the maxium blur used to make the global textures.  
**Quality**: the quality of the blur (the lower the better for mobile)  
**TextureCnt**: the number of global textures generated (5 max)  
**DownRes**: lowering the resolution of the image that is captured from the camera. (the highter the better for mobile).  
**RenderNow**: a trigger mostly used for debugging in the unity editor.  

![Imgur](http://i.imgur.com/QqZl5No.png)

#### GaussianBlur_RenderBlur.shader

Uses properties and the global textures to generate a blurred effect on the screen.

**_Lightnesss**: multiplies the color of the image to make it lighter, or darker.  
**_BlurSize**: A number between 0 and 1, in order to blender between each of the global textures (_Blur#).  
**_AlphaAsBlurSize**: Alpha in your image will be used to calculate the BlurSize.  

![Imgur](http://i.imgur.com/8h4rLRm.png)

Below is a diagram to convery how BlurSize is calculated.

* If BlurRenderer generated 5 textures.
	* If your BlurSize is 1, then _Blur4 will be used.
	* If your BlurSize is 0.5 then the shader will use a mix of _Blur1 and _Blur2.
	* If your BlurSize is 0.1 then the shader will used _Blur0 with an alpha of 0.5.

![Imgur](http://i.imgur.com/Tlrsep0m.png)


#### Troubleshooting

##### It's Blocky?  
If the Shader seems blocky (like in the image below), increase the Quality value in the shader.

![Imgur](http://i.imgur.com/5xclyZ4m.png)

##### I've got a ton of Errors!

The Shaders written in Unity get recompiled into 8 different shaders for each rendering engine. Unfortunately  this shader doesn't work with DirectX9 (D3D9), therefore please disable it in the inspector.

![Imgur](http://i.imgur.com/weoBIhh.png)

If compiling for windows you can choose the graphical API in PlayerSettings
![Imgur](http://i.imgur.com/V909vba.png)

##### Doesn't Do WorldSpace

Sorry, but this method doesn't support WorldSpace canvasses.
*Please let me know if you want this and I will do my best.*

##### Jumpy Update

The blur effect will seem to jump a bit when the texture(s) are updated. (i.e. not a smooth update)
This is something i will be working on in the next few months.
*Let me know if this is a priotity*

>**WARNING!**  
In order to accomplish a fix for this this i might need to render more textures