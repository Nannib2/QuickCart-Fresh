using BO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLApi;

public interface IAdmin
{
    /// <summary>
    /// Resets the database to a completely empty state.
    /// This includes setting all configuration variables back to their initial default values 
    /// and clearing the data of all entities (emptying all data lists).
    /// </summary>
    public void ResetDB();

    /// <summary>
    /// Initializes the database with predefined starting data.
    /// This operation first performs a full reset (ResetDB) and then populates all 
    /// entity lists with initial values according to the database initialization requirements.
    /// </summary>
    public void InitializeDB();

    /// <summary>
    /// Retrieves the current value of the system clock.
    /// </summary>
    /// <returns>The current time of the system clock as a DateTime object.</returns>
    public DateTime GetClock();

    /// <summary>
    /// Advances the system clock by a specified time unit.
    /// Internally calculates the new time based on the current clock and the provided unit, 
    /// and updates the system clock accordingly.
    /// </summary>
    /// <param name="timeUnit">The unit of time by which to advance the clock (Minute, Hour, Day, etc.).</param>
    public void ForwardClock(TimeUnit timeUnit);

    /// <summary>
    /// Retrieves the current configuration settings relevant to the presentation layer.
    /// Excludes internal configuration variables such as running numbers.
    /// </summary>
    /// <returns>An object of type BO.Config containing the current, externally relevant configuration values.</returns>
    public BO.Config GetConfig();

    /// <summary>
    /// Updates the system configuration settings based on the provided configuration object.
    /// Only updates configuration values that are intended to be modified from the presentation layer (excluding internal variables).
    /// </summary>
    /// <param name="newConfig">A BO.Config object containing the new configuration values.</param>
    public Task SetConfig(BO.Config newConfig);

    public void StartSimulator(int interval); //stage 7
    public void StopSimulator(); //stage 7
    #region Stage 5
    void AddConfigObserver(Action configObserver);
    void RemoveConfigObserver(Action configObserver);
    void AddClockObserver(Action clockObserver);
    void RemoveClockObserver(Action clockObserver);
    #endregion Stage 5


}
