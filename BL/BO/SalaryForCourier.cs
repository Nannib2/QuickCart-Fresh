using Helpers;
namespace BO;
/// <summary>
/// courier salary details for a specific month for reporting(extra)
/// </summary>
public class SalaryForCourier

{
    /// <summary>
    /// year of the salary
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// month of the salary
    /// </summary>
    public Months Month { get; set; }

    /// <summary>
    /// base salary component of the total salary (from config)
    /// </summary>
    public double BaseSalary { get; init; } // Added BaseSalary field for clarity

    /// <summary>
    /// bonus for total deliveries exceeding a threshold
    /// </summary>
    public double DeliveryVolumeBonus { get; set; } // Bonus for > 20 deliveries at month

    /// <summary>
    /// bonus based on the courier's delivery performance (on-time rate)
    /// </summary>
    public double PerformanceBonus { get; set; } // Bonus for on-time performance

    /// <summary>
    /// bonus for veteran couriers (based on start date)
    /// </summary>
    public double SeniorityBonus { get; set; } //Bonus for seniority

    /// <summary>
    /// total amount of all bonuses
    /// </summary>
    public double TotalBonus => DeliveryVolumeBonus + PerformanceBonus + SeniorityBonus;

    /// <summary>
    /// total salary amount for the courier in the specified month (Base + Distance + TotalBonus)
    /// </summary>
    public double TotalSalary { get; set; }

    /// <summary>
    /// total salary amount from distance calculation
    /// </summary>
    public double DistanceSalaryComponent { get; set; } //distance component for clarity

    public override string ToString() => Tools.ToStringProperty<SalaryForCourier>(this);
}