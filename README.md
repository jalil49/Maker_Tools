# Maker_Tools
A series of mods made to help creators of Koikatsu Cards manage more complex designs and add more identifying information to coordinate cards.
Adding more information to coordinate cards with Additional Card Info.

## Additional Card Info
Add additional information to coordinate cards that other mods/applications to distinguish them.
	https://github.com/jalil49/Maker_Tools/blob/main/Additional%20Card%20Info/README.md

## Accessory Shortcuts
While an empty slot is selected input a number between 1 ~ 0 to quickly assign the last selected accessory of said kind, this resets on swapping outfits.
It also does you the favor of opening the accessory select menu if you didn't want the last selected.

	1=> Last selected Hair Accessory
	
	2=> Last selected Head Accessory
	
	3=> Last selected Face Accessory
	
	and so on

Press "Q" and "E" to change the current slot to the previous or next slot.

## Accessory Parents
Use accessories like parents. Move the parent, move the child.

	Important: resetting rotations don't yet return to the correct position so it'll be best just to rotate back to the desired position
	
Create a parent by entering a name (or leave it blank for a generic slot name), select add in the radio options and click the modify button.

Attach another accessory to it by having the parent selected in dropdown and clicking make child. You can branch this out like a tree if you wanted to.

	Attaching the child will move it to the position of the parent so you can quickly move a hip piece to something parented in the head quickly

To remove a child from parent make it a child of the same parent or make it child of None.

### Shortcuts

	C to make child of currently selected parent
	F to make child and make it a parent with a generic name

## Accessory States
An alternative to AccStateSync, can use its data.

Also has the ability to make a large amount of states with a custom group.

Bind to Parent toggle is different from AccStateSync as it is merely an on-off button that gets added, but can still be part of a group

### Custom Groups

Any added group can have a stupid amount of possible "clothing states" the slider defaults to five, but by moving the stop slider to the right it will increasing the maximum slider value.

Slid too far? No problem when you swap accessories the slider will adjust to the largest state shared by a group. The mod uses the same logic in Hscenes.

## Accessory Themes
Color stuff. Currently dependent of Additional Card Info for Hair info to not accidentally color it.

https://github.com/jalil49/Maker_Tools/tree/main/Accessory%20Themes#readme
