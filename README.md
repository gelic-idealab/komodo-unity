[![openupm](https://img.shields.io/npm/v/com.graingeridealab.komodo?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.graingeridealab.komodo/)

## Installation

**Install Unity.** If it’s not already installed, install Unity version 2020.x. Make sure to check the box for WebGL Build Support.

**Create a WebGL Project.** Create a new Unity project, version 2020.x, type 3D, name `[YourProjectName]`. Choose File > Build Settings. Choose `WebGL` under `Platform`. Select `Switch Platform`.

**Add Scoped Registries to `manifest.json`.** In File Explorer, open `[YourProjectName] > Packages > manifest.json`.

Edit the file to add the following two scoped registries: 

    { // this is the beginning of manifest.json  
      "scopedRegistries": [
        {
          "name": "Packages from jillejr",
          "url": "https://npm.cloudsmith.io/jillejr/newtonsoft-json-for-unity/",
          "scopes": [
            "jillejr"
          ]
        },
        {
          "name": "OpenUPM",
          "url": "https://package.openupm.com",
          "scopes": [
            "com.de-panther",
            "com.atteneder",
            "com.graingeridealab"
          ]
        }
      ],
      "dependencies": {
        //... (these are dependencies that are already in the file)
      }
    } // this is the end of manifest.json

JSON does not support comments, so take out all  `//` and everything after them. [1]

Save the file and return to the Unity Editor. Unity should alert you that two new scoped registries are available. [2]

**Install KomodoCore.** In the Unity Editor main window, choose `Window > Package Manager`. Click the  `[ Packages: In Project v ]` (the second dropdown) in the upper left. Look for the entry GELIC-IDEALab > KomodoCore. Select `Install`. Wait for the package to be loaded, then skip to “Add Scenes.” This method of installing KomodoCore uses OpenUPM, but there are two alternative installation methods. [3]

**Copy WebGLTemplates.** Select Window > Komodo > Copy WebGLTemplates. In the Project window, there should now be a folder under Assets titled “WebGLTemplates.” 

**Add Scenes.** Go to File > Build Settings. Add `Packages/KomodoCore/KomodoCoreAssets/Scenes/Main` and `Packages/KomodoCore/KomodoCoreAssets/Scenes/Outdoors/Outdoors` to `Scenes in Build`.

**Select the Komodo WebXR Template.** While still in the `Build Settings` window, choose `Player Settings…`. Make sure the tab selected is the WebGL icon. Expand the `Resolution and Presentation` panel. Select `KomodoWebXRFullView` in the WebGL templates option. [4]

**Test the project.** Press `[ > ]` (the play button at the center top of the Unity Editor). Check the console for errors. If it runs without errors, congratulations!

**Develop a module with KomodoCore**. Follow the Guide to Developing Modules with Komodo. [6]


##  Footnotes

1. If for some reason the above is not up-to-date, the latest version of the scoped registries can always be found in the development repository — `komodo-unity/Komodo/Packages/manifest.json`.

2. If not, go to `Edit > Project Settings > Package Manager > Scoped Registries` and check to see that `Packages from jillejr` and `OpenUPM` are listed and reflect the structure of `manifest.json`.

3. Alternative install methods: 

    1. If you have downloaded the Komodo package manually
    Click `[` `+` `v ]` (the plus-button dropdown) in the upper left. Choose `Add package from disk…` Find and select `[KomodoPackageLocation]/KomodoCore/package.json`. Wait for the package to be loaded, then skip to “Add Scenes.”
    
    1. If you have downloaded or cloned the komodo-unity repository
    Click `[` `+` `v ]` (the plus-button dropdown) in the upper left. Choose `Add package from disk…`. Find and select `komodo-unity/Komodo/Assets/Packages/KomodoCore/package.json`. Wait for the package to be loaded, then skip to “Add Scenes.”

4. If you only see `Default` and `Minimal` as options, the WebGLTemplates folder did not copy properly. 

5. TODO — add Unity manual link here. 

6. TODO — add link to this guide, which will detail how to perform Unity-level changes, Configuration- and Extension-level changes, and Package-level changes to the Komodo project. 
