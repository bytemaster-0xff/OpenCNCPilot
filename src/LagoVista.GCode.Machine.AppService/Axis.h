#pragma once
ref class Axis sealed
{
private:
	bool m_lastToggleLow;
	INT64 m_StepsRemaining = 0;
	int m_multiplier = 1;
	int m_pin;
	int m_multiplierCountDown;
	INT64 m_axis;

public:
	Axis(int axis, int pin);
	void SetSteps(INT64 steps, int multiplier);
	void Clear();
	void Update();
};

