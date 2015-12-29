//
//File: SIALogger.cpp
//Description: 
//Author: Ivlev Alexander. Stef
//Created: 21:21 16/7/2015
//Copyright (c) SIA 2015. All Right Reserved.
//

#include "SIALogger.h"

#include <stdarg.h>  // For va_start, etc.

#if defined(_WIN32)
#include <Windows.h>
#endif

#define ABS(a) ((a) < 0 ? -(a) : (a))

#pragma warning( push )
#pragma warning( disable : 4996 )

using namespace SIA;

static std::string fileName(std::string filePath) {
  std::size_t found = filePath.find_last_of("/\\");
  if (found == std::string::npos) {
     return filePath;
  }
  return filePath.substr(found+1);
}


void Logger::logToStream(const char* logLevel, const char* file, int line, const char* module, const char* message, ...) {
  static const size_t maxLogBufferSize = 2048;

  char logBuffer[maxLogBufferSize] = {0};
  size_t logIndex = 0;

  ///Added logLevel
  if (nullptr != logLevel) {
    logIndex += sprintf(logBuffer + logIndex, "> %s ", logLevel);
  }

  ///Added module
  if (nullptr != module) {
    logIndex += sprintf(logBuffer + logIndex, "[%s] ", module);
  }

  ///Added filename and file line
  if (nullptr != file) {
    logIndex += sprintf(logBuffer + logIndex, "{%s:%d} ", fileName(file).c_str(), line);
  }

  ///Added message
  if (nullptr != message) {
    va_list ap;
    va_start(ap, message);
    logIndex += vsprintf(logBuffer + logIndex, message, ap);
    va_end(ap);
  }

  logBuffer[logIndex++] = '\n';
  logBuffer[logIndex] = 0;

  ///print
#if defined(_WIN32)
  OutputDebugStringA(logBuffer);
#else
  std::cerr << logBuffer;
#endif
}


#pragma warning( pop )