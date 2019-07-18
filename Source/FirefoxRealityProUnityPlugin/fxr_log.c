//
// fxr_log.c
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0.If a copy of the MPL was not distributed with this
// file, You can obtain one at https ://mozilla.org/MPL/2.0/.
//
// Copyright (c) 2019- Mozilla, Inc.
//
// Author(s): Philip Lamb
//

#include "fxr_log.h"

#ifndef _WIN32
#  include <pthread.h> // pthread_self(), pthread_equal()
#  ifdef __ANDROID__
#    include <android/log.h>
#  endif
#  ifdef __APPLE__
#    include <os/log.h>
#  endif
#else
#  include <Windows.h>
#  define snprintf _snprintf
#endif

//
// Global required for logging functions.
//
int fxrLogLevel = FXR_LOG_LEVEL_DEFAULT;
static FXR_LOG_LOGGER_CALLBACK fxrLogLoggerCallback = NULL;
static int fxrLogLoggerCallBackOnlyIfOnSameThread = 0;
#ifndef _WIN32
static pthread_t fxrLogLoggerThread;
#else
static DWORD fxrLogLoggerThreadID;
#endif
#define FXR_LOG_WRONG_THREAD_BUFFER_SIZE 4096
static char *fxrLogWrongThreadBuffer = NULL;
static size_t fxrLogWrongThreadBufferSize = 0;
static size_t fxrLogWrongThreadBufferCount = 0;


void fxrLogSetLogger(FXR_LOG_LOGGER_CALLBACK callback, int callBackOnlyIfOnSameThread)
{
	fxrLogLoggerCallback = callback;
	fxrLogLoggerCallBackOnlyIfOnSameThread = callBackOnlyIfOnSameThread;
	if (callback && callBackOnlyIfOnSameThread) {
#ifndef _WIN32
		fxrLogLoggerThread = pthread_self();
#else
		fxrLogLoggerThreadID = GetCurrentThreadId();
#endif
		if (!fxrLogWrongThreadBuffer) {
			if ((fxrLogWrongThreadBuffer = malloc(sizeof(char) * FXR_LOG_WRONG_THREAD_BUFFER_SIZE))) {
				fxrLogWrongThreadBufferSize = FXR_LOG_WRONG_THREAD_BUFFER_SIZE;
			}
		}
	}
	else {
		if (fxrLogWrongThreadBuffer) {
			free(fxrLogWrongThreadBuffer);
			fxrLogWrongThreadBuffer = NULL;
			fxrLogWrongThreadBufferSize = 0;
		}
	}
}

void fxrLog(const char *tag, const int logLevel, const char *format, ...)
{
	if (logLevel < fxrLogLevel) return;
	if (!format || !format[0]) return;

	va_list ap;
	va_start(ap, format);
	fxrLogv(tag, logLevel, format, ap);
	va_end(ap);
}

