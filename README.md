# FakeManager

## About
Plugin for creating zones with fake tiles and signs.

## Usage
You can create a new fake zone:
```cs
FakeManager.FakeManager.Common.Add(object Key, int X, int Y,
	int Width, int Height, ITileCollection CopyFrom);
```
<details><summary> <sup><b><ins>Parameters</ins></b> (click here to expand)</sup> </summary>
<p>

* object **Key**
	* Unique identifier.
* [ITileCollection **CopyFrom**]
	* Tiles to copy from if specified.

</p>
</details>

And remove it:
```cs
FakeManager.FakeManager.Common.Remove(object Key);
```

***

This plugin uses modified version of [Tiled](https://github.com/thanatos-tshock/Tiled).