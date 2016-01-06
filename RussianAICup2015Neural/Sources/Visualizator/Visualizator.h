#pragma once
#ifndef _VISUALIZATOR_H__
#define _VISUALIZATOR_H__

#if (defined _WIN32 || defined _WIN64)
# include <winsock2.h>
#else
#include <sys/socket.h>
#endif

#include <string>
#include <cstdint>

class Visualizator {
private:
  static std::string DEFAULT_HOST;
  static std::string DEFAULT_PORT;
  static const int BUF_SIZE;
  static char buf[];

  static double windowCenterX;
  static double windowCenterY;
  static double windowWidth;
  static double windowHeight;

  static bool isFirst;

public:
  Visualizator();

  static void setWindowCenter(double x, double y, double maxWidth, double maxHeight);

  void beginPre() const;
  void endPre() const;

  void beginPost() const;
  void endPost() const;

  void circle(double x, double y, double r, int32_t color = 0x7F7F7F) const;
  void fillCircle(double x, double y, double r, int32_t color = 0x7F7F7F) const;
  void rect(double x1, double y1, double x2, double y2, int32_t color = 0x7F7F7F) const;
  void fillRect(double x1, double y1, double x2, double y2, int32_t color = 0x7F7F7F) const;
  void line(double x1, double y1, double x2, double y2, int32_t color = 0x7F7F7F) const;
  void text(double x, double y, const char* text, int32_t color = 0x7F7F7F) const;
  void text(double x, double y, double value, int32_t color = 0x7F7F7F) const;
  void text(double x, double y, int64_t value, int32_t color = 0x7F7F7F) const;

private:
  static bool checkPoint(double x, double y);

  void sendCommand(const char* str) const;
  void writeWithColor(char* buf, int32_t color) const;

  SOCKET openSocket;
};

#endif