void fxrLogv(const char *tag, const int logLevel, const char *format, va_list ap)
{
	va_list ap2;
	char *buf = NULL;
	size_t len;
	const char *logLevelStrings[] = {
		"debug",
		"info",
		"warning",
		"error"
	};
	const size_t logLevelStringsCount = (sizeof(logLevelStrings) / sizeof(logLevelStrings[0]));
	size_t logLevelStringLen;

	if (logLevel < fxrLogLevel) return;
	if (!format || !format[0]) return;

	// Count length required to unpack varargs.
	va_copy(ap2, ap);
#ifdef _WIN32
	len = _vscprintf(format, ap);
#else
	len = vsnprintf(NULL, 0, format, ap2);
#endif
	va_end(ap2);
	if (len < 1) return;

	// Add characters required for logLevelString.
	if (logLevel >= 0 && logLevel < (int)logLevelStringsCount) {
		logLevelStringLen = 3 + strlen(logLevelStrings[logLevel]); // +3 for brackets and a space, e.g. "[debug] ".
	}
	else {
		logLevelStringLen = 0;
	}

	buf = (char *)malloc((logLevelStringLen + len + 1) * sizeof(char)); // +1 for nul-term.

	if (logLevelStringLen > 0) {
		snprintf(buf, logLevelStringLen + 1, "[%s] ", logLevelStrings[logLevel]);
	}

	vsnprintf(buf + logLevelStringLen, len + 1, format, ap);
	len += logLevelStringLen;

	if (fxrLogLoggerCallback) {

		if (!fxrLogLoggerCallBackOnlyIfOnSameThread) {
			(*fxrLogLoggerCallback)(buf);
		}
		else {
#ifndef _WIN32
			if (!pthread_equal(pthread_self(), fxrLogLoggerThread))
#else
			if (GetCurrentThreadId() != fxrLogLoggerThreadID)
#endif
			{
				// On non-log thread, put it into buffer if we can.
				if (fxrLogWrongThreadBuffer && (fxrLogWrongThreadBufferCount < fxrLogWrongThreadBufferSize)) {
					if (len <= (fxrLogWrongThreadBufferSize - (fxrLogWrongThreadBufferCount + 4))) { // +4 to reserve space for "...\0".
						strncpy(&fxrLogWrongThreadBuffer[fxrLogWrongThreadBufferCount], buf, len + 1);
						fxrLogWrongThreadBufferCount += len;
					}
					else {
						strncpy(&fxrLogWrongThreadBuffer[fxrLogWrongThreadBufferCount], "...", 4);
						fxrLogWrongThreadBufferCount = fxrLogWrongThreadBufferSize; // Mark buffer as full.
					}
				}
			}
			else {
				// On log thread, print buffer if anything was in it, then the current message.
				if (fxrLogWrongThreadBufferCount > 0) {
					(*fxrLogLoggerCallback)(fxrLogWrongThreadBuffer);
					fxrLogWrongThreadBufferCount = 0;
				}
				(*fxrLogLoggerCallback)(buf);
			}
		}

	}
	else {
#if defined(__ANDROID__)
		int logLevelA;
		switch (logLevel) {
		case FXR_LOG_LEVEL_REL_INFO:         logLevelA = ANDROID_LOG_ERROR; break;
		case FXR_LOG_LEVEL_ERROR:            logLevelA = ANDROID_LOG_ERROR; break;
		case FXR_LOG_LEVEL_WARN:             logLevelA = ANDROID_LOG_WARN;  break;
		case FXR_LOG_LEVEL_INFO:             logLevelA = ANDROID_LOG_INFO;  break;
		case FXR_LOG_LEVEL_DEBUG: default:   logLevelA = ANDROID_LOG_DEBUG; break;
		}
		__android_log_write(logLevelA, (tag ? tag : "fxr_unity"), buf);
		//#elif defined(_WINRT)
		//            OutputDebugStringA(buf);
#elif defined(__APPLE__)
		if (os_log_create == NULL) { // os_log only available macOS 10.12 / iOS 10.0 and later.
			fprintf(stderr, "%s", buf);
		}
		else {
			os_log_type_t type;
			switch (logLevel) {
			case FXR_LOG_LEVEL_REL_INFO:         type = OS_LOG_TYPE_DEFAULT; break;
			case FXR_LOG_LEVEL_ERROR:            type = OS_LOG_TYPE_ERROR; break;
			case FXR_LOG_LEVEL_WARN:             type = OS_LOG_TYPE_DEFAULT;  break;
			case FXR_LOG_LEVEL_INFO:             type = OS_LOG_TYPE_INFO;  break;
			case FXR_LOG_LEVEL_DEBUG: default:   type = OS_LOG_TYPE_DEBUG; break;
			}
			os_log_with_type(OS_LOG_DEFAULT, type, "%{public}s", buf);
		}
#else
		fprintf(stderr, "%s", buf);
#endif
	}
	free(buf);
}
