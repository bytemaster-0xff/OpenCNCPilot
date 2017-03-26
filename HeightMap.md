# Height Maps

Height Maps are used to compensate for a non-level PCB Blank.  Even small differences in the height or the PCB blank can make a big difference in trace width when milling your PCB.

If the height of the board is too low, the bit may not remove enough material and your board will result in a short.  If the board height is too high it may remove too much material and will result in a trace on your board being open.

The Height Map process will probe predetermined points on your board to find the exact height and then apply those heights to the GCode that will mill your PCB.

For best results it isn't recommended to reuse height maps between circuit boards or if you are milling both the top and bottom of the board.  You should capture a height map after mounting the board to your surface or after flipping the board and prior to milling/etcing the traces.



### Capturing Height Map

1. [Drill Hold Downs](HoldDowns.md) for your board
1. Optionally [Drill Holes](Drills.md) in your board
1. Attach the board to your base
1. Attach one probe to the screws used to hold down the board
1. Attach the other probe to the drill bit used for probing
1. Move the Drill bit to the Origin of your PCB Blank
1. Move the Drill bit so it's approximately 5mm above the board
1. Zero the Z access
1. Press the Probe Height Map button
1. At this point the Tool Head will move to the origin of the actual PCB within the PCB Blank and perform an initial probe and set the Work Offset to Zero.  This will be used to establish a zero reference point.
1. The process will then take over and probe each of the individual points on the grid.