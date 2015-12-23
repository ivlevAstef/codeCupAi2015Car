using System;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Globalization;

namespace Com.CodeGame.CodeRacing2015.DevKit.CSharpCgdk {

  // цвет нужно задавать hex-числом, например 0xABCDEF, AB - red, CD - green, EF - blue, каждый цвет - число из двух hex-цифр в диапазоне от 00 до FF
  public class VisualClient {
    
    private TcpClient client;
    private StreamWriter writer;

    public VisualClient(string host, int port) {
      Connect(host, port);
    }

    public void Connect(string host, int port) {
      client = new TcpClient(host, port);
    }

    public void Disconnect() {
      client.Close();
    }

    private void sendCommand(string command) {
      command = command.Replace(',', '.');

      if (client != null) {
        if (writer == null) {
          writer = new StreamWriter(client.GetStream (), Encoding.ASCII);
        }
        writer.WriteLine(command);
      }
      System.Console.WriteLine(command);
    }

    public void BeginPre() {
      sendCommand("begin pre");
    }

    public void BeginPost() {
      sendCommand("begin post");
    }
  
    public void EndPre() {
      sendCommand("end pre");
    }

    public void EndPost() {
      sendCommand("end post");
    }

    private string encodeColor(int color) {
      int red = (color & 0xFF0000) >> 16;
      int green = (color & 0x00FF00) >> 8;
      int blue = color & 0x0000FF;

      return String.Format("{0} {1} {2}", (double)red / 256.0, (double)green / 256.0, (double)blue / 256.0);
    }

    public void Circle(double x, double y, double radius, int color) {
      sendCommand(String.Format("circle {0} {1} {2} {3}", x, y, radius, encodeColor(color)));
    }

    public void FillCircle(double x, double y, double radius, int color) {
      sendCommand(String.Format("fill_circle {0} {1} {2} {3}", x, y, radius, encodeColor(color)));
    }

    public void Rect(double x1, double y1, double x2, double y2, int color) {
      sendCommand(String.Format("rect {0} {1} {2} {3} {4}", x1, y1, x2, y2, encodeColor(color)));
    }

    public void FillRect(double x1, double y1, double x2, double y2, int color) {
      sendCommand(String.Format("fill_rect {0} {1} {2} {3} {4}", x1, y1, x2, y2, encodeColor(color)));
    }

    public void Line(double x1, double y1, double x2, double y2, int color) {
      sendCommand(String.Format("line {0} {1} {2} {3} {4}", x1, y1, x2, y2, encodeColor(color)));
    }

    public void Print(double x, double y, string msg, int color = 0) {
      sendCommand(String.Format("text {0} {1} {2} {3}", x, y, msg, encodeColor(color)));
    }
  }

}

