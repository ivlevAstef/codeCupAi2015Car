#define USE_LOG
using System;

namespace RussianAICup2015Car.Sources.Common {
  public class Logger {
    public static readonly Logger instance = new Logger();

#if USE_LOG
    public void Error(String message, params object[] args) {
       System.Console.Error.WriteLine("Error: " + message, args);
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

    public void Assert(bool condition, String message) {
      if (!condition) {
        System.Console.Error.WriteLine("Assert: {0} ", message);
      }
      System.Diagnostics.Debug.Assert(condition);
    }
#else
    public void Error(String message, params object[] args) {
    }
    public void Warning(String message, params object[] args) {
    }
    public void Debug(String message, params object[] args) {
    }
    public void Info(String message, params object[] args) {
    }

    public void Assert(bool condition, String message) {
    }

#endif
  }
}
