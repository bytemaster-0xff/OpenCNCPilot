#pragma once
ref class Axis sealed
{
private:
	bool m_lastToggleLow;
	int m_StepsRemaining = 0;
	int m_multiplier = 1;
	unsigned int m_pin;
	int m_multiplierCountDown;
	int m_axis;

public:
	Axis(int axis, unsigned int pin);
	void SetSteps(int steps, int multiplier);
	void Clear();
	void Init();
	void Update();
};

