using CoreAdminWeb.Http;
using System;
using System.Threading;
using CoreAdminWeb.Model;
using CoreAdminWeb.Model.RequestHttps;
using CoreAdminWeb.Services.BaseServices;
using CoreAdminWeb.Services.Http;
using System.Net;

namespace CoreAdminWeb.Services
{
    public interface ILoadingService
    {
        /// <summary>Đang loading hay không.</summary>
        bool IsBusy { get; }


        /// <summary>Bật loading (có thể kèm message). Hỗ trợ lồng nhau (ref-count).</summary>
        void Show();

        /// <summary>Tắt loading (giảm ref-count). Khi về 0 sẽ tắt.</summary>
        void Hide();

        /// <summary>Reset về tắt ngay lập tức.</summary>
        void Reset();

        /// <summary>Sự kiện khi trạng thái thay đổi (UI sẽ subscribe).</summary>
        event Action? OnChanged;
    }
    public class LoadingService : ILoadingService
    {
        private int _counter = 0;
        private readonly object _lock = new();

        public bool IsBusy { get; private set; }

        public event Action? OnChanged;

        public void Show()
        {
            lock (_lock)
            {
                _counter++;
                if (!IsBusy)
                {
                    IsBusy = true;
                    Notify();
                }
            }
        }

        public void Hide()
        {
            lock (_lock)
            {
                if (_counter > 0) _counter--;
                if (_counter == 0 && IsBusy)
                {
                    IsBusy = false;
                    Notify();
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _counter = 0;
                IsBusy = false;
                Notify();
            }
        }

        private void Notify() => OnChanged?.Invoke();
    }
}