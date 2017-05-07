// The Arduino Wiring application template is documented at
// http://go.microsoft.com/fwlink/?LinkID=820766&clcid=0x409

#include "Axis.h"

// Use GPIO pin 5
const unsigned int LED_PIN = GPIO5;

extern bool m_bKill;

extern Axis PasteAxis;
extern Axis PlaceAxis;
extern Axis CAxis;
extern Axis YAxis;
extern Axis XAxis;

void setup()
{
    // put your setup code here, to run once:

    pinMode(LED_PIN, OUTPUT);
}

void loop(){
	//TODO May want to experiment with checking elapsed micros rather than delay...
	delayMicroseconds(25);
	if (!m_bKill) {
		XAxis.Update();
		YAxis.Update();
		CAxis.Update();
		PasteAxis.Update();
		PlaceAxis.Update();
	}
}
