namespace AppTiemposV3.Web.Services
{
    public class ActivityStateService
    {
        public event Action OnActivityUpdated;

        public void NotifyActivityUpdated()
        {
            // Disparar el evento
            OnActivityUpdated?.Invoke();
        }
    }
}
