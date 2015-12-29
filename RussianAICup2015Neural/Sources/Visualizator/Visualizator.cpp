#include "Visualizator.h"
#if (defined _WIN32 || defined _WIN64)
# include <winsock2.h>
# include <Ws2tcpip.h>

#include <BaseTsd.h>
typedef SSIZE_T ssize_t;

namespace {

  ssize_t close(SOCKET s)
  {
    return closesocket(s);
  }

  ssize_t write(SOCKET s, const char *buf, int len, int flags = 0)
  {
    return send(s, buf, len, flags);
  }

}

#pragma warning(disable: 4244 4996)
#else
# include <sys/socket.h>
#include <netdb.h>
#include <unistd.h>
#endif
#include <cstdio>
#include <cstdlib>

#include <string>

std::string Visualizator::DEFAULT_HOST = "127.0.0.1";
std::string Visualizator::DEFAULT_PORT = "13579";
const int Visualizator::BUF_SIZE = 1024;

Visualizator::Visualizator() : openSocket(-1) {
  /* Obtain address(es) matching host/port */
  addrinfo hints;
  memset(&hints, 0, sizeof(addrinfo));
  hints.ai_family = AF_UNSPEC;    /* Allow IPv4 or IPv6 */
  hints.ai_socktype = SOCK_STREAM; /* Datagram socket */
  hints.ai_flags = 0;
  hints.ai_protocol = 0;          /* Any protocol */

  addrinfo* result = NULL;
  INT success = getaddrinfo(DEFAULT_HOST.c_str(), DEFAULT_PORT.c_str(), &hints, &result);
  if (0 != success) {
    fprintf(stderr, "Could not get address");
    return;
  }


  for (addrinfo* rp = result; NULL != rp; rp = rp->ai_next) {
    SOCKET sfd = socket(rp->ai_family, rp->ai_socktype, rp->ai_protocol);
    if (-1 == sfd) {
      continue;
    }

    if (-1 == connect(sfd, rp->ai_addr, rp->ai_addrlen)) {
      close(sfd);
      continue;
    }

    openSocket = sfd;
    break;/* Success */
  }

  freeaddrinfo(result);

  if (-1 == openSocket) {/* No address succeeded */
    fprintf(stderr, "Could not connect\n");
  }
  
}

void Visualizator::sendCommand(const char* str) const {
  if (-1 == openSocket) {
    return;
  }

  int len = strlen(str);
  int pos = 0;
  while (pos < len) {
    ssize_t res = write(openSocket, str + pos, len);
    if (-1 == res) {
      fprintf(stderr, "Couldn't send command");
      return;
    }
    pos += res;
  }
}

void Visualizator::beginPre() const {
  sendCommand("begin pre\n");
}

void Visualizator::endPre() const {
  sendCommand("end pre\n");
}

void Visualizator::beginPost() const {
  sendCommand("begin post\n");
}

void Visualizator::endPost() const {
  sendCommand("end post\n");
}

void Visualizator::writeWithColor(char* buf, int32_t color) const {
  size_t len = strlen(buf);
  float r = ((color & 0xFF0000) >> 16) / 256.0;
  float g = ((color & 0x00FF00) >> 8) / 256.0;
  float b = ((color & 0x0000FF)) / 256.0;
  sprintf(buf + len, " %.3f %.3f %.3f\n", r, g, b);
  sendCommand(buf);
}

void Visualizator::circle(double x, double y, double r, int32_t color) const {
  char buf[BUF_SIZE] = {0};
  sprintf(buf, "circle %.3lf %.3lf %.3lf", x, y, r);
  writeWithColor(buf, color);
}

void Visualizator::fillCircle(double x, double y, double r, int32_t color) const {
  char buf[BUF_SIZE] = {0};
  sprintf(buf, "fill_circle %.3lf %.3lf %.3lf", x, y, r);
  writeWithColor(buf, color);
}

void Visualizator::rect(double x1, double y1, double x2, double y2, int32_t color) const {
  char buf[BUF_SIZE] = {0};
  sprintf(buf, "rect %.3lf %.3lf %.3lf %.3lf", x1, y1, x2, y2);
  writeWithColor(buf, color);
}

void Visualizator::fillRect(double x1, double y1, double x2, double y2, int32_t color) const {
  char buf[BUF_SIZE] = {0};
  sprintf(buf, "fill_rect %.3lf %.3lf %.3lf %.3lf", x1, y1, x2, y2);
  writeWithColor(buf, color);
}

void Visualizator::line(double x1, double y1, double x2, double y2, int32_t color) const {
  char buf[BUF_SIZE] = {0};
  sprintf(buf, "line %.3lf %.3lf %.3lf %.3lf", x1, y1, x2, y2);
  writeWithColor(buf, color);
}

void Visualizator::text(double x, double y, const char* text, int32_t color) const {
  char buf[BUF_SIZE] = {0};
  sprintf(buf, "text %.3lf %.3lf %s", x, y, text);
  writeWithColor(buf, color);
}