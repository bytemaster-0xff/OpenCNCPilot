// The Arduino Wiring application template is documented at
// http://go.microsoft.com/fwlink/?LinkID=820766&clcid=0x409

#include "Axis.h"

extern void AxisCompleted(int axisNumber);

extern bool m_bKill;

extern Axis^ PasteAxis;
extern Axis^ PlaceAxis;
extern Axis^ CAxis;
extern Axis^ YAxis;
extern Axis^ XAxis;

//http://stackoverflow.com/questions/39952978/how-to-set-the-background-process-priority-at-windows-iot-startup

void setup()
{
	AxisCompleted(99);
	XAxis->Init();
	YAxis->Init();
	CAxis->Init();
	PasteAxis->Init();
	PlaceAxis->Init();
}

void loop(){
	//TODO May want to experiment with checking elapsed micros rather than delay...
	delayMicroseconds(100);
	if (!m_bKill) {
		XAxis->Update();
		YAxis->Update();
		CAxis->Update();
		PasteAxis->Update();
		PlaceAxis->Update();
	}
}
 