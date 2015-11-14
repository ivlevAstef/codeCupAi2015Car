using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianAICup2015Car.Sources {
    class Logger {
        void Error(String message, params object[] args) {
            System.Console.WriteLine("Error: " + message, args);
        }
        void Warning(String message, params object[] args) {
            System.Console.WriteLine("Warning: " + message, args);
        }
        void Debug(String message, params object[] args) {
            System.Console.WriteLine("Debug: " + message, args);
        }
        void Info(String message, params object[] args) {
            System.Console.WriteLine("Info: " + message, args);
        }
    }
}
