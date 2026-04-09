

namespace BlImplementation;
using BLApi;
using BO;
using Helpers;
using System;

internal class AdminImplementation : IAdmin
{
   
    public void ForwardClock(TimeUnit timeUnit)
    {
         AdminManager.ForwardClock(timeUnit);
    }

    public DateTime GetClock()
    {
       return AdminManager.Now;
    }

    public BO.Config GetConfig() => AdminManager.GetConfig();


    public void InitializeDB()
    {
        AdminManager.InitializeDB();
    }

    public void ResetDB()
    {
        AdminManager.ResetDB();
    }

    public async Task SetConfig(BO.Config configuration) =>await AdminManager.SetConfig(configuration);

    public void StartSimulator(int interval)  //stage 7
    {
        AdminManager.ThrowOnSimulatorIsRunning();  //stage 7
        AdminManager.Start(interval); //stage 7
    }

    public void StopSimulator()
    => AdminManager.Stop(); //stage 7


    #region Stage 5
    public void AddClockObserver(Action clockObserver) =>
   AdminManager.ClockUpdatedObservers += clockObserver;
    public void RemoveClockObserver(Action clockObserver) =>
    AdminManager.ClockUpdatedObservers -= clockObserver;
    public void AddConfigObserver(Action configObserver) =>
   AdminManager.ConfigUpdatedObservers += configObserver;
    public void RemoveConfigObserver(Action configObserver) =>
    AdminManager.ConfigUpdatedObservers -= configObserver;
    #endregion Stage 5


}
