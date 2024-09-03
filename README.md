# DOTS-Spawner-Test
A simple project using Entities Jobs and Burst to test a simple sphere spawner, basically what it does is spawn a sphere every 0.01 seconds, and it chooses a rand position to move, after 1 sec the direction of movement is changed abruptly.
I was able to keep over 60 FPS at 13000 entities, keep in mind they have 3D mesh and a metallic (smoothness to 1) material, so it has to calculate reflections and shadows per entity.
![DOTS sphere](https://github.com/user-attachments/assets/e9e546d7-c86f-481a-99ef-4a694e9c89ae)
