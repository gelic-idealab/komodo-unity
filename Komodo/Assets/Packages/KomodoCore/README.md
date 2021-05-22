Change relay.js so that API_BASE_URL and RELAY_BASE_URL point to your own komodo-portal and komodo-relay deployments.

If you are developing on the University of Illinois campus, please contact idealab@library.illinois.edu to request access to the Komodo development server connection credentials.

# Attributions

Komodo Astronaut is remixed from <a href="https://poly.google.com/view/dLHpzNdygsg">Astronaut</a> by <a href="https://poly.google.com/user/4aEd8rQgKu2">Poly by Google</a> and licensed under <a href="https://creativecommons.org/licenses/by/3.0/legalcode">CC-BY 3.0.</a>
        
<a href="https://assetstore.unity.com/packages/3d/vegetation/trees/low-poly-trees-pack-73954#description">Low Poly Trees - Pack</a> by <a href="https://www.lmhpoly.com/">LMHPOLY</a> is licensed under <a href="https://www.mediafire.com/file/ejr7fqerjftpfyt/License.pdf/file">a custom license</a>.

___
*The rest of these instructions are partially correct and incomplete as of 2021-05-07. Our apologies.*
___

## Installing the komodo_unity Core

**Clone the repository.** 

Open a bash terminal and run `git clone https://gitlab.engr.illinois.edu/dev-studio/komodo/komodo_unity`, or download a release ZIP from the [Releases](https://gitlab.engr.illinois.edu/dev-studio/komodo/komodo_unity/-/releases/) page. 

**Add the project to your Unity Hub and open it.**

Open Unity Hub. Make sure the Projects tab is open. At the top right of the screen, press “Add.” When the folder selection window opens, choose the  `… > komodo_unity > Komodo` folder. Click the dropdown to choose a variant of Unity 2018.4 LTS, or install one if you don’t have 2018.4. Then click the project name to open it. 

**Download and open the main scene.** 

Download the main scene from the same page that you got the release from, or visit the [Komodo Releases folder](https://uofi.box.com/s/gsrtdj8bfyxet3gssnefif8d30cpvpk6) and download vX.Y.Z.unity

Import the scene file into the `… > Assets > Scenes` folder with the name “<name>” by drag-and-dropping it into Unity or right-clicking inside the `Scenes` folder and choosing `Import New Asset…`. Double-click to open it, or use File > Open Scene.

**Test your installation in the editor.**

Enter Play Mode by pressing the play button at the center top of the Unity window. 

Open the Console to make sure there are no warnings or errors. You may need to use the filters to enable the display of warnings and errors. 
Learn how to Enable VR Support in Play Mode. [1] 

You may develop for Komodo without using VR in Play Mode, but you will need to build each time you need to test VR-specific interactions.

**[1]** **Enable VR Support in Play Mode**

For convenience, the Komodo SDK supports VR in Unity Play Mode. This allows you to test non-networked interaction without building, as long as you have a VR headset that functions. 

You will need the appropriate SDK to use Unity with the headset, which is usually provided by the headset manufacturer. 

Currently, only the Oculus Rift CV1, Rift S, and Quest 1 and 2 (via Oculus Link) are officially supported. And only Oculus Touch v2 inputs are fully supported by the unmodified SDK, but you may provide your own input mapping. [3]

Install the [Oculus Desktop](https://docs.unity3d.com/Packages/com.unity.xr.oculus.standalone@1.38/manual/index.html) package in Unity. 
Ensure that your Oculus headset is set up and that the Oculus app is running on your PC.

## Running komodo_unity in the Browser (Non-networked)

To test with networking, install komodo_relay and read “Running komodo_unity with komodo_relay”. 

**Test your installation in the browser by building.**

Make sure you are following our recommended project settings. [2]

If you have not made any modifications to the scene, press Build and Run. 

Read more about using Komodo with our provided host-it-yourself relay server. [4]

**Serve the contents of the build folder**

This method requires having a Bash terminal and having NodeJS installed and in your path. 

`cd …/<build-folder-name>/`

`npx serve .`

**Connect to the build folder in the browser** 

Open a browser compatible with WebXR and your headset runtime. 

Go to `localhost:5000`.

Wait for the page to load

Press the goggles button to enter VR.

**[2]** **Recommended Project Settings**

Coming soon. 

**[3]** **Custom Input Mappings**

Coming soon.

## Making Basic Modifications to `komodo_unity`

Out-of-the-Box Features

(TODO: “If using Komodo core, you can make modifications to everything EXCEPT…”)

Coming soon. 

Modifying the Base Scene

Coming soon. 

Adding More Scenes

Coming soon.

## Developing Modules with `komodo_unity`

Networking Components

Coming soon. 

Custom Interactions: Simple Example

Coming soon. 

Custom Interactions: Complex Example

Coming soon. 

See also: Modifying Scenes, Adding More Scenes

## Further Reading

To test with networking, install komodo_relay and read “Running komodo_unity with komodo_relay”. 
