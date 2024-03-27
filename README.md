UnityGunTutorial_Part11

Part 1 is just one script: https://pastebin.com/C0hSYuHz

Uploading parts based on youtube videos.

Each part has new code (overrwrites last part!)

If upgrading, delete old code.

Do not install all parts only most current (highest part number) or the part you want to use.

To install, copy files to the "assets" folder of a new unity project.

Follow tutorials to set tags & layers as well as understand the project & code. https://www.youtube.com/playlist?list=PLarxIB7wVp2qLqS9EmZQ-YKxzvl8M-Vrs

NOTE: 

To fix the spraying, reduce the spray amount to something like 0.25 
in the FireHitscan() function, replace the old code at the start of the function with this:

        if (isAimingDownSight)
        {
            posToShootFrom = Camera.main.transform.forward + sprayOffset;
        }
        else
        {
            posToShootFrom = transform.forward + sprayOffset;
        }
        if (Physics.Raycast(transform.position, posToShootFrom, out RaycastHit hit, hitscanRange, playerManager.hitscanlayers))
