#ifndef VISUAL_DEBUG_CLIENT_
#define VISUAL_DEBUG_CLIENT_

#include <cstdlib>
#include <cstdio>
#include <string>
#include <cstdint>

class Visualizator {
public:
  static std::string DEFAULT_HOST;
  static std::string DEFAULT_PORT;
  static const int BUF_SIZE;

  Visualizator();

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
  
private:
  void sendCommand(const char* str) const;
  void writeWithColor(char* buf, int32_t color) const;

  int openSocket;
};

#endif