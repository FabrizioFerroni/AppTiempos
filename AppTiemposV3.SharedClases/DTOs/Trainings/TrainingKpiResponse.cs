namespace AppTiemposV3.SharedClases.DTOs.Trainings;

public class TrainingKpiResponse
{
    public int TotalTrainings { get; set; }
    public int TrainingCompleted { get; set; }
    public int TrainingInProgress { get; set; }
    public int TrainingIsLoaded { get; set; }
}