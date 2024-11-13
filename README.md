# Battle of the Foods!
 This is a spawn simulation project experimenting with managing and spawning hundreds of objects at a time. The goals were to gain experience with spawn managers, object pooling, and runtime optimization.
 
### Controls
* **Mouse Click** on the food items to spawn units
* **Scroll** to zoom in/out
* Use **WASD** or **Drag Mouse** to pan
<p align="center">
 <img src="https://github.com/emmalei-sc/Ares-Spawner-Sim/assets/78105383/c6bee147-94df-4cc2-8140-05f508233772" width="720"/>  
 
 The four holy food groups... the egg, the blueberry, the grape, and the cheese.
</p>

### Behavior
 Clicking on each food item will create a unit of that type. When units of the same type collide, they will bounce off of each other and spawn a new unit. When units of different types collide, they will destroy each other.

### Comments
 The biggest challenge while working on this project was handling same-unit collisions. In theory, it seemed like I could let them bump around and spawn indefinitely, but I quickly realized that their numbers would increase exponentially, which caused severe lag and general unpleasantness. (The entire screen would just be covered by hundreds of the same unit.)  
 
My solution to this was to lower the spawn rate of units. One method I used was to impose a spawn cooldown on same-unit collisions — the more units on screen there were, the longer this cooldown would be. I also ran into severe lag issues caused by the number of collisions happening between same-type units, so once the population hit a certain threshold, I disabled the ability to spawn new units for a percentage of the unit population. Similarly, this percentage scaled linearly with the amount of units on screen. My optimization approach did mean that some same-unit collisions did not result in new units, but I felt that this balanced user experience with the design features outlined in the project. 

 Overall, this was pretty fun! I don't have a whole lot of experience with optimization but would love to learn more, so I welcome any feedback on my approach!

### Additional Features/Improvements
 I think the player could theoretically still cause a lot of lag by spam clicking to spawn the same unit over and over. Though I stress tested this on my PC in editor, I'm not quite sure what the limit is. I think there's probably still more that could be done to optimize the load, especially for mobile devices.  
 
 Also, if I had more time to tinker with this, I think it would have been fun to add some VFX or audio to the collisions to make the experience really pop.
