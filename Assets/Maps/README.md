-----------------------------------------
-	HOW TO CREATE YOUR OWN MAPS	-
-----------------------------------------

Maps for this loader are written in hex, here is how the game interprets them:

The first 4 bytes are designated as the row size, i. e. how many tiles before it starts rendering a new line

From then the division is as follows:

4 bits - top of tile sprite, ground type
4 bits - right of tile sprite, ground type
4 bits - bottom of tile sprite, ground type
4 bits - left of tile sprite, ground type
4 bits - resource type on sprite
4 bits- y position of tile

This can be repeated any amount of times.