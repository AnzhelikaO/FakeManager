# FakeManager

## About
Plugin for creating zones with fake tiles and signs.

## FakeManager
You can create a new fake zone using method FakeManager.Common.Add():
```cs
FakeTileRectangle Add(object Key, int X, int Y,
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

## FakeTileRectangle
Object of class FakeTileRectangle is equivalent to ITileCollection (or ITile[,]).

You can access tile at relative coordinates (X, Y) with operator[,]:
```cs
ITile this[int X, int Y];
```
<details><summary> <sup><b><ins>Parameters</ins></b> (click here to expand)</sup> </summary>
<p>

* int **X**
	* Horizontal coordinate relative to left border of rectangle.
* int **Y**
	* Vertical coordinate relative to top border of rectangle.

</p>
</details>

Update position and size of fake zone:
```cs
void SetXYWH(int X, int Y, int Width, int Height);
```

### Currently supported entities

#### Signs

Add (or replace) fake sign:
```cs
void AddSign(Sign Sign, bool Replace = true);
```
For example: AddSign(new Sign() { x=10, y=5, text='kek' });

Remove:
```cs
bool RemoveSign(Sign Sign);
bool RemoveSign(int X, int Y);
```

***

This plugin uses modified version of [Tiled](https://github.com/thanatos-tshock/Tiled).