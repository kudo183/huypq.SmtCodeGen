using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace huypq.SmtCodeGen
{
    public class Logger
    {
        private static readonly Logger instance = new Logger();

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Logger()
        {
        }

        private Logger()
        {
        }

        public static Logger Instance
        {
            get
            {
                return instance;
            }
        }

        Action<string> _actionWrite;
        Action _actionClear;
        bool _isInitialized = false;

        public void Init(Action<string> actionWrite, Action actionClear)
        {
            _isInitialized = true;
            _actionWrite = actionWrite;
            _actionClear = actionClear;
        }

        public void Write(string msg)
        {
            if(_isInitialized==false)
            {
                throw new InvalidOperationException("not initialized.");
            }
            _actionWrite?.Invoke(msg);
        }

        public void Clear()
        {
            if (_isInitialized == false)
            {
                throw new InvalidOperationException("not initialized.");
            }
            _actionClear?.Invoke();
        }
    }
}
