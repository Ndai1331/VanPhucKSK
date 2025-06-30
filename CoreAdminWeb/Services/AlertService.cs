using CoreAdminWeb.Model.Alert;

namespace CoreAdminWeb.Services
{
    public class AlertService
    {
        public event Action<List<AlertModel>> OnAlertChange;
        private List<AlertModel> _alerts = new List<AlertModel>();
        private readonly object _lock = new object();

        public void ShowAlert(string message, string type = "primary")
        {
            var alert = new AlertModel
            {
                Message = message,
                Type = type
            };

            lock (_lock)
            {
                _alerts.Add(alert);
                OnAlertChange?.Invoke(new List<AlertModel>(_alerts));
            }

            // Auto remove after 5 seconds
            Task.Delay(5000).ContinueWith(_ =>
            {
                RemoveAlert(alert.Id);
            });
        }

        public void RemoveAlert(Guid id)
        {
            lock (_lock)
            {
                var alert = _alerts.Find(x => x.Id == id);
                if (alert != null)
                {
                    alert.Visible = false;
                    OnAlertChange?.Invoke(new List<AlertModel>(_alerts));
                    
                    // Remove from list after animation
                    Task.Delay(300).ContinueWith(_ =>
                    {
                        lock (_lock)
                        {
                            _alerts.Remove(alert);
                            OnAlertChange?.Invoke(new List<AlertModel>(_alerts));
                        }
                    });
                }
            }
        }

        public List<AlertModel> GetAlerts()
        {
            lock (_lock)
            {
                return new List<AlertModel>(_alerts);
            }
        }
    }
} 