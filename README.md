# REPOSE

A lightweight API library, easily allowing you modify [REPO](https://store.steampowered.com/app/3241660/REPO/)!

Currently, you will need to compile the project in something like Visual Studio 2022. 

However, you can find the [modified injection DLL in the repository](https://github.com/BIGDummyHead/REPOSE/tree/master/REPOSE), simply download the REPO_Data/Managed folder and drop inside of your Root game folder. 

## Creating Mods

Creating mods is quite straightforward, you will simply need to make a new C# project in .Net Standard 2.1 (Very important) and use the API. Reference the API in your dependencies and start using under the namespace ``REPOSE.Mods``

A test mod here:


```cs
using REPOSE.Mods;
using UnityEngine;

namespace TestMod
{
    public class TestMod : Mod
    {
        public CustomGameObject cGO = null;
        public GameObject go = null;

        //called on initializations (Scene loads), Main Menu, Lobby Menu, Runs, etc...
        public override void Initialize()
        {
            go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = new Vector3(10000, -10000, -10000);
            cGO = go.AddComponent<CustomGameObject>();    
            GameObject.DontDestroyOnLoad(go);
        }

        //called before an initialization (Scene loads), this is called n - 1 where n is the times that the mod has been initialized.
        public override void UnInitialize()
        {
            GameObject.Destroy(go);
            go = null;
            cGO = null;
        }
    }

    //Do your game mod stuff here, creating custom game objects, detecting inputs, etc...
    public class CustomGameObject : MonoBehaviour
    {
        public void Start()
        {
            //one time, just like unity
        }

        public void Update()
        {
           //every frame, just like unity
        }
    }
}


```

Once you compile your mod, you will need to create a *.mod file (A json file) like so:

```json
{

	"Name" : "Test",
	"Description" : "This is a test mod.",
	"Version" : "1.0.0.1",
	"Author" : "BIGDummyHead"

}
```

This information is then saved into your mods ``Info`` property, which can be accessed when creating your ``Mod``

### Using Your Mod

Once you have compiled your mod's DLL and created your *.mod file. Add a ``Mods`` folder to your game's root directory, if it does not exist. Once you have a ``Mods`` folder, create a folder inside and drop your compiled DLL and *.mod file.

Directories should look like:
```
REPO Root
|_Mods
 |_Test Mod
  |_TestMod.dll
  |_testmod.mod
```
This process will be simplified in the future, since you will not have to compile REPOSE.dll yourself or add in the dependencies like 0Harmony.dll.

### Future plans:
<ul>
  <li>More events using Harmony for pre/post fixing.</li>
  <li>Simpler install process</li>
  <li>Easy PhotonView invocation (Meaning calling a method reflects on other people's system, IF it is a base method)</li>
</ul>
