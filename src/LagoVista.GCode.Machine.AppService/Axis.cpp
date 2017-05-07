#include "Axis.h"

extern void AxisCompleted(int axisNumber);

Axis::Axis(int axis, int pin){
	this->m_pin = pin;
	this->m_axis = axis;
	pinMode(pin, OUTPUT);
}

void Axis::SetSteps(INT64 steps, int multiplier) {
	this->m_StepsRemaining = steps;
	this->m_multiplier = multiplier;
	this->m_multiplierCountDown = multiplier;
	this->m_lastToggleLow = true;
}

void Axis::Clear() {
	this->m_StepsRemaining = 0;
}

void Axis::Update() {
	if (this->m_StepsRemaining > 0)
	{
		if (m_multiplierCountDown > 1) {
			m_multiplierCountDown--;
			return;
		}

		m_multiplierCountDown = m_multiplier;

		if (m_lastToggleLow) {
			digitalWrite(this->m_pin, LOW);
		}
		else {
			digitalWrite(this->m_pin, LOW);
			this->m_StepsRemaining--;
		}

		m_lastToggleLow = !m_lastToggleLow;
		
		if (this->m_StepsRemaining == 0) {
			AxisCompleted(this->m_axis);
		}
	}
}
