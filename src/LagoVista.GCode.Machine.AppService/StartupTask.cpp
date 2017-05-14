using namespace Windows::ApplicationModel::Background;
using namespace Windows::ApplicationModel::AppService;

#include <Axis.h>

// These functions should be defined in the sketch file
void setup();
void loop();

bool m_bKill = false;

Axis ^XAxis = ref new Axis(0, GPIO21);
Axis ^YAxis = ref new Axis(1, GPIO19);;
Axis ^CAxis = ref new Axis(2, GPIO6);;
Axis ^PlaceAxis = ref new Axis(3, GPIO22);
Axis ^PasteAxis = ref new Axis(4, GPIO17);


AppServiceConnection ^m_appServiceConnection;

namespace LagoVistaGCodeMachineAppService
{
	[Windows::Foundation::Metadata::WebHostHidden]
	public ref class StartupTask sealed : public IBackgroundTask
	{
	private:
		Platform::Agile<BackgroundTaskDeferral> m_Deferral = nullptr;
		IBackgroundTaskInstance^ m_TaskInstance = nullptr;

	public:
		virtual void Run(Windows::ApplicationModel::Background::IBackgroundTaskInstance^ taskInstance)
		{
			m_TaskInstance = taskInstance;
			m_Deferral = taskInstance->GetDeferral();

			taskInstance->Canceled += ref new Windows::ApplicationModel::Background::BackgroundTaskCanceledEventHandler(this, &LagoVistaGCodeMachineAppService::StartupTask::OnCanceled);

			auto triggerDetails = taskInstance->TriggerDetails;
			auto trigger = ((AppServiceTriggerDetails ^)(triggerDetails));
			m_appServiceConnection = trigger->AppServiceConnection;
			m_appServiceConnection->RequestReceived += ref new Windows::Foundation::TypedEventHandler<Windows::ApplicationModel::AppService::AppServiceConnection ^, Windows::ApplicationModel::AppService::AppServiceRequestReceivedEventArgs ^>(this, &LagoVistaGCodeMachineAppService::StartupTask::OnRequestReceived);

			setup();
			while (true)
			{
				loop();
			}
		}

		void OnRequestReceived(Windows::ApplicationModel::AppService::AppServiceConnection ^sender, Windows::ApplicationModel::AppService::AppServiceRequestReceivedEventArgs ^args);
		void OnCanceled(Windows::ApplicationModel::Background::IBackgroundTaskInstance ^sender, Windows::ApplicationModel::Background::BackgroundTaskCancellationReason reason);
	};

	void LagoVistaGCodeMachineAppService::StartupTask::OnRequestReceived(Windows::ApplicationModel::AppService::AppServiceConnection ^sender, Windows::ApplicationModel::AppService::AppServiceRequestReceivedEventArgs ^args)
	{
		auto deferral = args->GetDeferral();

		/*auto msg = ref new Windows::Foundation::Collections::ValueSet();
		msg->Insert("STATUS", "HI");
		m_appServiceConnection->SendMessageAsync(msg);*/


		if (args->Request->Message->HasKey("KILL")) {
			m_bKill = true;
			XAxis->Clear();
			YAxis->Clear();
			CAxis->Clear();
			PlaceAxis->Clear();
			PasteAxis->Clear();
		}
		else if (args->Request->Message->HasKey("RESET")) {
			m_bKill = false;
		}
		else if (args->Request->Message->HasKey("AXIS")) {
			int axis = (int)args->Request->Message->Lookup("AXIS");
			int multiplier = (int)args->Request->Message->Lookup("MULTIPLIER");
			int steps = (int)args->Request->Message->Lookup("STEPS");

			switch (axis)
			{
			case 0: XAxis->SetSteps(steps, multiplier); break;
			case 1: YAxis->SetSteps(steps, multiplier); break;
			case 2: CAxis->SetSteps(steps, multiplier); break;
			case 3: PlaceAxis->SetSteps(steps, multiplier); break;
			case 4: PasteAxis->SetSteps(steps, multiplier); break;
			}
		}

		deferral->Complete();
	}


	void LagoVistaGCodeMachineAppService::StartupTask::OnCanceled(Windows::ApplicationModel::Background::IBackgroundTaskInstance ^sender, Windows::ApplicationModel::Background::BackgroundTaskCancellationReason reason)
	{
		m_Deferral.Get()->Complete();
	}
}

void AxisCompleted(int axis) {
	auto msg = ref new Windows::Foundation::Collections::ValueSet();
	msg->Insert("DONE", axis);
	m_appServiceConnection->SendMessageAsync(msg);
}
