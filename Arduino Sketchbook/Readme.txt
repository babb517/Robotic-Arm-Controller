Currently the project uses:

  3 x ArduIMU V2 with "arduimu V2 for sleeve - Team7" firmware.  Although they run at 28.8 baud, serial monitor does NOT properly show data from these.
  1 x Lilypad (ATMEGA328) running "LilyPadFirmware-Team7", which takes the data from the 3 ArduIMU V3s and presents it nicely over USB/serial at 19.2 baud (possibly changed to 38.4).

Other, testing firmwares for the ArduIMUs:

ArduIMU 1.9-temp - Slightly-modified stock 1.9 firmware.
  For ArduIMU V2: Air Start seems to work, as does non-air-start.  With "extra" ArduIMU V2 the ArduPilot (ArduIMU Test) plane test program reads reasonable results, except when rolling.
  For ArduIMU V3+: Air Start leads to erratic data.  With non-air start (which takes about 20 seconds) data is exaggerated (90 degrees deltra becomes 120ish).

ArduIMU V3 1.9.8 - Team 7 - Modified 1.9.8 firmware.
  For ArduIMU V3+: With air start off (~20 second calibration) all motions seemed to read as doubled.  e.g. 90 degree pitch is seen as 180 degrees.

