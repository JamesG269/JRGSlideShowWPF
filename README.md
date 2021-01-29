# JRGSlideShowWPF
![Untitled](https://user-images.githubusercontent.com/2309574/106255569-cf647280-61df-11eb-8ade-9da6694570b9.jpg)
Simple SlideShow software for Windows, written in C# WPF.
I wrote this because every slide show software for windows I tried was missing what I consider to be basic functionality. 
Most can not stop the monitor from sleeping, which is odd because it's just a few lines of C#, and they can not delete the current picture, which
I consider to be important. Plus they're intolerably slow even on a 3.5Ghz octo-core processor which is shameful. And I wanted to learn C# better. 

Key features:

Borderless, drag window to top or sides, to full screen or half screen, drag again to un-fullscreen, etc. resizeable.

Right click on Window or Notify Icon for options, including Open Directory.

Double click or drag to top of monitor to full screen.

Can delete the current picture. (Insert key undeletes last picture. All pictures deleted in a session are remembered and can be undeleted by pressing INS repeatedly. 
Deleted pictures are stored in the Recycle Bin)

Can stop monitor from Sleeping.

Has google picture lookup.

Written for Multithreadedness and Performanance. Every time I add code, I test performance and benchmark as if it were a 3D game. 
It includes a benchmark mode, and it can display 700 high quality jpegs in 18 seconds which is twice the speed what I could get out of Xnview MP x64 with the same images.
