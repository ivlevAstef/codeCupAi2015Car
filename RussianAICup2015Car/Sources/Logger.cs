using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RussianAICup2015Car.Sources {
  public class Logger {
    public void Error(String message, params object[] args) {
       System.Console.WriteLine("Error: " + message, args);
    }
    public void Warning(String message, params object[] args) {
      System.Console.WriteLine("Warning: " + message, args);
    }
    public void Debug(String message, params object[] args) {
      System.Console.WriteLine("Debug: " + message, args);
    }
    public void Info(String message, params object[] args) {
      System.Console.WriteLine("Info: " + message, args);
    }

    public void Assert(bool condition) {
      System.Diagnostics.Debug.Assert(condition);
    }
  }
}
