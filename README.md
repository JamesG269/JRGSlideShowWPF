# JRGSlideShowWPF
Simple SlideShow software for Windows, written in C# WPF.
I wrote this because every slide show software for windows I tried was missing what I consider to be basic functionality. 
Most can not stop the monitor from sleeping, which is odd because it's just a few lines of C#, and they can not delete the current picture, which
I consider to be important. Plus they're intolerably slow even on a 3.5Ghz octo-core processor which is shameful. And I wanted to learn C# better. 

Key features:

Right click on Window or Notify Icon for options, including Open Directory.

Double click or drag to top of monitor to full screen.

Can delete the current picture. (Insert key undeletes last picture. All pictures deleted in a session are remembered and can be undeleted by pressing INS repeatedly. 
Deleted pictures are stored in the Recycle Bin)

Can stop monitor from Sleeping.

Has google picture lookup.

Written for Multithreadedness and Performanance. Every time I add code, I test performance and benchmark as if it were a 3D game. 
Most slide show programs I tried are 1-2 pictures a second. This one includes a benchmark mode, and it can display 400 high quality jpegs in 11 seconds. 
(most of the pictures I tested are 500k - 3MB, and are stored on a Samsung 850 512GB SSD, the test bed cpu is a 5960x 8-core but most of the code executes sequentially).
Even though most people would not consider performance of a slide show program to be important at first thought, it's annoying to use slide software that is just 
unnecessarily slow, the functions seem too simple for the dog slow performance I find in most of them.
